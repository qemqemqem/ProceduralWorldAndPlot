using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using CSD;


public class UnityMeshComponent : MonoBehaviour, IComponent, IPathfindingInterface {
	private IEntity entity;
	public bool isUnderDirectControl=false;
	public Resource position = new Resource();
	public NavMeshAgent navmeshAgent;
	public Vector3 desiredPosition;
	public Vector3 closestNavmeshPosition;
	public bool isReachable;
	public bool isNavigating;
	public float reach = .5f;
	public IEntity target;

	// Use this for initialization
	void Start () {
		if(navmeshAgent==null)
			navmeshAgent = gameObject.GetComponent<NavMeshAgent> ();
	}

	void Reset(){
		if (navmeshAgent == null)
			navmeshAgent = gameObject.AddComponent<NavMeshAgent> ();
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		var positionComponent = entity.GetComponent<PositionComponent> ();
		if (isUnderDirectControl||(navmeshAgent!=null&&isNavigating)) {
			positionComponent.position = new Vector2 (transform.position.x, transform.position.z);
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
		return Vector3.Distance(navmeshAgent.destination,transform.position)<reach;
	}
	public void Cancel(){
		isNavigating = false;
		desiredPosition = Vector3.negativeInfinity;
		if (navmeshAgent == null)
			return;
		navmeshAgent.SetDestination (transform.position);
		navmeshAgent.enabled = false;
	}
}
