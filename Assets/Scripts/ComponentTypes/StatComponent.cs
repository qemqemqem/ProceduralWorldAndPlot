using UnityEngine;
using System.Collections.Generic;


namespace CSD{

	public interface IStat{
		int GetValue ();
	}

	//TODO these can be instantaneous or durational
	public class StatEffect{
		//-1f means the effect is permenant, 0 means instant
		float duration=-1f;
		int value;
		string[] tags;
		string statName;
	}

	public class IntStat : IStat {
		public int value;
		public IntStat(int value){
			this.value = value;
		}
		public int GetValue(){
			return value;
		}
	}

	public class BoundedIntStat : IntStat {
		public int value=100;
		public int min=0;
		public int max=100;

		public BoundedIntStat (int value) : base (value) {
			this.max = value;
		}

		public int GetValue(){
			return Mathf.Min(Mathf.Max(value, min), max);
		}

		public int GetMin(){
			return min;
		}

		public int GetMax(){
			return max;
		}
	}

	public interface IStatBlock{
		List<IStat> GetStats();
		List<StatEffect> GetEffects();
		IStat GetStat(string name);
		T GetStat<T> (string name) where T : IStat;
		void AddEffect (StatEffect effect);
	}

	public class AgentStatBlock : IStatBlock{
		public List<StatEffect> ongoingEffects = new List<StatEffect> ();
		public BoundedIntStat health = new BoundedIntStat (100);
		public BoundedIntStat hunger = new BoundedIntStat (100);
		public IntStat eatSpeed = new IntStat (50);
		public IntStat moveSpeed = new IntStat (500);


		public List<IStat> GetStats(){
			List<IStat> stats = new List<IStat> ();
			stats.Add (health);
			stats.Add (hunger);
			return stats;
		}
		public List<StatEffect> GetEffects(){
			return ongoingEffects;
		}

		//TODO just keep a dictionary and add the stats to that
		public IStat GetStat(string name){
			string lcName = name.ToLower ();
			if (lcName.Equals ("health")) {
				return health;
			} else if (lcName.Equals ("hunger")) {
				return hunger;
			} else if (lcName.Equals ("eat speed")) {
				return eatSpeed;
			} else if (lcName.Equals ("move speed")) {
				return moveSpeed;
			}
			return null;
		}
		public T GetStat<T> (string name) where T : IStat{
			return (T)GetStat (name);
		}

		public void AddEffect(StatEffect effect){
			//check the resistances

		}
	}

	public class StatComponent : Component, IStatBlock
	{
		private Dictionary<string, IStat> namedStats = new Dictionary<string, IStat> ();
		private List<IStat> stats = new List<IStat>();
		private List<StatEffect> effects = new List<StatEffect> ();

		public List<IStat> GetStats(){
			return stats;
		}

		public List<StatEffect> GetEffects (){
			return effects;
		}

		public IStat GetStat(string name){
			if (!namedStats.ContainsKey (name))
				return null;
			return namedStats [name];
		}

		public T GetStat<T>(string name) where T : IStat{
			var stat = GetStat (name);
			if (stat == null || !(stat is T))
				return default(T);
			return (T) stat;
		}

		public void AddEffect(StatEffect effect){
		}
	}

}