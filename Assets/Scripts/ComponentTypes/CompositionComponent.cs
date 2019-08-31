using System;
using System.Collections.Generic;

namespace CSD
{

	//CORE components are Behavior, Stats, Composition and Navigation

	public class Role{
		public IEntity member;
	}

	public class CompositionComponent : Component
	{
		public List<Role> components = new List<Role> ();

		public CompositionComponent ()
		{
		}

		public void AddRole(Role role){
			components.Add (role);
		}

	}
}

