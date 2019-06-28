using UnityEngine;

namespace CSD{
	public static class CSDUtils{

		public static bool HasNaNComponent(Vector2 vec2){
			return float.IsNaN (vec2.x) || float.IsNaN (vec2.y);
		}

		public static bool HasNaNComponent(Vector3 vec3){
			return float.IsNaN (vec3.x) || float.IsNaN (vec3.y) || float.IsNaN (vec3.z);
		}

		public static bool IsFinite(Vector2 vec2){
			return !HasNaNComponent (vec2) && (!float.IsInfinity (vec2.x) && !float.IsInfinity (vec2.y));
		}

		public static bool IsFinite(Vector3 vec3){
			return !HasNaNComponent (vec3) && (!float.IsInfinity (vec3.x) && !float.IsInfinity (vec3.y) && !float.IsInfinity (vec3.z));
		}
	}
}
