using System;
using System.Collections.Generic;
using UnityEngine;

namespace CSD
{
	public interface IRequirementHolder {
		List<Requirement> GetRequirments ();
	}

	public interface IWorldEvent : IComponent, IUpdateable, IRequirementHolder {
		string GetName ();

		string GetDescription ();

		void Initialize ();

		List<Resource> GetRequiredResources ();

		void Tick();

		bool IsComplete();

		List<Requirement> GetRequirments ();
	}


	//TODO make resources use Requirements
	public class Requirement : Objective {
	}

	public class RangeRequirement : Requirement{
		//TODO replace this with colliders and adjacency and orientation
		public float range;
		public PositionComponent rangeTo;
		public PositionComponent actor;
		public Vector2 rangeToPos;

		// TODO switch target and actor so they make sense, actor first.
		public RangeRequirement (PositionComponent target, PositionComponent actor, float range){
			this.range = range;
			this.rangeTo = target;
			this.actor = actor;
		}
		public RangeRequirement (Vector2 targetPos, PositionComponent actor, float range){
			this.range = range;
			this.actor = actor;
			this.rangeToPos = targetPos;
		}
		public override bool IsComplete ()
		{
			if (rangeTo != null) {
				if (actor == null || Vector2.Distance (rangeTo.position, actor.position) <= range)
					return true;
			} else if (rangeToPos != null) {
				if (actor == null || Vector2.Distance (rangeToPos, actor.position) <= range)
					return true;
			}
			return false;
		}
		public override EventComponent GetWayToDo () {
			var actorAgent = actor.GetEntity ().GetComponent<BehaviorComponent> ();
			if (actorAgent == null)
				return null;
			var components = actor.GetEntity ().GetComponents<IComponent> ();
			if (rangeTo != null)
				return new MoveEvent (actorAgent.GetEntity().GetComponent<UnityMeshComponent>(), actorAgent, rangeTo.position, actorAgent.stats.GetStat("move speed").GetValue() / 100f);
			if (rangeToPos != null)
				return new MoveEvent (actorAgent.GetEntity().GetComponent<UnityMeshComponent>(), actorAgent, rangeToPos, actorAgent.stats.GetStat("move speed").GetValue() / 100f);
			return null;
		}
	}

	//TODO convert this to an interface so that unity components can also implement this
	public class EventComponent : UpdateableComponent
	{
		public EventComponent() {
		}

		public virtual string GetName(){
			return "Unnamed Event";
		}

		public virtual string GetDescription(){
			return "This Event has no description";
		}

		public virtual void Initialize(){
			Activate ();
		}

		public virtual List<Resource> GetRequiredResources(){
			return new List<Resource>();
		}

		//return true when the event is complete and is ready to be destroyed
		public  override void Tick(float time){
			//TODO this is where we update things and generate effects
		}

		public virtual bool IsComplete(){
			return false;
		}

		public virtual List<Requirement> GetRequirments(){
			return null;
		}

		public virtual Transform DbgGetTarget(){
			return null;
		}

		public virtual string ToString() {
			return "hello " + GetName ();
		}
	}

	public class InstantEvent : EventComponent
	{
		private bool isComplete = false;

		public override bool IsComplete() {
			return isComplete;
		}

		public override void Initialize () {
			isComplete = true;
		}
	}

	public interface IPathfindingInterface{
		void SetTarget (Vector2 position);
		bool HasReachedDestination();
		void Cancel();
		void TogglePathfinding (bool value);
		//TODO this is probalby overloaded and should be a different interface
		void ToggleCollision (bool value);
	}

	public interface ISensingInterface{
		List<IEntity> GetNearbyEntities (float range);
		bool HasLineOfSight (IEntity entity);
		bool OnSameSurface (IEntity entity);
		float GetBrightness (IEntity entity);
	}

	public class MoveEvent : EventComponent
	{
		private BehaviorComponent mover;
		private PositionComponent moverPosition;
		private Vector2 desiredPosition;
		private float moveSpeed;
		private float progress;
		private float maxDist;
		private IPathfindingInterface pc;


		public MoveEvent (IPathfindingInterface pc, BehaviorComponent mover, Vector2 desiredPosition, float moveSpeed)
		{
			this.pc = pc;
			this.mover = mover;
			this.moverPosition = mover.GetEntity ().GetComponent<PositionComponent> ();
			if (float.IsNaN(desiredPosition.x))
				Debug.Log ("SUPER UNCOOL");
			this.desiredPosition = desiredPosition;
			this.moveSpeed = moveSpeed;
			this.maxDist = Vector2.Distance (desiredPosition, moverPosition.position);
		}

		public override void Initialize ()
		{
			ProceduralWorldSimulator.RegisterUpdatable (this);
			mover.movement.user = this;
			this.maxDist = Vector2.Distance (desiredPosition, moverPosition.position);
			//TODO turn off other movement
			if(pc!=null)
				pc.SetTarget (desiredPosition);
			Activate ();

		}

		public override List<Resource> GetRequiredResources(){
			List<Resource> resources = new List<Resource> ();
			resources.Add (mover.movement);
			return resources;
		}

		public override string GetName(){

			return "Move to ("+desiredPosition.x+","+desiredPosition.y+")";
		}

		public override string GetDescription(){
			return mover.name+" is moving to ("+desiredPosition.x+","+desiredPosition.y+")";
		}

		//return true when the event is complete and is ready to be destroyed
		public override void Tick(float time){
			if (pc!=null&&pc.HasReachedDestination ()||mover.movement.user!=this)
				progress = 1.0f;

			/*
			if (mover == null || progress >= 1.0f || mover.movement.user != this)
				progress = 1.0f;
			else {
				Vector2 desiredDelta = desiredPosition - moverPosition.position;
				if (desiredDelta.magnitude > moveSpeed * time) {
					desiredDelta.Normalize ();
					desiredDelta *= moveSpeed * time;
				} else {
					moverPosition.position = desiredPosition;
					progress = 1.0f;
					return;
				}
				moverPosition.position += desiredDelta;
				float distance = Vector2.Distance (moverPosition.position, desiredPosition);
				progress = (maxDist-distance)/maxDist;
			}*/
			// Move whatever they're hauling.
			/*
			var inventoryComponent = mover.GetEntity().GetComponent<InventoryComponent> ();
			if (!inventoryComponent.haulingSlot.IsFree()) {
				var hauledPosition = inventoryComponent.haulingSlot.item.GetEntity().GetComponent<PositionComponent> ();
				hauledPosition.position = moverPosition.position + Vector2.up;
			}*/
			if (progress >= 1.0f) {
				Debug.Log ("Finished " + GetName ());
				if(pc!=null)
					pc.Cancel ();
			}
		}

		public override bool IsComplete(){
			return progress >= 1.0f;
		}

		public override string ToString ()
		{
			return string.Format ("[MoveEvent]: moving to "+desiredPosition.ToString());
		}

		public override Transform DbgGetTarget ()
		{
			return null;
		}
	}

	public class EatEvent : EventComponent
	{
		private BehaviorComponent eater;
		private PositionComponent food;
		private PlantComponent plant;
		private float progress;
		private float initialSize;

		public EatEvent (IEntity eater, PositionComponent food)
		{
			this.eater = eater.GetComponent<BehaviorComponent>();
			this.food = food;
			this.plant = food.GetEntity ().GetComponent<PlantComponent> ();
		}

		public override void Initialize ()
		{
			progress = 0f;
			eater.movement.user = this;
			plant.substance.user = this;
			Activate ();
			initialSize = plant.size;
		}

		public override List<Resource> GetRequiredResources(){
			//TODO get requirements instead of just resources so it can be used by the AI to plan out actions
			List<Resource> resources = new List<Resource> ();
			resources.Add (eater.movement);
			resources.Add (plant.substance);
			return resources;
		}

		public override string GetName(){
			return "Eat food";
		}

		public override string GetDescription(){
			return eater.name+" is eating food";
		}

		//return true when the event is complete and is ready to be destroyed
		public override void Tick(float time){
			if (eater == null || food == null || progress >= 1.0f || eater.movement.user != this || plant == null || plant.substance.user != this) {
				progress = 1.0f;
				Debug.Break ();
			}
			else {
				progress += eater.stats.GetStat("eat speed").GetValue() / 100f * time;
				//plant.size -= eater.eatSpeed*time;
				//progress = 1- plant.size / initialSize;
			}
			if (IsComplete()) {
				food.GetEntity().SetDestroyed(true);
			}
			if (progress >= 1.0f)
				Debug.Log ("Finished " + GetName ());
		}

		public override bool IsComplete(){
			return progress >= 1.0f;
		}

		public override List<Requirement> GetRequirments(){
			List<Requirement> requirements = new List<Requirement> ();
			requirements.Add (new RangeRequirement (food, eater.GetEntity().GetComponent<PositionComponent>(), 3f));
			return requirements;
		}

		public override string ToString ()
		{
			return string.Format ("[EatEvent]: trying to eat food at "+food.position.ToString());
		}

		public override Transform DbgGetTarget ()
		{
			if (food == null)
				return null;
			var mc = food.GetEntity ().GetComponent<UnityMeshComponent> ();
			if (mc == null || mc.gameObject == null)
				return null;
			return food.GetEntity ().GetComponent<UnityMeshComponent> ().gameObject.transform;
		}
	}
}
