using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* TODO: Modify this to work with a perspective camera.  Add in camera pivot and zoom
 * 
 */


public delegate Vector3 object2Position<T>(T obj);
public delegate float object2Priority<T>(T obj);

public interface ICameraFocus{
	Vector3 GetFocusPosition();
	float GetFocusPriority();
	bool MustStayOnScreen ();
	float GetRequiredDistanceFromEdge();
	float GetRadius();
}

public abstract class BasicFocus : ICameraFocus {
	readonly private bool _mustStayOnScreen=false;
	readonly private float _focusPriority=1f;

	public BasicFocus(){
	}

	public BasicFocus(bool mustStayOnScreen, float focusPriority){
		_mustStayOnScreen = mustStayOnScreen;
		_focusPriority = focusPriority;
	}

	public abstract Vector3 GetFocusPosition();

	public float GetFocusPriority(){
		return _focusPriority;
	}

	public bool MustStayOnScreen(){
		return _mustStayOnScreen;
	}

	public float GetRequiredDistanceFromEdge(){
		return 0f;
	}

	public float GetRadius(){
		return 0f;
	}
}

public class RigidbodyFocus : BasicFocus {
	readonly private Rigidbody _rigidbody;
	readonly private float _temporalLookahead=.2f;
	readonly private float _lookaheadPriority=1f;
	private Vector3 projectedDisplacementAverage=Vector3.zero;

	public RigidbodyFocus(Rigidbody rigidbody){
		_rigidbody = rigidbody;
	}

	public RigidbodyFocus(Rigidbody rigidbody, float temporalLookahead, bool mustStayOnScreen, float lookaheadPriority, float focusPriority) : base(mustStayOnScreen, focusPriority) {
		_rigidbody = rigidbody;
		_temporalLookahead = temporalLookahead;
		_lookaheadPriority = lookaheadPriority;
	}

	public override Vector3 GetFocusPosition(){
		float AVERAGE_CONSTANT = .9f*Time.deltaTime;//TODO make this interpolation better
		projectedDisplacementAverage = projectedDisplacementAverage*(1-AVERAGE_CONSTANT)+_rigidbody.velocity * _temporalLookahead * _lookaheadPriority * AVERAGE_CONSTANT;
		Vector3 vec3Displacement = new Vector3 (projectedDisplacementAverage.x, 0f, projectedDisplacementAverage.y);
		return _rigidbody.transform.position + vec3Displacement;
	}
}

public class MouseFocus : BasicFocus{
	private readonly Camera _camera;
	private Vector3 mouseDisplacementAverage=Vector3.zero;

	public MouseFocus(Camera camera, bool mustStayOnScreen, float focusPriority) : base(mustStayOnScreen, focusPriority) {
		_camera = camera;
	}

	public override Vector3 GetFocusPosition(){
		float AVERAGE_CONSTANT = .99f*Time.deltaTime;//Mathf.Pow(.001f,Time.deltaTime);//TODO make this interpolation better
		Vector3 cameraXY = new Vector3 (_camera.transform.position.x, _camera.transform.position.y, 0f);
		mouseDisplacementAverage = mouseDisplacementAverage*(1-AVERAGE_CONSTANT)+(_camera.ScreenToWorldPoint (new Vector3 (Input.mousePosition.x, Input.mousePosition.y, 0))-cameraXY)*AVERAGE_CONSTANT;		
		return cameraXY+mouseDisplacementAverage;
	}
}

public interface IGround {
	Vector3 GetAverageGroundPosition<T> (List<T> objects, object2Position<T> obj2Pos, object2Priority<T> obj2Pri);
	Vector3 GetUp(Vector3 position);
	Vector2 GetProjectedDirection(Vector3 position, Vector3 orienation);
	Vector2 GetPosition(Vector3 position);
}


public class TransformBasedPlane : IGround{
	Transform transform;

	public TransformBasedPlane(Transform transform){
		this.transform = transform;
	}
	public Vector3 GetAverageGroundPosition<T>(List<T> objects, object2Position<T> obj2Pos, object2Priority<T> obj2Pri){
		if (objects == null || objects.Count == 0 || obj2Pos == null || obj2Pri == null)
			return Vector3.zero;
		float x=0f;
		float y=0f;
		float z=0f;
		float total = 0f;
		foreach (var obj in objects) {
			Vector3 vector = GetPosition(obj2Pos.Invoke (obj));
			float priority = obj2Pri.Invoke (obj);
			total += priority;
			x += vector.x*priority;
			y += vector.y*priority;
			z += vector.z*priority;
		}
		x/=total;
		y/=total;
		z/=total;
		return new Vector3 (x, y, z);
	}
	public Vector3 GetUp(Vector3 position){
		return transform.up;
	}
	public Vector2 GetProjectedDirection(Vector3 position, Vector3 orientation){
		float x = Vector3.Dot (orientation, transform.right);
		float y = Vector3.Dot (orientation, transform.forward);
		return new Vector2 (x, y);
	}
	public Vector2 GetPosition(Vector3 position){
		Vector3 localPosition = position - transform.position;
		return new Vector2(localPosition.x, localPosition.z);	
	}
}

//TODO I need to add smoothing and impose the constraints
//I need to make it so the camera pulls back if the focuses are hitting the edge constraints
//I need to make it so the camera zooms toward the preferred size if the focuses are closer than the edge contraints
public class TopDownShooterCamera : MonoBehaviour {
	//ground should be a surface and have some method for getting the center in surface coordinates of the various objects projected down onto the surface...
	//ground should have some method for getting the surface constrained forward/backward left/right motion of the camera
	//ground should have some method for getting the 
	public Camera camera;
	public Transform groundTransform;
	private IGround ground;

	private List<ICameraFocus> foci = new List<ICameraFocus> ();
	public Vector3 worldCursorPosition;
	public Vector3 focusCenter;
	public Vector3 desiredPosition;
	public List<Rigidbody> players = new List<Rigidbody>();
	public float maxSize=20f;//the maximum numver of tiles on screen in the smaller dimension
	public float preferredSize=5f;//the desired number of tiles on screen in the smaller dimension
	public float minSize=2f;//the minimum number of tiles on screen in the smaller dimension
	public float minBorderAbsolute=.5f;//there must be a .5 tile buffer between units and the edge of the screen
	public float minBorderRelative=.1f;//in focus units must stay 10% in from the edges of the screen
	public int minBorderPixels=50;//the minimum number of pixels between the player and the edge of the screen
	public float timeLookAhead=.1f;

	public float cameraMaxSpeed;
	public float cameraSpeed;





	// Use this for initialization
	void Start () {
		ground = new TransformBasedPlane (groundTransform);
		if (camera == null)
			camera = Camera.main;
		foci.Add (new MouseFocus (camera, true, 1f));
		foreach (Rigidbody rb in players) {
			foci.Add (new RigidbodyFocus (rb, .2f, true, 1f, 2f));
		}
	}
	
	// Update is called once per frame
	void Update () {
		desiredPosition = GetDesiredPosition ();
		Vector3 delta = new Vector3(desiredPosition.x - camera.transform.position.x, desiredPosition.y-camera.transform.position.y, 0);
		float distance = delta.magnitude;
		delta.Normalize ();
		distance = distance * Mathf.Pow (.2f, Time.deltaTime);
		delta *= distance;
		//if camera position > max world distance set it to delta scaled to max world distance
		//otherswise use greater of max deltaV or 


		camera.transform.position = new Vector3(camera.transform.position.x+delta.x, camera.transform.position.y+delta.y, camera.transform.position.z);
	}

	private Vector3 GetDesiredPosition(){
		desiredPosition =weightedAveragePosition<ICameraFocus> (foci, f => f.GetFocusPosition (), f => f.GetFocusPriority ());
		return desiredPosition;
	}

	public void Add(ICameraFocus focus){
		foci.Add (focus);
	}

	Vector3 weightedAveragePosition<T>(List<T> objects, object2Position<T> obj2Pos){
		return weightedAveragePosition (objects, obj2Pos, f=>1f);
	}

	Vector3 weightedAveragePosition<T>(List<T> objects, object2Position<T> obj2Pos, object2Priority<T> obj2Pri){
		if (objects == null || objects.Count == 0 || obj2Pos == null || obj2Pri == null)
			return Vector3.zero;
		float x=0f;
		float y=0f;
		float z=0f;
		float total = 0f;
		foreach (var obj in objects) {
			Vector3 vector = obj2Pos.Invoke (obj);
			float priority = obj2Pri.Invoke (obj);
			total += priority;
			x += vector.x*priority;
			y += vector.y*priority;
			z += vector.z*priority;
		}
		x/=total;
		y/=total;
		z/=total;
		return new Vector3 (x, y, z);
	}
}
