using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CSD{

	public class BuildCursor : MonoBehaviour, ICameraFocus {
		public TopDownActions actions;
	    public HumanPlayer player;
		public bool underDirectControl = false;
		private float desiredMoveSpeed = 50f;
		private float maxAcceleration = 200f;
		private float maxTurnSpeed = 1080; //degress per second
		private Vector3 velocity;

		private Vector2 desiredHorizontalVelocity;
		private Vector2 desiredLookDirection;


		// Use this for initialization
		void Start () {
			
		}
		
		// Update is called once per frame
		void Update () {
			
		}

		public void AssertControl(TopDownActions actions, HumanPlayer player){
			this.actions = actions;
			this.player = player;
			underDirectControl = true;
		}

		public void ReleaseControl(){
			this.actions = null;
			underDirectControl = false;
			player = null;
		}

		void FixedUpdate() {
			if (!underDirectControl||actions==null)
				return;

			desiredHorizontalVelocity = new Vector2 (actions.Move.X, actions.Move.Y);//actions==null?new Vector2 (Input.GetAxisRaw ("Horizontal"), Input.GetAxisRaw ("Vertical")):
			desiredLookDirection = new Vector2 (actions.Rotate.X, actions.Rotate.Y);//= actions==null?new Vector2 (Input.GetAxisRaw ("Horizontal Look"), Input.GetAxisRaw ("Vertical Look")):
			desiredHorizontalVelocity *= desiredMoveSpeed;
			UpdateVelocity ();
			UpdateOrientation ();
			transform.position += velocity * Time.deltaTime;
		}

		// Update is called once per frame
		void UpdateVelocity () {
			Vector2 desiredDelta = new Vector3(desiredHorizontalVelocity.x-velocity.x, desiredHorizontalVelocity.y-velocity.z);
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
			float degreesThisFrame = maxTurnSpeed*Time.deltaTime;
			if (degrees <= (degreesThisFrame) || degrees <= 0f) {
				transform.rotation = Quaternion.LookRotation (normalizedLookDirection);
			} else {
				transform.rotation = Quaternion.LookRotation (Vector3.Slerp (currentLookDirection, normalizedLookDirection, degreesThisFrame / degrees));
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
	}

}