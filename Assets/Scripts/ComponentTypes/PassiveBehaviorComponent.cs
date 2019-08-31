using System;
using System.Collections.Generic;

namespace CSD
{

	public interface ITrigger{
		bool IsConditionMet();
	}
		
	public interface IActivatable{
		void Activate();
	}

	public delegate bool GenericCondition(IEntity entity);
	public delegate void GenericEntityModifier(IEntity entity);


	/// <summary>
	/// A lazy generic trigger, useful for manually defining triggers
	/// </summary>
	public class GenericTrigger : ITrigger{
		GenericCondition condition;
		IEntity entity;

		public GenericTrigger(IEntity entity, GenericCondition condition){
			this.condition = condition;
			this.entity = entity;
		}

		public bool IsConditionMet(){
			return condition.Invoke (entity);
		}
	}

	public class GenericInstantAction : IActivatable {
		GenericEntityModifier modifier;
		IEntity entity;
		public GenericInstantAction(IEntity entity, GenericEntityModifier modifier){
			this.modifier = modifier;
			this.entity = entity;
		}

		public void Activate(){
			modifier.Invoke (entity);
		}
	}


	//TODO use this for the plant entity growth and reproduction
	public class PassiveBehaviorComponent : Component, IUpdateable
	{
		public Dictionary<ITrigger, IActivatable> passiveBehaviors = new Dictionary<ITrigger, IActivatable>();

		public PassiveBehaviorComponent ()
		{
		}

		public void Tick(float deltaT){
			//TODO have different triggers checked on different time scales
			foreach (var entry in passiveBehaviors) {
				if (entry.Key.IsConditionMet ()) {
					entry.Value.Activate();
				}
			}
		}

		public IActivatable AddPassiveBehavior(ITrigger trigger, IActivatable response){
			if (passiveBehaviors.ContainsKey (trigger))
				return passiveBehaviors[trigger];
			passiveBehaviors.Add (trigger, response);
			return null;
		}

		public bool RemovePassiveBehavior(ITrigger trigger){
			if (!passiveBehaviors.ContainsKey (trigger))
				return false;
			return RemovePassiveBehavior(trigger, null);
		}

		public bool RemovePassiveBehavior(ITrigger trigger, IActivatable response){
			if (!passiveBehaviors.ContainsKey (trigger) || (response!=null&&passiveBehaviors [trigger] != response))
				return false;
			passiveBehaviors.Remove(trigger);
			return true;
		}
	}
}

