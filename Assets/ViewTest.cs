using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CSD;

public class ViewTest : MonoBehaviour {

	public static ViewTest viewer;

	public static Dictionary<Entity, GameObject> displaysMap = new Dictionary<Entity, GameObject> ();

	public GameObject prototype;

	public static List<Entity> entities = new List<Entity>();

	// Use this for initialization
	void Awake () {
		viewer = this;
	}

	// Update is called once per frame
	void Update () {
		RemoveTheDead();
		for (int i = 0; i < entities.Count; i++) {
			var pos = entities[i].GetComponent<PositionComponent>().position;
			displaysMap[entities[i]].transform.position = new Vector3(pos.x, 0, pos.y);
		}
	}

	public void RemoveTheDead() {
		foreach (var entity in entities) {
			if (entity.isDestroyed) {
				Destroy(displaysMap[entity]);
			}
		}
		entities.RemoveAll(x => x.IsDestroyed());
	}

	public static void AddEntity(Entity entity) {
		entities.Add(entity);

		GameObject display = GameObject.Instantiate(viewer.prototype);
		displaysMap[entity] = display;
	}
}
