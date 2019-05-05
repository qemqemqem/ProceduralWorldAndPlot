using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CSD;


public class UnityMeshComponent : MonoBehaviour, IComponent {
	private IEntity entity;
	public bool isUnderDirectControl=false;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		var positionComponent = entity.GetComponent<PositionComponent> ();
		if (isUnderDirectControl) {
			positionComponent.position = new Vector2 (transform.position.x, transform.position.z);
			return;
		}
		if(positionComponent!=null)
			transform.position = new Vector3(positionComponent.position.x, 0, positionComponent.position.y);
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
}
