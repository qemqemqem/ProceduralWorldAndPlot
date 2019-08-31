using System;
using System.Collections.Generic;
using UnityEngine;

namespace CSD
{

	public delegate void VoidDelegate();
	public delegate bool Conditional();

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
		public virtual List<Objective> GetSortedObjectives (BehaviorComponent agentComponent){
			//TODO modify the algorithm so instead of just using the sorted objectives we have an objective priority queue
			//the order of which we modify as we elaborate on the queue
			//the idea is to greedily expand our options and then adjust rank based on certainty and expected outcome given
			//prediction of the actions other entities will take toward us and potentially the other thigns we are attentive to
			//we can then keep track of utility as well as uncertainty and cache the results for next update
			//if we compute a hash from what we checked at each step we can even check any values changed so we don't need to recompute
			//we can then update until we hit an expanded option and start assigning resources and bail once all resources are assigned
			//or we get to the bottom of the list (idle can always be an action)
			return objectives;
		}
		public void NotifyOfCompleteObjective(Objective obj){
			//if(obj.IsComplete()) // TODO this may be needed
				objectives.Remove (obj);
		}

		public List<IEntity> senseWorld(IEntity world){
			//TODO apply attentional filter to objects in the world within the scene (possibly culling to groups of attention based on similar filters)
			//apply sensory limits like line of sight or walking on the same ground mesh
			//rank with attention filter and keep only high value attention targets
			//we could apply sensory inertia or have limited updates to offset the cost of this so it isn't N^2 with the number of intelligent agents
			//or we could simply let it be N^2 with the number of intelligent agents but handle groups as individuals that follow orders from a single AI
			return null;
		}

		public List<Objective> getPotentialObjectives(List<IEntity> world){
			//TODO get a list of potential actions that we think we are likely to want to do
			//from all of the objects int he scene
			//rank them by expected outcome as underspecified options
			return null;
		}




	}

	public class HumanoidAI : AI {
		public override List<Objective> GetSortedObjectives(BehaviorComponent agentComponent){
			if (objectives.Count == 0) {
				List<PositionComponent> foods = ProceduralWorldSimulator.instance.foods;
				// AI goes here.
				//*
				if (UnityEngine.Random.value > .1f) {
					// Building.
					objectives.Add (new BuildObjective (agentComponent, new Vector2 (UnityEngine.Random.Range (-10, 10), UnityEngine.Random.Range (-10, 10))));
					Debug.Log ("Starting objective to build something");
				} else if (UnityEngine.Random.value > .5) {
					// Inventory.
					InventoryComponent inventoryComponent = GetEntity ().GetComponent<InventoryComponent> ();
					if (!inventoryComponent.haulingSlot.IsFree ()) {
						// Drop it.
						objectives.Add (new FullySpecifiedObjective (new DropEvent (inventoryComponent, inventoryComponent.haulingSlot, inventoryComponent.GetEntity ().GetComponent<UnityMeshComponent> ())));
						Debug.Log ("Starting objective to drop hauled item");
					} else {
						// Pick something up.
						if (foods.Count == 0)
							return new List<Objective> ();
						var targetFood = foods [UnityEngine.Random.Range (0, foods.Count - 1)];
						if (targetFood.GetEntity ().GetComponent<CarriableComponent> ().carrier == null) {
							Debug.Log ("Starting objective to pick up food at " + targetFood.position);
							objectives.Add (new FullySpecifiedObjective (new PickUpEvent (inventoryComponent, targetFood.GetEntity ().GetComponent<CarriableComponent> (), inventoryComponent.GetEntity ().GetComponent<UnityMeshComponent> ())));
						}
					}
				} else {
					// Eating.
					if (foods.Count == 0)
						return new List<Objective> ();
					var targetFood = foods [UnityEngine.Random.Range (0, foods.Count - 1)];
					Debug.Log ("Starting objective to eat food at " + targetFood.position);
					objectives.Add (new FullySpecifiedObjective (new EatEvent (GetEntity (), targetFood)));
				}//*/
				if (objectives.Count == 0)
					Debug.Log ("No objectives!!!");
			} else {
				Objective obj = objectives [0];
				if (obj is FullySpecifiedObjective) {
					FullySpecifiedObjective fso = (FullySpecifiedObjective)obj;
					if (fso.wayToDoObjective is PickUpEvent)
						Math.Sqrt (2);
				}
				Math.Sqrt (2);
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

		public override string ToString ()
		{
			return string.Format ("[Objective]: "+wayToDoObjective.ToString());
		}
	}

	public class BehaviorComponent : UpdateableComponent
	{
		public Dictionary<EventComponent, Objective> action2Objective = new Dictionary<EventComponent, Objective> ();

		//TODO add more preconditions to the conditional and void delegate potentially? Link conditionals to FSM maybe?
		public Dictionary<Conditional, VoidDelegate> automaticResponses = new Dictionary<Conditional, VoidDelegate> ();

		//TODO standardize resource claiming and asking for with IResourceProvider and IResourceUser
		//TODO standardize EventComponents with IActionProvider, IActionPerformer
		public Resource movement;
		public string name="Glorpo";
		public AgentStatBlock stats = new AgentStatBlock();
		public PositionComponent targetFood;
		public AI brain;
		private bool isUnderPlayerControl=false;

		//TODO maybe add convenience for tracking resources in derived events
		public BehaviorComponent (AI brain) : base()
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
				}
			}
			foreach (var action in completedEvents) {
				action2Objective.Remove (action);
			}
		}

		//TODO modify this to look over the list of the top actions for each objective not just the best action
		public void ChooseNewActions(){
			List<Objective> objectives = brain.GetSortedObjectives (this);
			if (objectives.Count == 0)
				Debug.Break ();
			while (HasAvailableResources()&&HasDoableActions()) {
				foreach (var objective in objectives) {
					if(action2Objective.ContainsValue(objective))
						continue;
					EventComponent action = GetBestDoableAction (objective);
					if (action != null) {
						if (action is PickUpEvent) {
							Math.Sqrt (2);
						}
						if (action is MoveEvent && objective is FullySpecifiedObjective) {
							FullySpecifiedObjective fso = (FullySpecifiedObjective)objective;
							if (fso.wayToDoObjective is PickUpEvent)
								Math.Sqrt (2);
						}
						action2Objective.Add (action, objective);
						AllocateResources (action);
					} else {
						Debug.Log ("No best doable action");
						Debug.Break ();
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

		public bool AddAutomaticResponse(Conditional condition, VoidDelegate effect){
			if (automaticResponses.ContainsKey (condition))
				return false;
			automaticResponses.Add (condition, effect);
			return true;
		}

		public bool RemoveAutomaticResponse(Conditional condition){
			if (!automaticResponses.ContainsKey (condition))
				return false;
			return RemoveAutomaticResponse (condition, null);
		}

		public bool RemoveAutomaticResponse(Conditional condition, VoidDelegate effect){
			if (!automaticResponses.ContainsKey (condition)||(effect!=null&&automaticResponses[condition]!=effect))
				return false;
			automaticResponses.Remove (condition);
			return true;
		}
	}
}