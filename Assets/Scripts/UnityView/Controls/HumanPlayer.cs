using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InControl;


namespace CSD
{

	public enum ControlState{ACTION_GAME, BUILDING_GAME};

	public class HumanPlayer{
		private UnityView view;
		private InputDevice inputDevice;
		public ControllableHomonid homonid;
		public TopDownActionCamera camera;
		private TopDownActions actions;
		public Entity entity;
		private ControlState state = ControlState.BUILDING_GAME;
		private BuildCursor cursor;
		private bool following = false;
		private float zoomSpeedPerSecond = 4f;


		public HumanPlayer(UnityView view, InputDevice inputDevice, TopDownActionCamera camera){
			this.view = view;
			this.inputDevice = inputDevice;
			actions = TopDownActions.CreateWithJoystickBindings();
			actions.Device = inputDevice;
			GameObject cursorObject = new GameObject ();
			CameraFocus focus = cursorObject.AddComponent<CameraFocus> ();
			focus.weight = 1f;
			cursor = cursorObject.AddComponent<BuildCursor> ();
			this.camera = camera;
			camera.SetFocus (cursor.gameObject.GetComponent<CameraFocus>());
			cursor.AssertControl (actions, this);
		}

		public InputDevice GetInputDevice(){
			return this.inputDevice;
		}

		public void TakeControlOf(ControllableHomonid homonid){
			this.homonid = homonid;
			homonid.AssertControl (actions, this);
		}

		public void StopFollowingHomonid(){
			homonid = null;
		}

		public void Update(){
			switch (state) {
			case ControlState.ACTION_GAME:
				UpdateActionControls ();
				break;
			case ControlState.BUILDING_GAME:
				UpdateBuildControls ();
				break;
			}
		}

		public void UpdateActionControls(){
			if (homonid == null)
				return;
			if (actions.Back.WasPressed) {
				//TODO release should go through view so we can make the entity resume it's previous behavior
				UnityView.ReleaseHumanoid(this);
				homonid.ReleaseControl ();
				state = ControlState.BUILDING_GAME;
				cursor.transform.position = homonid.transform.position;
				cursor.AssertControl (actions, this);
				UnityView.viewer.topDownActionCam.SetFocus (cursor.gameObject.GetComponent<CameraFocus>());
				following = true;
			}
		}

		public void UpdateBuildControls(){
			
			if (following) {
				if (cursor != null && entity != null) {
					var homonid = UnityView.GetHomonid (entity);
					if(homonid!=null)
						cursor.transform.position = homonid.transform.position;
				}
				if (actions.Move.Vector.magnitude>1E-4) {
					following = false;
					UnityView.viewer.topDownActionCam.SetFocus (cursor.gameObject.GetComponent<CameraFocus>());
				}
			}

			if (actions.Back.WasPressed) {
				if (entity != null) {
					state = ControlState.ACTION_GAME;
					cursor.ReleaseControl ();
					UnityView.ControlHumanoid (this);
				}
			}
			if (actions.SpeechUp.WasPressed) {
				if (entity != null) {
					state = ControlState.ACTION_GAME;
					cursor.ReleaseControl ();
					UnityView.ControlHumanoid (this);
				}
			}
			if (actions.SpeechLeft.WasPressed) {
				//previous entity, move to and lock on until the camera is panned
			}
			if (actions.SpeechRight.WasPressed) {
				//next entity, move to and lock on until the camera is panned
			}
			if (actions.Primary.WasPressed) {
				entity = UnityView.GetNextEntity (entity);
				//TODO fix this
				UnityView.FocusCameraOn (entity, camera);
				following = true;
			}

			if (actions.Secondary.WasPressed) {
				entity = UnityView.GetPrevEntity (entity);
				//TODO fix this
				UnityView.FocusCameraOn (entity, camera);
				following = true;
			}

			if (actions.TriggerAction) {
				TopDownActionCamera cam = UnityView.GetCamera (this);
				if (cam == null)
					return;
				cam.Zoom (1f / GetZoom ());
				
			}

			if (actions.LockOn) {
				TopDownActionCamera cam = UnityView.GetCamera (this);
				if (cam == null)
					return;
				cam.Zoom (GetZoom ());

			}
		}

		private float GetZoom(){
			return Mathf.Pow (zoomSpeedPerSecond, Time.deltaTime);
		}

		bool JoinButtonWasPressedOnListener( TopDownActions actions )
		{
			return actions.Primary.WasPressed||actions.Jump||actions.Crouch||actions.Interact||actions.Command;
		}

		public void ReleaseControl(){
			if (homonid == null)
				return;
			homonid.ReleaseControl ();
		}
	}

}