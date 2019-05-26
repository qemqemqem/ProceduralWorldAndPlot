using System;
using System.Collections.Generic;
using UnityEngine;

namespace CSD
{
	public class PositionComponent : Component
	{
		public Vector2 _position=new Vector2(0,0);

		public Vector2 position {
			get {
				return _position;
			}
			set {
				if (float.IsNaN(value.x))
					Debug.Log ("TOTALLY BOGUS");
				_position = value;
			}
		}

		public PositionComponent ()
		{
		}
	}
}


