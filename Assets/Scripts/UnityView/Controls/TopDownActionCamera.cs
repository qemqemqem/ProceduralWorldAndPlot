using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TopDownActionCamera : MonoBehaviour {
	public Transform cameraFocus;
	public Rigidbody focusRigidbody;
	public Camera mainCamera;
	public List<CameraFocus> foci = new List<CameraFocus> ();
	public Transform debugObject;
	public float cameraMoveSpeed = 100f;
	private Vector3 velocity;
	public float cameraDeltaDelay = 3f;
	public float cameraCriticalDistance=.2f;
	public float minDistance=5f;
	public float maxDistance=50f;
	public float minFOV=20f;
	public float maxFOV=110f;
	public float ratioBorder=.1f;


	//need to add pivoting around vb style
	//need to add zooming in and out with max velocity as a function of height so that the ammount of stuff on screen increases at a constant factor
	//private int pixelBorder;
	//private float zenithAngle=45;
	//private float radialAngle=0;


	// Use this for initialization
	void Start () {
		if(cameraFocus==null)
			cameraFocus = transform.parent;
		if (mainCamera == null)
			mainCamera = GetComponent<Camera> ();
		if (mainCamera == null || cameraFocus == null)
			return;
		//pixelBorder = Mathf.RoundToInt(Mathf.Min (mainCamera.scaledPixelHeight, mainCamera.scaledPixelWidth)*ratioBorder);
	}



	public void SetFocus(CameraFocus focus){
		foci.Clear();
		AddFocus (focus);
	}

	public void AddFocus(CameraFocus focus){
		foci.Add (focus);
	}

	public void SetFoci(List<CameraFocus> foci){
		this.foci = foci;
	}

	
	// Update is called once per frame
	void FixedUpdate () {
		//interpolate the camera focus to be the weighted average of the variou foci
		//set decide screen size based on minimum size that fits all the required things
		UpdateFocusPosition();

	}

	private void UpdateFocusPosition(){
		Vector3 desiredChange = weightedAverage(foci)-cameraFocus.transform.position;
		float desiredChangeMagnitude = desiredChange.magnitude*Time.deltaTime*.9f;//Mathf.Pow(.99f, Time.deltaTime)
		focusRigidbody.MovePosition (focusRigidbody.position+Vector3.Lerp (Vector3.zero, desiredChange, desiredChangeMagnitude));


		//cameraFocus.transform.position = Vector3.Lerp (cameraFocus.transform.position, weightedAverage (foci), 1f-Mathf.Pow(.9f,Time.deltaTime));
		//debugObject.transform.position = weightedAverage (foci);
	}

	private void UpdateFOV(){
		
	}

	private void UpdateDistance(){
		float distance = transform.localPosition.magnitude;
	}

	private Vector3 weightedAverage(List<CameraFocus> foci){
		float totalWeight = 0;
		Vector3 average = Vector3.zero;
		foreach (var focus in foci){
			totalWeight += focus.weight;
			average += (focus.transform.position * focus.weight);
		}
		if (totalWeight == 0)
			return Vector3.zero;
		average=average*(1f / totalWeight);
		return average;
	}

	public void Zoom(float ammount){
		float distance = transform.localPosition.magnitude;
		Vector3 newLocalPosition = (transform.localPosition * ammount);
		transform.localPosition = newLocalPosition;
	}

}
