using System;
using System.Collections.Generic;
using UnityEngine;

namespace CSD
{

	public class AI : Component {
		public List<Objective> objectives = new List<Objective> ();

		public virtual EventComponent GetBestAction (Objective objective){
			var wayToDo = objective.GetWayToDo ();
			if (wayToDo != null) {
				return wayToDo;
			}
			if (objective is FullySpecifiedObjective)
				return ((FullySpecifiedObjective)objective).wayToDoObjective;
			return null;
		}
		public virtual List<Objective> GetSortedObjectives (AgentComponent agentComponent){
			return objectives;
		}
		public void NotifyOfCompleteObjective(Objective obj){
			if(obj.IsComplete())
				objectives.Remove (obj);
		}
	}

	public class HumanoidAI : AI {
		public override List<Objective> GetSortedObjectives(AgentComponent agentComponent){
			if (objectives.Count==0) {
				List<PositionComponent> foods = ProceduralWorldSimulator.instance.foods;
				// AI goes here.
				//*
				if (UnityEngine.Random.value > 0.52) {
					// Building.
					objectives.Add(new BuildObjective(agentComponent, new Vector2(UnityEngine.Random.Range(-10, 10), UnityEngine.Random.Range(-10, 10))));
					Debug.Log("Starting objective to build something");
				} else if (UnityEngine.Random.value > .5) {
					// Inventory.
					InventoryComponent inventoryComponent = GetEntity().GetComponent<InventoryComponent>();
					if (!inventoryComponent.haulingSlot.IsFree()) {
						// Drop it.
						objectives.Add (new FullySpecifiedObjective (new DropEvent(inventoryComponent, inventoryComponent.haulingSlot)));
						Debug.Log("Starting objective to drop hauled item");
					} else {
						// Pick something up.
						if (foods.Count == 0)
							return new List<Objective>();
						var targetFood = foods [UnityEngine.Random.Range (0, foods.Count - 1)];
						if (targetFood.GetEntity().GetComponent<CarriableComponent>().carrier == null) {
							Debug.Log ("Starting objective to pick up food at "+targetFood.position);
							objectives.Add (new FullySpecifiedObjective (new PickUpEvent (inventoryComponent, targetFood.GetEntity().GetComponent<CarriableComponent> ())));
						}
					}
				} else {
					// Eating.
					if (foods.Count == 0)
						return new List<Objective>();;
					var targetFood = foods [UnityEngine.Random.Range (0, foods.Count - 1)];
					Debug.Log ("Starting objective to eat food at "+targetFood.position);
					objectives.Add (new FullySpecifiedObjective (new EatEvent (GetEntity(), targetFood)));
				}//*/
			}
			return objectives;
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

	public class AgentComponent : UpdateableComponent
	{
		public Dictionary<EventComponent, Objective> action2Objective = new Dictionary<EventComponent, Objective> ();
		//TODO
		public Resource movement;
		public string name="Glorpo";
		public float fullness=100;
		public float moveSpeed=5f;
		public float eatSpeed=1f;
		public PositionComponent targetFood;
		public AI brain;
		private bool isUnderPlayerControl=false;

		//TODO maybe add convenience for tracking resources in derived events
		public AgentComponent (AI brain) : base()
		{
			this.brain = brain;
			movement = new Resource("Movement", this);
			Activate ();
		}
		//for now we can just use these updates to simulate things and have the simulation speed be pausable and change
		public override void Tick(float time){
			if (isUnderPlayerControl)
				return;
			ChooseNewActions ();
			PruneEvents ();
		}

		public void TakeControl(){
			isUnderPlayerControl = true;
			foreach (var entry in action2Objective) {
				//entry.Key.IsComplete TODO exit the action
				brain.NotifyOfCompleteObjective (entry.Value);
			}
			action2Objective.Clear ();
		}

		public void ReleaseControl(){
			isUnderPlayerControl = false;
		}

		public void PruneEvents(){
			List<EventComponent> completedEvents = new List<EventComponent> ();
			foreach (var entry in action2Objective) {
				if (entry.Key.IsComplete ())
					completedEvents.Add (entry.Key);
				if (entry.Value.IsComplete ()) {
					brain.NotifyOfCompleteObjective (entry.Value);
					//objectives.Remove (entry.Value);
				}
			}
			foreach (var action in completedEvents) {
				action2Objective.Remove (action);
			}
		}

		//TODO modify this to look over the list of the top actions for each objective not just the best action
		public void ChooseNewActions(){
			List<Objective> objectives = brain.GetSortedObjectives (this);
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
			//ProceduralWorldSimulator.instance.ongoingEvents.Add (action);
		}

		private bool HasAvailableResources(){
			//TODO handle more general set of resources for this agent
			return IsResourceAvailable(movement);
		}

		private bool IsResourceAvailable(Resource resource){
			return resource.user == null;
		}

		private bool HasDoableActions(){
			List<Objective> brainObjectives = brain.GetSortedObjectives (this);
			foreach (var objective in brainObjectives) {
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
