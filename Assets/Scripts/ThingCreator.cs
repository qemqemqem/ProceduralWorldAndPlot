using System;
using System.Collections.Generic;
using UnityEngine;

namespace CSD
{
	public class ThingCreator {
		public static Entity CreatePlant(Vector2 position) {
			Vector3 offsetDir = UnityEngine.Random.onUnitSphere;
			var plantPos = new PositionComponent ();
			plantPos.position = position;
			var plantPlant = new PlantComponent ();
			Entity plant = new Entity ();
			plant.AddComponent (plantPos);
			plant.AddComponent (plantPlant);
			plant.AddComponent (new CarriableComponent ());
			ProceduralWorldSimulator.instance.positionManager.ObjectSpawnedAt (plant, position);
			ProceduralWorldSimulator.instance.foods.Add (plantPos);
			UnityView.AddEntity (plant);
			return plant;
		}
	}
}
