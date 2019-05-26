using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* TODO:
 * Switch this over to using a more generic actoin key binding system
 * Switch this over to ignoring force and doing either kinematic or character controller based movement to prevent the character from getting stuck
 * Switch to having movement be relative to the character's orientation and local gravity so we can do gravity manipulation mechanics and work on other surface types
 * Switch to movement that constrains the character to a surface in most conditions
 * 
 * 
 * 
 * I should parameterize and modify this to allow different types of controls, flying, non-holonimic vehicles, etc.
 * I should make this robust to changing surfaces so that it is relative up as long as there is friction
 * I should make this kinematic and only switch to force based controls under special circumstances
 * I should make the controls re-mappable
 * I should add support for multiple controllers
 * I should add support for mario/zelda styel jumping rules and dodges - b to crouch, a to jump, b and a direction to roll, target lock + those keys to dodge to the sides,
 * hop jump for increased height, change in direction jump for increased height, crouch back jump for increased height
 * Make it so holding A longer changes jump height slightly
 * Make it so there is more limited control in the air
 * Add in ways to change controls slightly (things like levitation vs all out flight)
 * Add in dashes and other moves that override basic controls
 * Add in conditions for overriding such as getting hit by too great a force
 */
namespace CSD
{
	//TODO
	/* 
	 * 
	 * 
	*/


	public enum MOVE_STATE{SURE_FOOTED, SLIDING_GROUND, SLIDING_WALL, JUMPING, FALLING, SWIMING, FLYING, CLIMBING, ANIMATION_DRIVEN};
	public enum STANCE{CROUCHING, STANDING};
	public enum LOOK_MODE{TARGET_LOCK, FREE_LOOK, MOVE_TO_TURN_ARC, MOVE_TO_TURN_SPEED};
	public enum MOVE_MODE{FREE_MOVE, FORWARD_BACKWARD, KEANU_SPEED, LOCKED};

	public class ControllableHomonid : MonoBehaviour, ICameraFocus, IComponent {
		private IEntity entity;
		public List<Resource> resources = new List<Resource>();
		public Transform characterFocus;
		public Transform predictedPosition;
		public MOVE_STATE state=MOVE_STATE.SURE_FOOTED;
		public STANCE stance = STANCE.STANDING;
		public LOOK_MODE lookMode = LOOK_MODE.FREE_LOOK;
		public MOVE_MODE moveMode = MOVE_MODE.FREE_MOVE;
		public double forwardBackSpeed = 10f;
		public double strafeSpeed = 8f;
		public double turnSpeed=1080f;
		public double arcRadius=5f;
		public float lookDistance = 30f;


		public TopDownActions actions;
		public bool underDirectControl = false;
		private float desiredMoveSpeed = 12f;
		private float maxAcceleration = 60f;
		private float maxTurnSpeed = 1080; //degress per second
		public CharacterController cc;
		public Vector2 desiredHorizontalVelocity;
		public Vector2 desiredLookDirection;

		public float distanceFromGround=0f;
		public bool onGround=true;
		public bool onWall = false;
		public float distanceFromWall=0f;
		public bool triggerRoll=false;
		private Vector3 velocity;
		private Vector3 characterFocusVelocity = Vector3.zero;
		//private int layerMask = ~(1 << 11);

		private float timeSinceDodge=0f;
		private float dodgeCooldown=.5f;
		private HumanPlayer player;

		public void AssertControl(TopDownActions actions, HumanPlayer player){
			this.actions = actions;
			this.player = player;
			underDirectControl = true;
			cc = gameObject.GetComponent<CharacterController> ();
			if (cc == null)
				cc = gameObject.AddComponent<CharacterController> ();
			if (characterFocus == null) {
				characterFocus = (new GameObject ()).transform;
				CameraFocus focus = characterFocus.gameObject.AddComponent<CameraFocus> ();
				focus.weight = 1f;
				focus.mustStayOnScreen = true;
				characterFocus.position = transform.position;
			}
			if (player != null) {
				//TODO this is a problem right here....
				player.camera.AddFocus (characterFocus.gameObject.GetComponent<CameraFocus> ());
			}
		}

		public void ReleaseControl(){
			this.actions = null;
			underDirectControl = false;
			player = null;
		}

		// Use this for initialization
		void Start () {
		}

		void FixedUpdate() {
			if (!underDirectControl||actions==null)
				return;


			velocity = cc.velocity;

			desiredHorizontalVelocity = new Vector2 (actions.Move.X, actions.Move.Y);//actions==null?new Vector2 (Input.GetAxisRaw ("Horizontal"), Input.GetAxisRaw ("Vertical")):
			desiredLookDirection = new Vector2 (actions.Rotate.X, actions.Rotate.Y);//= actions==null?new Vector2 (Input.GetAxisRaw ("Horizontal Look"), Input.GetAxisRaw ("Vertical Look")):
			desiredHorizontalVelocity *= desiredMoveSpeed;
			UpdateVelocity ();
			if (desiredLookDirection.magnitude < 1E-4&&desiredHorizontalVelocity.sqrMagnitude > 1E-4) {
				desiredLookDirection = desiredHorizontalVelocity.normalized;
			}
			DebugLogButtons ();
			UpdateOrientation ();
			if (actions.Jump.IsPressed) {
				Jump ();
			}
			timeSinceDodge += Time.deltaTime;
			if (actions.Crouch.IsPressed&&timeSinceDodge>dodgeCooldown) {
				Dodge ();
			}


			UpdateCharacterFocus ();
			UpdateFuturePosition ();
			velocity -= (Vector3.up* 9.8f * Time.deltaTime);
			//velocity = velocity*Mathf.Pow (.9f, Time.deltaTime);
			cc.Move (velocity*Time.deltaTime);
		}


		List<Resource> GetResources(){
			return null;
		}

		private void DebugLogButtons(){
			if (actions.Start.WasPressed) {
				Debug.Log ("Start");
			}
			if (actions.Back.WasPressed) {
				Debug.Log ("Back");
			}
			if (actions.Bonus1.WasPressed) {
				Debug.Log ("RJoy Down");
			}
			if (actions.Bonus2.WasPressed) {
				Debug.Log ("LJoy Down");
			}
			if (actions.SpeechUp.WasPressed) {
				Debug.Log ("Hello (Up)");
			}
			if (actions.SpeechDown.WasPressed) {
				Debug.Log ("Goodbye (Down)");
			}
			if (actions.SpeechLeft.WasPressed) {
				Debug.Log ("What's over there?! (Left)");
			}
			if (actions.SpeechRight.WasPressed) {
				Debug.Log ("Ne (Right)");
			}
			if (actions.Command.WasPressed) {
				Debug.Log ("Everyone listen up! (Y)");
			}
			if (actions.Interact.WasPressed) {
				Debug.Log ("Hey you! Follow me. (X)");
			}
			if (actions.Crouch.WasPressed) {
				Debug.Log ("Crouch");
			}
			if (actions.Jump.WasPressed) {
				Debug.Log ("Jump");
			}
			if (actions.Primary.WasPressed) {
				Debug.Log ("Pew Pew Pew (RB)");
			}
			if (actions.Secondary.WasPressed) {
				Debug.Log ("Block (LB)");
			}
			if (actions.TriggerAction.WasPressed) {
				Debug.Log ("Vrrrrrrrrm (RT)");
			}
			if (actions.LockOn.WasPressed) {
				Debug.Log ("Target Aquired (LT)");
			}




		}

		// Update is called once per frame
		void UpdateVelocity () {
			Vector2 desiredDelta = new Vector3(desiredHorizontalVelocity.x-velocity.x, desiredHorizontalVelocity.y-velocity.z);
			float maxAcceleration = GetMaxAccelerationDirection (desiredDelta);
			if (desiredDelta.magnitude < maxAcceleration * Time.deltaTime) {
				velocity = new Vector3(desiredHorizontalVelocity.x, velocity.y, desiredHorizontalVelocity.y);
				return;
			}
			desiredDelta.Normalize ();
			desiredDelta *= maxAcceleration * Time.deltaTime;
			velocity += new Vector3(desiredDelta.x, 0f, desiredDelta.y);
		}

		private void UpdateOrientation(){
			if (desiredLookDirection.sqrMagnitude < 1E-4)
				return;
			Vector3 currentLookDirection = new Vector3 (transform.forward.x, 0f, transform.forward.z);
			Vector3 normalizedLookDirection = new Vector3(desiredLookDirection.x, 0f, desiredLookDirection.y);
			normalizedLookDirection.Normalize ();
			float degrees = Vector3.Angle (currentLookDirection, normalizedLookDirection);
			float degreesThisFrame = cc.isGrounded?maxTurnSpeed*Time.deltaTime:maxTurnSpeed*Time.deltaTime*.05f;
			if (degrees <= (degreesThisFrame) || degrees <= 0f) {
				transform.rotation = Quaternion.LookRotation (normalizedLookDirection);
			} else {
				transform.rotation = Quaternion.LookRotation (Vector3.Slerp (currentLookDirection, normalizedLookDirection, degreesThisFrame / degrees));
			}
		}

		private void UpdateCharacterFocus(){
			if (characterFocus == null)
				return;
			Vector3 desiredOffset = new Vector3 (desiredLookDirection.x, 0f, desiredLookDirection.y) * lookDistance;
			float weight = Mathf.Pow (.001f, Time.deltaTime);
			characterFocus.transform.position = Vector3.Lerp(characterFocus.transform.position,transform.position + desiredOffset,(1f-weight));
		}

		private void UpdateFuturePosition(){
			if (predictedPosition == null)
				return;
			characterFocus.transform.localPosition = cc.velocity*1f;
		}

		private void Jump(){
			if (cc.isGrounded) {
				float ups = 10.6f - Mathf.Abs(desiredHorizontalVelocity.x) * .3f - Mathf.Abs(desiredHorizontalVelocity.y) * .3f;
				Vector3 jumpDirection = new Vector3 (desiredHorizontalVelocity.x * .3f, Mathf.Max (ups, 6f), desiredHorizontalVelocity.y * .3f);
				velocity += (jumpDirection * .8f);
			} else {
				velocity += Vector3.up*.01f;
			}
		}

		private void Dodge(){
			if (cc.isGrounded) {
				timeSinceDodge = 0f;
				Vector3 dodgeDirection = new Vector3 (desiredHorizontalVelocity.x, 0f, desiredHorizontalVelocity.y);
				velocity += dodgeDirection*2f;
			}
		}

		private float GetMaxAccelerationDirection(Vector2 direction){

			if (cc.isGrounded)
				return maxAcceleration;
			else {
				float speedBasedMultiplier = Mathf.Min(Mathf.Max((desiredMoveSpeed*.5f-Mathf.Sqrt (velocity.x * velocity.x + velocity.z * velocity.z))/desiredMoveSpeed,.05f), 1f);
				return maxAcceleration * speedBasedMultiplier;
			}
		}

		public Vector3 GetFocusPosition(){
			return transform.position;
		}
		public float GetFocusPriority(){
			return 1f;
		}
		public bool MustStayOnScreen (){
			return true;
		}
		public float GetRequiredDistanceFromEdge(){
			return .1f;
		}
		public float GetRadius(){
			return 1f;
		}

		public IEntity GetEntity (){
			return entity;
		}

		public bool SetEntity(IEntity entity){
			if (this.entity != null)
				return false;
			this.entity = entity;
			return true;
		}
	}
}