using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InControl;

namespace CSD{
	public class ProceduralWorldSimulator : MonoBehaviour {
		public static ProceduralWorldSimulator instance;
		public List<PositionComponent> foods = new List<PositionComponent> ();
		private static EntityManager manager = new EntityManager();



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


		public void DispalyOptionsToPlayer(){
			//TODO populate some text of actions/things the player can interact with
			//TODO place buttons next to text
			//TODO label actions with key strokes
		}

		private void SetupWorld(){
			int numPeople = 5;
			int mapSize = 50;
			int numFoods = 20;
			for (int i = 0; i < numFoods; ++i) {
				Entity food = new Entity ();
				PositionComponent position = new PositionComponent ();
				position.position = new Vector2 (UnityEngine.Random.value * mapSize, UnityEngine.Random.value * mapSize);
				food.AddComponent (position);
				foods.Add (position);
				PlantComponent plant = new PlantComponent ();
				food.AddComponent (plant);
				food.AddComponent (new CarriableComponent ());
				ViewTest.AddEntity(food);
			}
			for (int i = 0; i < numPeople; ++i) {
				Entity person = new Entity ();
				PositionComponent position = new PositionComponent ();
				position.position = new Vector2 (UnityEngine.Random.value * mapSize, UnityEngine.Random.value * mapSize);
				person.AddComponent (position);
				HumanoidAI brain = new HumanoidAI ();
				person.AddComponent (brain);
				AgentComponent agent = new AgentComponent (brain);
				agent.name = GetRandomName();
				person.AddComponent (agent);
				person.AddComponent (new InventoryComponent());
				ViewTest.AddEntity(person);
				ViewTest.RegisterControllableAgent (person);
			}
		}

		private string GetRandomName(){
			string[] suffixes = {"orpo", "azar", "ubari", "'adul","ule","ile","'s","opa"};
			string[] prefixes = {"Kla", "Len", "Sp", "Mrik", "Lam", "Kdor", "Blip", "Coor", "Smat", "Smo"};
			return prefixes[UnityEngine.Random.Range (0, prefixes.Length - 1)]+suffixes [UnityEngine.Random.Range (0, suffixes.Length - 1)]+"o";
		}



	}
}
