using System;
using System.Collections.Generic;
using UnityEngine;

namespace CSD
{
	//TODO make resources use Requirements
	public class Requirement : Objective {
	}

	public class RangeRequirement : Requirement{
		public float range;
		public PositionComponent rangeTo;
		public PositionComponent actor;
		public RangeRequirement (PositionComponent target, PositionComponent actor, float range){
			this.range = range;
			this.rangeTo = target;
			this.actor = actor;
		}
		public override bool IsComplete ()
		{
			return rangeTo == null || actor == null || Vector2.Distance (rangeTo.position, actor.position) <= range;
		}
		public override EventComponent GetWayToDo () {
			var actorAgent = actor.GetEntity ().GetComponent<AgentComponent> ();
			if (actorAgent == null)
				return null;
			return new MoveEvent (actorAgent, rangeTo.position, actorAgent.moveSpeed);
		}
	}

	public class EventComponent : Component
	{
		public virtual string GetName(){
			return "Unnamed Event";
		}

		public virtual string GetDescription(){
			return "This Event has no description";
		}

		public virtual void Initialize(){
		}

		public virtual List<Resource> GetRequiredResources(){
			return new List<Resource>();
		}

		//return true when the event is complete and is ready to be destroyed
		public virtual void Update(float time){
			//TODO this is where we update things and generate effects
		}

		public virtual bool IsComplete(){
			return false;
		}

		public virtual List<Requirement> GetRequirments(){
			return null;
		}
	}

	public class MoveEvent : EventComponent
	{
		private AgentComponent mover;
		private PositionComponent moverPosition;
		private Vector2 desiredPosition;
		private float moveSpeed;
		private float progress;
		private float maxDist;

		public MoveEvent (AgentComponent mover, Vector2 desiredPosition, float moveSpeed)
		{
			this.mover = mover;
			this.moverPosition = mover.GetEntity ().GetComponent<PositionComponent> ();
			this.desiredPosition = desiredPosition;
			this.moveSpeed = moveSpeed;
			this.maxDist = Vector2.Distance (desiredPosition, moverPosition.position);
		}

		public override void Initialize ()
		{
			mover.movement.user = this;
			this.maxDist = Vector2.Distance (desiredPosition, moverPosition.position);
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
		public override void Update(float time){
			if (mover == null || progress >= 1.0f)
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
			}
		}

		public override bool IsComplete(){
			return progress >= 1.0f;
		}
	}

	public class EatEvent : EventComponent
	{
		private AgentComponent eater;
		private PositionComponent food;
		private float progress;

		public EatEvent (AgentComponent eater, PositionComponent food)
		{
			this.eater = eater;
			this.food = food;
		}

		public override void Initialize ()
		{
			progress = 0f;
			eater.movement.user = this;
		}

		public override List<Resource> GetRequiredResources(){
			//TODO get requirements instead of just resources so it can be used by the AI to plan out actions
			List<Resource> resources = new List<Resource> ();
			resources.Add (eater.movement);
			return resources;
		}

		public override string GetName(){
			return "Eat food";
		}

		public override string GetDescription(){
			return eater.name+" is eating food";
		}

		//return true when the event is complete and is ready to be destroyed
		public override void Update(float time){
			if (eater == null || food == null || progress >= 1.0f)
				progress = 1.0f;
			else {
				progress = Mathf.Min (1f, progress + time * eater.eatSpeed);
			}
			if (IsComplete()) {
				food.GetEntity().SetDestroyed(true);
			}
		}

		public override bool IsComplete(){
			return progress >= 1.0f;
		}

		public override List<Requirement> GetRequirments(){
			List<Requirement> requirements = new List<Requirement> ();
			requirements.Add (new RangeRequirement (food, eater.GetEntity().GetComponent<PositionComponent>(), .1f));
			return requirements;
		}
	}
}
