using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using InControl;

namespace CSD{

	//TODO clean up the simulation and view by making the action code only interact with one of them
	//modify the world queries to standardize them in some way
	public class ProceduralWorldSimulator : MonoBehaviour {
		public static ProceduralWorldSimulator instance;
		public List<PositionComponent> foods = new List<PositionComponent> ();
		private EntityManager manager = new EntityManager();
		public PositionManager positionManager = new PositionManager();


		// Use this for initialization
		void Start () {
			instance = this;
			SetupWorld ();
		}

		void Update () {
			HandleInterface ();
			//TODO we need some better way to track world data maybe???
			RemoveTheDead();
			UpdateWorld (Time.deltaTime);
		}

		public void RemoveTheDead() {
			foods.RemoveAll(x => x.GetEntity().IsDestroyed());
		}

		public void UpdateWorld(float time){
			manager.Update (time);

		}
		public void HandleInterface(){
			//TODO implement this
		}

		private void SetupWorld(){
			//TODO initialize the view first...
			int numPeople = 5;
			int mapSize = 50;
			int numFoods = 20;
			for (int i = 0; i < numFoods; ++i) {
				Vector2 pos = positionManager.closestEmpty(new Vector3 (UnityEngine.Random.value * mapSize, 0f, UnityEngine.Random.value * mapSize));
				if (PositionManager.IsBogus(pos))
					continue;
				Entity food = new Entity ();
				PositionComponent position = new PositionComponent ();
				position.position = pos;
				food.AddComponent (position);
				foods.Add (position);
				PlantComponent plant = new PlantComponent ();
				food.AddComponent (plant);
				food.AddComponent (new CarriableComponent ());
				positionManager.ObjectSpawnedAt (food, pos);
				UnityView.AddEntity(food);
			}
			for (int i = 0; i < numPeople; ++i) {
				Vector2 pos = positionManager.closestEmpty(new Vector3 (UnityEngine.Random.value * mapSize, 0f, UnityEngine.Random.value * mapSize));
				if (PositionManager.IsBogus(pos))
					continue;
				Entity person = new Entity ();
				PositionComponent position = new PositionComponent ();
				position.position = pos;
				person.AddComponent (position);
				HumanoidAI brain = new HumanoidAI ();
				person.AddComponent (brain);
				BehaviorComponent agent = new BehaviorComponent (brain);
				agent.name = GetRandomName();
				person.AddComponent (agent);
				person.AddComponent (new InventoryComponent());
				positionManager.ObjectSpawnedAt (person, pos);
				UnityView.AddEntity(person);
				UnityView.RegisterControllableAgent (person);
			}
		}

		private string GetRandomName(){
			string[] suffixes = {"orpo", "azar", "ubari", "'adul","ule","ile","'s","opa"};
			string[] prefixes = {"Kla", "Len", "Sp", "Mrik", "Lam", "Kdor", "Blip", "Coor", "Smat", "Smo"};
			return prefixes[UnityEngine.Random.Range (0, prefixes.Length - 1)]+suffixes [UnityEngine.Random.Range (0, suffixes.Length - 1)]+"o";
		}

		public static void RegisterEntity(Entity entity){
			if (instance == null)
				return;
			instance.manager.RegisterEntity (entity);
		}

		//TODO remove when we switch over to the tickable interface
		public static void RegisterUpdatable(UpdateableComponent component){
			if (instance == null)
				return;
			instance.manager.RegisterUpdatable (component);
		}



	}

	public class PositionContent{
		public Entity entity;
		public PositionContent(Entity entity){
			this.entity = entity;
		}
		//public List<Entity> entites = new List<Entity>();
	}

	public class PositionManager{
		public static readonly Vector2 BOGUS = new Vector2(float.NaN, float.NaN);
		public Dictionary<Vector2Int, PositionContent> pos2ObjMap = new Dictionary<Vector2Int, PositionContent> ();
		public bool ObjectSpawnedAt(Entity entity, Vector2 vec){
			return ObjectSpawnedAt(entity, vec2ToVec3(vec));
		}
		public bool ObjectSpawnedAt(Entity entity, Vector3 vec){
			if (pos2ObjMap.ContainsKey(vec2Pos (vec)))
				return false;
			pos2ObjMap [vec2Pos (vec)] = new PositionContent (entity);
			return true;
		}

		public bool ObjectMovesTo(Entity entity, Vector2 start, Vector2 end){
			return ObjectMovesTo(entity, vec2ToVec3(start), vec2ToVec3(end));
		}
		public bool ObjectMovesTo(Entity entity,Vector3 start, Vector3 end){
			if (pos2ObjMap [vec2Pos (end)] != null)
				return false;
			if (pos2ObjMap [vec2Pos (start)] == null || pos2ObjMap [vec2Pos (start)].entity != entity)
				return false;
			pos2ObjMap.Remove(vec2Pos (start));
			pos2ObjMap [vec2Pos (end)] = new PositionContent (entity);
			return true;
		}
		public Vector2 closestEmpty(Vector3 pos){
			Vector2Int origPos = vec2Pos (pos);
			if (!pos2ObjMap.ContainsKey(origPos))
				return new Vector2(pos.x, pos.z);
			Vector2Int bestPos = vec2Pos (pos);
			for (int i = 0; i < 10; ++i) {
				break;
				for (int j = 0; j < i; ++j) {
					//TODO search out in spiral from center
				}
			}
			return BOGUS;
		}

		private Vector2Int vec2Pos(Vector3 vec){
			return new Vector2Int (Mathf.RoundToInt(vec.x), Mathf.RoundToInt(vec.z));
		}

		private Vector3 vec2ToVec3(Vector2 vec2d){
			return new Vector3 (vec2d.x, 0f, vec2d.y);
		}
		public static bool IsBogus(Vector2 vec2){
			return float.IsNaN (vec2.x) || float.IsNaN (vec2.y);
		}

	}

}
