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
	}

	public void RemoveTheDead() {
		foreach (var entity in entities) {
			if (entity.IsDestroyed()) {
				Destroy(displaysMap[entity]);
			}
		}
		entities.RemoveAll(x => x.IsDestroyed());
	}

	public static void AddEntity(Entity entity) {
		entities.Add(entity);
		GameObject display = GameObject.Instantiate(viewer.prototype);
		UnityMeshComponent meshComponent = display.AddComponent<UnityMeshComponent>();
		meshComponent.SetEntity (entity);
		displaysMap[entity] = display;
	}
}//*/
