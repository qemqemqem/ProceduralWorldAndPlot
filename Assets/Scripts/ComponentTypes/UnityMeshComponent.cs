﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CSD;


public class UnityMeshComponent : MonoBehaviour, IComponent {
	private IEntity entity;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		var positionComponent = entity.GetComponent<PositionComponent> ();
		if(positionComponent!=null)
			transform.position = new Vector3(positionComponent.position.x, 0, positionComponent.position.y);
		var plantComponent = entity.GetComponent<PlantComponent> ();
		if (plantComponent != null)
			transform.localScale = new Vector3 (plantComponent.size, plantComponent.size, plantComponent.size);
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