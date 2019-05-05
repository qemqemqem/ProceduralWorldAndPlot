using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CSD;
using InControl;

public class ViewTest : MonoBehaviour {
	public static ViewTest viewer;
	public TopDownActionCamera topDownActionCam;
	public static Dictionary<Entity, GameObject> displaysMap = new Dictionary<Entity, GameObject> ();
	public GameObject prototype;

	public static List<Entity> entities = new List<Entity>();
	public static List<Entity> controllableAgents = new List<Entity>();

	private static Dictionary<InputDevice, HumanPlayer> device2Player = new Dictionary<InputDevice, HumanPlayer> ();
	private static Dictionary<HumanPlayer, Entity> player2Entity = new Dictionary<HumanPlayer, Entity> ();
	private static Dictionary<Entity, ControllableHomonid> entity2Homonid = new Dictionary<Entity, ControllableHomonid> ();

	private static TopDownActions joystickListener;


	// Use this for initialization
	void Awake () {
		viewer = this;
	}

	// Update is called once per frame
	void Update () {
		CheckControllerJoin ();
		RemoveTheDead();
		foreach (var player in device2Player.Values) {
			player.Update ();
		}
	}

	public void RemoveTheDead() {
		foreach (var entity in entities) {
			if (entity.IsDestroyed()) {
				if (displaysMap.ContainsKey (entity)) {
					Destroy(displaysMap[entity]);
					displaysMap.Remove (entity);
				}
				if(entity2Homonid.ContainsKey(entity))
					entity2Homonid.Remove (entity);

			}
		}
		entities.RemoveAll(x => x.IsDestroyed());
		foreach (var player in player2Entity.Keys) {
			if (player.entity.IsDestroyed ()) {
				player.ReleaseControl ();
				player.entity = GetNextEntity (player.entity);
			}
		}
	}

	public static void AddEntity(Entity entity) {
		entities.Add(entity);
		GameObject display = GameObject.Instantiate(viewer.prototype);
		UnityMeshComponent meshComponent = display.AddComponent<UnityMeshComponent>();
		meshComponent.SetEntity (entity);
		displaysMap[entity] = display;
	}

	public static void RegisterControllableAgent(Entity entity){
		if (viewer==null||controllableAgents == null||entity2Homonid==null)
			return;
		if (!entity.HasComponent<HumanoidAI> ()||!entity.HasComponent<AgentComponent> ())
			return;
		if (viewer == null || viewer.topDownActionCam == null || entity == null || !displaysMap.ContainsKey (entity))
			return;
		GameObject view = displaysMap [entity];
		var ch = view.GetComponent<ControllableHomonid> ();
		if(ch==null){
			ch = view.AddComponent<ControllableHomonid> ();
		}
		if(!controllableAgents.Contains(entity))
			controllableAgents.Add (entity);
		entity2Homonid.Add (entity, ch);
	}


	public static void FocusCameraOn(Entity entity){
		if (viewer == null || viewer.topDownActionCam == null || entity == null || !displaysMap.ContainsKey (entity))
			return;
		GameObject view = displaysMap [entity];
		if (view == null)
			return;
		CameraFocus focus = view.GetComponent<CameraFocus> ();
		if (focus == null)
			return;
		viewer.topDownActionCam.SetFocus (focus);
	}

	void OnEnable()
	{

		InputManager.OnDeviceDetached += OnDeviceDetached;
		joystickListener = TopDownActions.CreateWithJoystickBindings();
	}


	void OnDisable()
	{
		InputManager.OnDeviceDetached -= OnDeviceDetached;
		joystickListener.Destroy();
	}

	void CheckControllerJoin()
	{
		var inputDevice = InputManager.ActiveDevice;
		if (JoinButtonWasPressedOnListener( joystickListener ))
		{
			
			if (ThereIsNoPlayerUsingJoystick( inputDevice ))
			{
				var player = CreatePlayer( inputDevice );
				//ControlHumanoid (player);
			}
		}
	}

	HumanPlayer CreatePlayer(InputDevice inputDevice){
		var player = new HumanPlayer (this, inputDevice);
		device2Player.Add (inputDevice, player);
		player.entity = controllableAgents.Count==0?null:GetNextEntity (controllableAgents [0]);
		//if(player.entity!=null)
		//	FocusCameraOn (player.entity);
		return player;
	}


	static bool JoinButtonWasPressedOnListener( TopDownActions actions )
	{
		return actions.Primary||actions.Jump||actions.Crouch||actions.Interact||actions.Command||actions.Start;
	}


	public static HumanPlayer FindPlayerUsingJoystick( InputDevice inputDevice )
	{
		if (device2Player.ContainsKey (inputDevice))
			return device2Player [inputDevice];
		else
			return null;
	}

	public static ControllableHomonid GetHomonid(Entity entity){
		if (entity == null || !entity2Homonid.ContainsKey (entity))
			return null;
		return entity2Homonid [entity];
	}


	bool ThereIsNoPlayerUsingJoystick( InputDevice inputDevice )
	{
		return FindPlayerUsingJoystick( inputDevice ) == null;
	}

	static void OnDeviceDetached( InputDevice inputDevice )
	{
		var player = FindPlayerUsingJoystick( inputDevice );
		if (player != null)
		{
			RemovePlayer( player );
		}
	}

	public static Entity GetNextEntity(Entity entity){
		int index = controllableAgents.IndexOf (entity);
		if (index < 0 || index > controllableAgents.Count)
			return null;
		for (int i = index + 1; i % controllableAgents.Count != index; ++i) {
			i = (i + controllableAgents.Count) % controllableAgents.Count;
			var nextEntity = controllableAgents [i%controllableAgents.Count];
			if (entity2Homonid.ContainsKey (nextEntity) && !player2Entity.ContainsValue (nextEntity))
				return nextEntity;
		}
		return null;
	}
	public static Entity GetPrevEntity(Entity entity){
		int index = controllableAgents.IndexOf (entity);
		if (index < 0 || index > controllableAgents.Count)
			return null;
		for (int i = index - 1; i % controllableAgents.Count != index; --i) {
			i = (i + controllableAgents.Count) % controllableAgents.Count;
			var prevEntity = controllableAgents [i%controllableAgents.Count];
			if (entity2Homonid.ContainsKey (prevEntity) && !player2Entity.ContainsValue (prevEntity))
				return prevEntity;
		}
		return null;
	}

	public static void ControlHumanoid( HumanPlayer player) {
		if (player==null||player.entity==null||!entity2Homonid.ContainsKey (player.entity) || player2Entity.ContainsValue (player.entity))
			return;
		ControllableHomonid availableAgent = entity2Homonid [player.entity];
		if (availableAgent == null || player == null || player.GetInputDevice()==null)
			return;
		var inputDevice = player.GetInputDevice ();
		Debug.Log ("asserting controll");
		if (inputDevice != null){
			foreach (var entry in displaysMap) {
				if (entry.Value == availableAgent.gameObject)
					TakeControlOf (entry.Key);
			}
			player.TakeControlOf (availableAgent);
		}
	}


	public static void TakeControlOf(Entity entity){
		//TODO add an appropriate controller for the specific entity type
		if (!entity.HasComponent<HumanoidAI> ()||!entity.HasComponent<AgentComponent> ())
			return;
		if (viewer == null || viewer.topDownActionCam == null || entity == null || !displaysMap.ContainsKey (entity))
			return;
		var ac = entity.GetComponent<AgentComponent> ();
		ac.TakeControl ();
		GameObject view = displaysMap [entity];
		var pc = view.GetComponent<UnityMeshComponent> ();
		if(pc==null){
			pc = view.AddComponent<UnityMeshComponent> ();
		}
		pc.TakeControl ();
		//TODO set the next key press to take control and or use the already active player controller
		FocusCameraOn (entity);

	}

	public static void ReleaseHumanoid( HumanPlayer player) {
		if (player==null||player.entity==null||!entity2Homonid.ContainsKey (player.entity) || player2Entity.ContainsValue (player.entity))
			return;
		ControllableHomonid availableAgent = entity2Homonid [player.entity];
		if (availableAgent == null || player == null || player.GetInputDevice()==null)
			return;
		var inputDevice = player.GetInputDevice ();
		Debug.Log ("releasing controll");
		player2Entity.Remove (player);
		ReleaseControlOf (player.entity);

	}


	public static void ReleaseControlOf(Entity entity){
		//TODO add an appropriate controller for the specific entity type
		if (!entity.HasComponent<HumanoidAI> ()||!entity.HasComponent<AgentComponent> ())
			return;
		if (viewer == null || viewer.topDownActionCam == null || entity == null || !displaysMap.ContainsKey (entity))
			return;
		var ac = entity.GetComponent<AgentComponent> ();
		ac.ReleaseControl ();
		GameObject view = displaysMap [entity];
		var pc = view.GetComponent<UnityMeshComponent> ();
		if(pc==null){
			pc = view.AddComponent<UnityMeshComponent> ();
		}
		pc.ReleaseControl ();
		//TODO set the next key press to take control and or use the already active player controller
		FocusCameraOn (entity);
	}

	private static ControllableHomonid GetAvailableHomonid(){
		HashSet<ControllableHomonid> controlledHomonids = new HashSet<ControllableHomonid> ();
		foreach (var player in device2Player.Values) {
			if (player.homonid != null)
				controlledHomonids.Add (player.homonid);
		}
		foreach (var entity in controllableAgents) {
			if (player2Entity.ContainsValue(entity))
				continue;
			return entity2Homonid[entity];
		}
		return null;
	}


	public static void RemovePlayer( HumanPlayer player )
	{
		player.ReleaseControl ();
		device2Player.Remove (player.GetInputDevice());
		player2Entity.Remove (player);
	}
}//*/
