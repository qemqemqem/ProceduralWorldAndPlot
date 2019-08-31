using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InControl;


namespace CSD
{

	//TODO this should be modified so that the player is just data, a player manager should be responsible for adding and removing players, updating the cameras and UI and providing entities to control
	public enum ControlState{ACTION_GAME, BUILDING_GAME, ENTRY_STATE};

	public class HumanPlayer{
		private UnityView view;
		private InputDevice inputDevice;
		public ControllableHomonid homonid;
		public TopDownActionCamera camera;
		private TopDownActions actions;
		public Entity entity;
		private ControlState state = ControlState.ENTRY_STATE;
		private BuildCursor cursor;
		private Transform cursorContent;
		private bool following = false;
		private float zoomSpeedPerSecond = 4f;

		//TODO switch to this constructor and move this stuff to a player state that manages controls and selections and whatnot
		public HumanPlayer(){
		}

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
			SetState (ControlState.BUILDING_GAME);
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
				//TODO open menu and pass in input device to build menu
				UpdateBuildControls ();
				break;
			}
		}

		public void UpdateActionControls(){
			if (homonid == null)
				return;
			if (actions.Back.WasPressed) {
				SetState (ControlState.BUILDING_GAME);
			}
		}

		public void SetState(ControlState state){
			if (state == this.state)
				return;
			if (state == ControlState.BUILDING_GAME) {
				UnityView.ReleaseHumanoid (this);
				if (homonid != null) {
					homonid.ReleaseControl ();
					cursor.transform.position = homonid.transform.position;
				}
				state = ControlState.BUILDING_GAME;
				cursor.AssertControl (actions, this);
				UnityView.viewer.topDownActionCam.SetFocus (cursor.gameObject.GetComponent<CameraFocus> ());
				following = true;
				//CREATE BUILD MENu
				List<MenuButton> buttons = new List<MenuButton>();
				foreach (var building in UnityView.viewer.buildings) {
					MenuButton button = new MenuButton ();
					button.text = building.name;
					button.onClick = () => {
						if(cursorContent!=null){
							Transform newThing = Transform.Instantiate(cursorContent);
							newThing.position = cursorContent.position;
							newThing.rotation = cursorContent.rotation;
						}
					};
					button.onFocus = () => {
						if(cursorContent!=null)
							GameObject.Destroy(cursorContent.gameObject);
						Transform newBuilding = Transform.Instantiate(building);
						cursorContent = newBuilding;
						newBuilding.SetParent(cursor.transform);
						newBuilding.localPosition = Vector3.zero;
					};
					buttons.Add (button);
				}
				UnityView.viewer.menu.SetupMenu (inputDevice, buttons);
			} else if (state == ControlState.ACTION_GAME) {
				if (entity != null) {
					state = ControlState.ACTION_GAME;
					cursor.ReleaseControl ();
					UnityView.ControlHumanoid (this);
				}
			} else {
				Debug.LogError ("What the fuck state is this you idiot!?!?! " + state);
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
				SetState (ControlState.ACTION_GAME);
			}
			if (actions.SpeechUp.WasPressed) {
				SetState (ControlState.ACTION_GAME);

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

	//This is what we'll use to manage the differnt game states for different players
	public class PlayerState{
		//TODO make variants here and have a way to transition between them...
		//should track the camera, what we control, menus and UI
		//should track stackable states when applicable (i.e. aiming, steering a projectile etc.)
		public void Update(){
		}
	}

/*	public class PlayerManager{
		private Dictionary<InputDevice, HumanPlayer> device2Player = new Dictionary<InputDevice, HumanPlayer> ();
		private ControllerActions controllerListener;

		public PlayerManager(){
			controllerListener = ControllerActions.CreateWithJoystickBindings ();
		}

		public void Update(){
			CheckControllerJoin ();
		}

		void CheckControllerJoin()
		{
			var inputDevice = InputManager.ActiveDevice;
			if (JoinButtonWasPressedOnListener( controllerListener ))
			{
				if (IsInputDeviceInUse( inputDevice ))
				{
					var player = CreatePlayer( inputDevice );
				}
			}
		}

		private bool JoinButtonWasPressedOnListener( ControllerActions actions )
		{
			return actions.Start || actions.A;
		}

		private bool IsInputDeviceInUse( InputDevice inputDevice )
		{
			return FindPlayerUsingJoystick( inputDevice ) == null;
		}

		private HumanPlayer FindPlayerUsingJoystick( InputDevice inputDevice )
		{
			if (device2Player.ContainsKey (inputDevice))
				return device2Player [inputDevice];
			else
				return null;
		}

		private HumanPlayer CreatePlayer(InputDevice inputDevice){
			var player = new HumanPlayer ();
			return player;
		}
	}*/

}