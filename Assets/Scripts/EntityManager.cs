using System;
using System.Collections.Generic;
using UnityEngine;

namespace CSD
{
	public class EntityManager
	{
		//TODO handle scheduling and honoring secondsPerUpdate
		public List<UpdateableComponent> componentsToUpdate = new List<UpdateableComponent>();
		public List<Entity> entities = new List<Entity>();
		public List<IUpdateable> updatables = new List<IUpdateable> ();


		public EntityManager ()
		{
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
			componentsToUpdate.ForEach(updateable => updateable.Tick(deltaTime));
		}

		public void RegisterEntity(Entity entity){
			entities.Add (entity);
		}

		public void RegisterUpdatable(UpdateableComponent component){
			componentsToUpdate.Add (component);
		}
	}
}

