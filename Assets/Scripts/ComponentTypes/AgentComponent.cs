using System;
using System.Collections.Generic;
using UnityEngine;

namespace CSD
{

	public class Resource {
		public static List<Resource> allResources = new List<Resource> ();
		public string name;
		public Component owner;
		public EventComponent user;
		public Resource(string name, Component owner){
			this.name = name;
			this.owner = owner;
			allResources.Add (this);
		}

		public bool IsFree(){
			return user == null;
		}
	}

	public class Objective {
		public virtual bool IsComplete(){
			return false;
		}
		public virtual EventComponent GetWayToDo () {
			return null;
		}
	}

	public class FullySpecifiedObjective : Objective {
		public EventComponent wayToDoObjective;
		public FullySpecifiedObjective(EventComponent wayToDoObjective){
			this.wayToDoObjective = wayToDoObjective;
		}

		public override bool IsComplete(){
			return wayToDoObjective == null || wayToDoObjective.IsComplete ();
		}
	}

	public class AgentComponent : Component
	{
		public List<Objective> objectives = new List<Objective> ();
		//public List<EventComponent> currentEvents = new List<EventComponent> ();
		public Dictionary<EventComponent, Objective> action2Objective = new Dictionary<EventComponent, Objective> ();
		//TODO 
		public Resource movement;
		public string name="Glorpo";
		public float fullness=100;
		public float moveSpeed=5f;
		public float eatSpeed=1f;
		public PositionComponent targetFood;

		//TODO maybe add convenience for tracking resources in derived events
		public AgentComponent ()
		{
			movement = new Resource("Movement", this);
		}
		//for now we can just use these updates to simulate things and have the simulation speed be pausable and change
		public void Update(float time){

			UpdateObjectives ();

			if (objectives.Count==0) {
				List<PositionComponent> foods = ProceduralWorldSimulator.instance.foods;
				var targetFood = foods [UnityEngine.Random.Range (0, foods.Count - 1)];
				Debug.Log ("Starting objective to eat food at "+targetFood.position);
				objectives.Add (new FullySpecifiedObjective (new EatEvent (this, targetFood)));
				/*
				if (targetFood == null)
					return;
				EventComponent eatEvent = new EatEvent (this, targetFood);
				if (!eatEvent.IsComplete ()) {
					Debug.Log ("Starting " + eatEvent.GetDescription ());
					ProceduralWorldSimulator.instance.ongoingEvents.Add (eatEvent);
					targetFood = null;
				} else {
					MoveEvent moveEvent = new MoveEvent (this, targetFood.position, moveSpeed);
					Debug.Log ("Starting " + moveEvent.GetDescription ());
					ProceduralWorldSimulator.instance.ongoingEvents.Add (moveEvent);
				}*/
			}
			PruneEvents ();
		}

		public void PruneEvents(){
			List<EventComponent> completedEvents = new List<EventComponent> ();
			foreach (var entry in action2Objective) {
				if (entry.Key.IsComplete ())
					completedEvents.Add (entry.Key);
				if (entry.Value.IsComplete ())
					objectives.Remove (entry.Value);
			}
			foreach (var action in completedEvents) {
				action2Objective.Remove (action);
			}
		}

		//TODO modify this to look over the list of the top actions for each objective not just the best action
		public void UpdateObjectives(){
			RankObjectives ();
			while (HasAvailableResources()&&HasDoableActions()) {
				foreach (var objective in objectives) {
					if(action2Objective.ContainsValue(objective))
						continue;
					EventComponent action = GetBestDoableAction (objective);
					if (action != null) {
						action2Objective.Add (action, objective);
						AllocateResources (action);
					}
				}
			}
		}

		public void AllocateResources(EventComponent action){
			action.Initialize ();
			//currentEvents.Add (action);
			ProceduralWorldSimulator.instance.ongoingEvents.Add (action);
		}

		public void RankObjectives(){
			//TODO rank objectives by priority and availability of resources and activity inertia
			objectives.Sort ();
		}

		private bool HasAvailableResources(){
			//TODO handle more general set of resources for this agent
			return IsResourceAvailable(movement);
		}

		private bool IsResourceAvailable(Resource resource){
			return resource.user == null;
		}

		private bool HasDoableActions(){
			foreach (var objective in objectives) {
				if(action2Objective.ContainsValue(objective))
					continue;
				EventComponent action = GetBestDoableAction (objective);
				if (action != null)
					return true;
			}
			return false;
		}

			
		public EventComponent GetBestDoableAction(Objective objective){
			EventComponent action = GetBestAction (objective);
			if (!AreResourcesAvailable (action))
				return null;
			var requirements = action.GetRequirments ();
			if (requirements == null || requirements.Count == 0)
				return action;
			foreach (var r in requirements) {
				if (r.IsComplete ()) {
					continue;
				}
				var best = GetBestDoableAction (r);
				if (best == null)
					return null;
				action = best;
			}
			return action;
		}

		public bool AreResourcesAvailable(EventComponent action){
			foreach (var resource in action.GetRequiredResources()) {
				if (!IsResourceAvailable (resource))
					return false;
			}
			return true;
		}
	
		public EventComponent GetBestAction(Objective objective){
			var wayToDo = objective.GetWayToDo ();
			if (wayToDo != null) {
				return wayToDo;
			}
			if (objective is FullySpecifiedObjective)
				return ((FullySpecifiedObjective)objective).wayToDoObjective;
			return null;
		}

	}
}

