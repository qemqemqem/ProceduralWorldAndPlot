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

		public Resource() {}

		public virtual bool IsFree(){
			return user == null;
		}
	}
}
