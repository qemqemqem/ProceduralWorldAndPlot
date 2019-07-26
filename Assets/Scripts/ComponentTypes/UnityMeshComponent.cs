using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using CSD;


public class UnityMeshComponent : MonoBehaviour, IComponent, IPathfindingInterface, IViewHolder {
	private IEntity entity;
	public bool isUnderDirectControl=false;
	public Resource position = new Resource();
	public NavMeshAgent navmeshAgent;
	public NavMeshObstacle obstacle;
	public Vector3 desiredPosition;
	public Vector3 closestNavmeshPosition;
	public bool isReachable;
	public bool isNavigating;
	public float reach = 1.5f;
	public IEntity target;

	// Use this for initialization
	void Start () {
	}

	void Reset(){
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		var positionComponent = entity.GetComponent<PositionComponent> ();
		if (positionComponent == null)
			return;
		var carriableComponent = entity.GetComponent<CarriableComponent> ();
		if (isUnderDirectControl||(navmeshAgent!=null&&isNavigating)||(carriableComponent!=null&&carriableComponent.carrier!=null)) {
			positionComponent.position = new Vector2 (transform.position.x, transform.position.z);
			if(carriableComponent!=null&&carriableComponent.carrier!=null)
				return;
			return;
		}

		if (positionComponent != null) {
			if (float.IsNaN (positionComponent.position.x)) {
				Mathf.Sqrt (2f);
			}
			transform.position = new Vector3 (positionComponent.position.x, 0, positionComponent.position.y);
		}
		var plantComponent = entity.GetComponent<PlantComponent> ();
		if (plantComponent != null)
			transform.localScale = new Vector3 (plantComponent.size, plantComponent.size, plantComponent.size);
	}

	public void TakeControl(){
		isUnderDirectControl = true;
	}

	public void ReleaseControl(){
		isUnderDirectControl = false;
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


	public bool IsBlocked(){
		return navmeshAgent == null || !navmeshAgent.isOnNavMesh || navmeshAgent.pathStatus == NavMeshPathStatus.PathInvalid || (!navmeshAgent.pathPending && navmeshAgent.pathStatus != NavMeshPathStatus.PathComplete);
	}

	public bool IsReachable(){
		return navmeshAgent.path.status == NavMeshPathStatus.PathComplete;
	}

	//TODO implement follow and get line of site methods
	public void SetTartget(IEntity entity){
		target = entity;
	}

	public void SetTarget (Vector2 position){
		if (navmeshAgent == null)
			return;
		navmeshAgent.enabled = true;
		isNavigating = true;
		desiredPosition = new Vector3 (position.x, .5f, position.y);
		navmeshAgent.SetDestination(desiredPosition);
		closestNavmeshPosition = navmeshAgent.destination;
	}

	public bool HasReachedDestination(){
		if (navmeshAgent == null || navmeshAgent.destination.Equals (Vector3.positiveInfinity)) {
			Debug.LogError ("Navmesh Component not set up correctly");
			return false;
		}
		if (Vector3.Distance (navmeshAgent.destination, transform.position) < reach)
			return true;
		else
			return false;
		//return Vector3.Distance(navmeshAgent.destination,transform.position)<reach;
	}
	public void Cancel(){
		isNavigating = false;
		desiredPosition = Vector3.negativeInfinity;
		if (navmeshAgent == null)
			return;
		navmeshAgent.SetDestination (transform.position);
		//navmeshAgent.enabled = false;//TODO fix this...
	}
		
	public void TogglePathfinding (bool value){
		if (!value) {
			if (navmeshAgent == null)
				return;
			Destroy (navmeshAgent);
		} else {
			if (navmeshAgent != null)
				return;
			navmeshAgent = gameObject.AddComponent<NavMeshAgent> ();
		}
	}
	public void ToggleCollision (bool value){
		if (!value) {
			if (obstacle == null)
				return;
			Destroy (obstacle);
		} else {
			if (obstacle != null)
				return;
			obstacle = gameObject.AddComponent<NavMeshObstacle> ();
			obstacle.carving = true;
			obstacle.carveOnlyStationary = false;
		}		
	}

	public void HoldThing (IEntity entity){
		var mc = entity.GetComponent<UnityMeshComponent> ();
		if (mc == null)
			return;
		mc.transform.SetParent (gameObject.transform);
		mc.transform.localPosition = mc.transform.localPosition + Vector3.up*.1f;
		mc.isUnderDirectControl = true;
		var oc = mc.GetComponent<NavMeshObstacle> ();
		//Debug code
		var renderer = gameObject.GetComponent<Renderer>();
		renderer.material.color = Color.green;
		var renderer2 = mc.transform.gameObject.GetComponent<Renderer>();
		renderer2.material.color = Color.blue;
		//end debug code
		if (oc == null)
			return;
		oc.enabled = false;
	}

	public void DropThing (IEntity entity){
		Debug.Break ();
		var mc = entity.GetComponent<UnityMeshComponent> ();
		if (mc == null)
			return;
		mc.transform.localPosition = mc.transform.localPosition - Vector3.up*.1f;
		mc.transform.SetParent (null);
		var renderer = gameObject.GetComponent<Renderer>();
		renderer.material.color = Color.red;
		var renderer2 = mc.transform.gameObject.GetComponent<Renderer>();
		renderer2.material.color = Color.white;
		var oc = mc.GetComponent<NavMeshObstacle> ();
		if (oc == null)
			return;
		oc.enabled = true;
		mc.isUnderDirectControl = false;
		//TODO add a place method to the tile and rebake the navmesh
	}
}
