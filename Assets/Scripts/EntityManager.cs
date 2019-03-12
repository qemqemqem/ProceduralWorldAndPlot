using System;
using System.Collections.Generic;
using UnityEngine;

namespace CSD
{
	public class EntityManager
	{
		private static EntityManager instance;

		//TODO handle scheduling and honoring secondsPerUpdate
		public List<UpdateableComponent> componentsToUpdate = new List<UpdateableComponent>();
		public List<Entity> entities = new List<Entity>();

		public EntityManager ()
		{
			instance = this;
		}

		public void Update(float deltaTime){
			Resource.allResources.ForEach(resource=> {
				if(resource.user!=null&&resource.user.IsComplete())
					resource.user=null;
			});
			Resource.allResources.RemoveAll (resource => resource.owner.GetEntity ().IsDestroyed ());
			componentsToUpdate.RemoveAll (updatable => updatable.GetEntity ()!=null?
				updatable.GetEntity ().IsDestroyed ():((EventComponent)updatable).IsComplete());
			entities.RemoveAll (entity => entity.IsDestroyed ());
			componentsToUpdate.ForEach(updateable => updateable.Update(deltaTime));
		}

		public static void RegisterEntity(Entity entity){
			instance.entities.Add (entity);
		}

		public static void RegisterUpdatable(UpdateableComponent component){
			instance.componentsToUpdate.Add (component);
		}
	}
}

