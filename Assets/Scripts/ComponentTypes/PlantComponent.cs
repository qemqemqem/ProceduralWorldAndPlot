using System;
using System.Collections.Generic;
using UnityEngine;

namespace CSD
{
	public class PlantComponent : UpdateableComponent
	{
		public float size = 1.0f;
		public float growthRate =.63f;
		public float minSize = 1.0f;
		public float maxSize = 4.0f;
		public int numOffspringMin = 1;
		public int numOffspringMax = 2;
		public float spreadRadius = 10f;
		public Resource substance;

		public PlantComponent () : base()
		{
			substance = new Resource ("substance", this);
			growthRate = UnityEngine.Random.value*growthRate;
			Activate ();
		}

		public override void Update (float time)
		{
			if (!substance.IsFree ())
				return;
			size = Mathf.Max (Mathf.Min (size + time * growthRate, maxSize), minSize);
			if (size == maxSize&&!GetEntity().IsDestroyed()) {
				SpawnOffspring ();
				size = minSize;
			}
		}

		public void SpawnOffspring(){
			int numOffspring = UnityEngine.Random.Range (numOffspringMin, numOffspringMax + 1);
			var position = GetEntity ().GetComponent<PositionComponent> ();
			for (int i = 0; i < numOffspring;  ++i) {
				Vector3 offsetDir = UnityEngine.Random.onUnitSphere;
				var offspringPos = new PositionComponent ();
				var offsetMagnitude = UnityEngine.Random.value*spreadRadius;
				ThingCreator.CreatePlant(new Vector2 (position.position.x + offsetDir.x*offsetMagnitude, position.position.y + offsetDir.y*offsetMagnitude));
			}
		}
	}
}
