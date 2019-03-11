using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CSD{
	public class ProceduralWorldSimulator : MonoBehaviour {
		public static ProceduralWorldSimulator instance;
		public List<EventComponent> ongoingEvents = new List<EventComponent>();
		public List<AgentComponent> agents = new List<AgentComponent>();
		public List<PositionComponent> foods = new List<PositionComponent> ();

		// Use this for initialization
		void Start () {
			instance = this;
			SetupWorld ();
		}

		void Update () {
			HandleInterface ();
			RemoveTheDead();
			UpdateWorld (Time.deltaTime);
		}

		public void RemoveTheDead() {
			foods.RemoveAll(x => x.GetEntity().IsDestroyed());
		}

		public void UpdateWorld(float time){
			foreach (var worldEvent in ongoingEvents) {
				worldEvent.Update (time);
				if (worldEvent.IsComplete ()) {
					Debug.Log ("Completed " + worldEvent.GetDescription ());
				}
			}
			ongoingEvents.RemoveAll (x => x.IsComplete());
			foreach (var resource in Resource.allResources) {
				if (resource.user != null && resource.user.IsComplete ()) {
					resource.user = null;
				}
			}

			//TODO sort the agents in some stable order
			foreach (var agent in agents) {
				agent.Update (time);
			}
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
				ViewTest.AddEntity(food);
			}
			for (int i = 0; i < numPeople; ++i) {
				Entity person = new Entity ();
				PositionComponent position = new PositionComponent ();
				position.position = new Vector2 (UnityEngine.Random.value * mapSize, UnityEngine.Random.value * mapSize);
				person.AddComponent (position);
				AgentComponent agent = new AgentComponent ();
				agent.name = GetRandomName();
				person.AddComponent (agent);
				agents.Add (agent);
				ViewTest.AddEntity(person);
			}
		}

		private string GetRandomName(){
			string[] suffixes = {"orpo", "azar", "ubari", "'adul","ule","ile","'s","opa"};
			string[] prefixes = {"Kla", "Len", "Sp", "Mrik", "Lam", "Kdor", "Blip", "Coor", "Smat", "Smo"};
			return prefixes[UnityEngine.Random.Range (0, prefixes.Length - 1)]+suffixes [UnityEngine.Random.Range (0, suffixes.Length - 1)]+"o";
		}
	}
}
