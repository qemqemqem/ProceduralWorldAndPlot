using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using CSD;
using InControl;

public class UnityView : MonoBehaviour {
	//TODO have methods to create the unity representation of entities given a dataobject entity
	//Have methods to add view specific entities

	public static UnityView viewer;
	//TODO delete this
	public TopDownActionCamera topDownActionCam;
	private static List<TopDownActionCamera> cameras = new List<TopDownActionCamera> ();


	public static Dictionary<Entity, GameObject> displaysMap = new Dictionary<Entity, GameObject> ();
	public GameObject prototype;
	public GameObject cameraPrototype;
	public GameObject mapTilePrefab;
	public int startMapSize = 10;
	public float mapTilePrefabSize=10f;

	public static List<Entity> entities = new List<Entity>();
	public static List<Entity> controllableAgents = new List<Entity>();

	private static Dictionary<InputDevice, HumanPlayer> device2Player = new Dictionary<InputDevice, HumanPlayer> ();
	private static Dictionary<HumanPlayer, Entity> player2Entity = new Dictionary<HumanPlayer, Entity> ();
	//TODO replace with a camera interface
	private static Dictionary<HumanPlayer, TopDownActionCamera> player2Camera = new Dictionary<HumanPlayer, TopDownActionCamera> ();
	private static Dictionary<Entity, ControllableHomonid> entity2Homonid = new Dictionary<Entity, ControllableHomonid> ();

	private static TopDownActions joystickListener;

	//TODO maintain this based on things being added
	private Dictionary<Vector2Int, NavMeshSurface> coordinate2Navmesh = new Dictionary<Vector2Int, NavMeshSurface>();
	private Queue<NavMeshSurface> rebakeQueue = new Queue<NavMeshSurface> ();
	private int maxTilesToBakePerFrame=5;


	private static bool needsToRebake=false;



	// Use this for initialization
	void Awake () {
		viewer = this;
		CreateMap ();
	}

	void Start(){
		if (mapTilePrefab == null)
			return;
		var surface = mapTilePrefab.GetComponent<NavMeshSurface> ();
		if (surface == null)
			return;
		surface.overrideTileSize = true;
		surface.tileSize = 64;
		surface.overrideVoxelSize = true;
		surface.voxelSize = .25f;
	}

	// Update is called once per frame
	void Update () {
		CheckControllerJoin ();
		RemoveTheDead();
		foreach (var player in device2Player.Values) {
			player.Update ();
		}
		int i = 0;
		while (rebakeQueue.Count > 0&&i<maxTilesToBakePerFrame) {
			var navMesh = rebakeQueue.Dequeue ();
			navMesh.BuildNavMesh ();
		}
	}

	public void CreateMap(){
		if (mapTilePrefab == null)
			return;
		for (int i = -startMapSize/2; i <= startMapSize/2; ++i) { 
			for (int j = -startMapSize/2; j <= startMapSize/2; ++j) {
				Vector2Int pos = new Vector2Int (i, j);
				GameObject mapTile = GameObject.Instantiate (mapTilePrefab, transform);
				NavMeshSurface surf = mapTile.GetComponent<NavMeshSurface> ();
				if (surf == null) {
					GameObject.Destroy (mapTile);
					return;
				}
				mapTile.transform.position = new Vector3 (i * mapTilePrefabSize, -.5f, j * mapTilePrefabSize);
				mapTile.SetActive (true);
				coordinate2Navmesh.Add (pos, surf);
				rebakeQueue.Enqueue (surf);
			}
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
		if (entity.HasComponent<AgentComponent> ()) {
			meshComponent.TogglePathfinding (true);
			meshComponent.ToggleCollision (false);
			//DBG appearance code
			var rend = meshComponent.gameObject.GetComponent<Renderer> ();
			rend.material.color = Color.red;
			meshComponent.gameObject.name = "agent";
		} else {
			meshComponent.TogglePathfinding (false);
			meshComponent.ToggleCollision (true);
		}
		
		entity.AddComponent<UnityMeshComponent> (meshComponent);
		//meshComponent.SetEntity (entity);
		displaysMap[entity] = display;
		var pos = entity.GetComponent<PositionComponent> ();
		if (pos == null)
			return;
		Vector2Int coord = new Vector2Int (Mathf.FloorToInt (pos.position.x/viewer.mapTilePrefabSize), Mathf.FloorToInt (pos.position.y/viewer.mapTilePrefabSize));
		if (viewer.coordinate2Navmesh.ContainsKey (coord)) {
			var navmesh = viewer.coordinate2Navmesh [coord];
			viewer.rebakeQueue.Enqueue (navmesh);
		} else {
			Mathf.Sqrt (2f);
		}
		display.SetActive (true);
			


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


	public static void FocusCameraOn(Entity entity, TopDownActionCamera cam){
		if (viewer == null || viewer.topDownActionCam == null || entity == null || !displaysMap.ContainsKey (entity))
			return;
		GameObject view = displaysMap [entity];
		if (view == null)
			return;
		UnityEditor.Selection.activeGameObject = view;
		CameraFocus focus = view.GetComponent<CameraFocus> ();
		if (focus == null)
			return;
		cam.SetFocus (focus);
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
		var camera = GetCamera ();
		var player = new HumanPlayer (this, inputDevice, camera);
		player2Camera.Add (player, camera);
		device2Player.Add (inputDevice, player);
		player.entity = controllableAgents.Count==0?null:GetNextEntity (controllableAgents [0]);
		return player;
	}

	static TopDownActionCamera GetCamera(){
		if (cameras.Count == 0) {
			cameras.Add (viewer.topDownActionCam);
			return viewer.topDownActionCam;
		}
		return AddCamera ();
	}

	static TopDownActionCamera AddCamera(){
		GameObject cameraFocus = GameObject.Instantiate (viewer.cameraPrototype);
		TopDownActionCamera cameraController = cameraFocus.GetComponentInChildren<TopDownActionCamera> ();
		Camera cameraComponent = cameraFocus.GetComponentInChildren<Camera> ();
		cameras.Add (cameraController);
		if (cameras.Count == 2) {
			var mainCam = viewer.topDownActionCam.gameObject.GetComponent<Camera> ();
			mainCam.rect = new Rect (0f, 0f, .5f, 1f);
			cameraComponent.rect = new Rect (.5f, 0f, .5f, 1f);
			//2[|]
		} else {
			//TODO
			//3-4[+]
			//6[-|-|-]
			//8[+|+]
			int numPlayers = 4;
		}
		return cameraController;
	}


	static bool JoinButtonWasPressedOnListener( TopDownActions actions )
	{
		return actions.Primary||actions.Jump||actions.Crouch||actions.Interact||actions.Command||actions.Start;
	}

	public static TopDownActionCamera GetCamera(HumanPlayer player){
		if (player2Camera.ContainsKey (player))
			return player2Camera [player];
		return null;
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
		//dbg code
		UnityEditor.Selection.activeGameObject = availableAgent.gameObject;
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
		//FocusCameraOn (entity);

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
		//FocusCameraOn (entity);
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
