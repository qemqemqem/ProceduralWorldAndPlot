using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CSD{

	public class AgentDebugger : MonoBehaviour {
		private UnityMeshComponent umc;
		private IEntity entity;
		private AgentComponent ac;
		public string currentBehavior;
		public List<Transform> targets = new List<Transform> ();

		// Use this for initialization
		void Start () {
			umc = GetComponent<UnityMeshComponent> ();
		}
		
		// Update is called once per frame
		void Update () {
			if(umc==null)
				umc = GetComponent<UnityMeshComponent> ();
			if (umc == null)
				return;
			entity = umc.GetEntity ();
			if (entity == null)
				return;
			ac = entity.GetComponent<AgentComponent> ();
			if (ac == null)
				return;
			currentBehavior = "";//TODO get entity name component
			targets.Clear();
			foreach (var entry in ac.action2Objective) {
				currentBehavior+="Performing "+entry.Key.ToString()+" pursuant of "+entry.Value.ToString()+System.Environment.NewLine;
				if (entry.Key.DbgGetTarget () != null)
					targets.Add (entry.Key.DbgGetTarget ());
			}
		}
	}
}