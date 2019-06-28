using System;
using System.Collections.Generic;
using UnityEngine;

namespace CSD
{
	// All agents can build, for now.
	// public class BuilderComponent : Component {}

	public class BuildingComponent : Component {}

	public class BuildingResource {
		public int numNeeded = 1;
		public int numSupplied = 0;
		// TODO: specify the type of thing.
	}

	// This class is used for buildable things like houses.
	public class BuildableComponent : Component
	{
		public float totalWorkNeeded = 4; // Seconds / building speed.
		public float workDone = 0;
		public List<BuildingResource> resources = new List<BuildingResource>();
		public BuildingComponent resultBuilding;

		// Returns one of the resources needed, if any.
		public BuildingResource NeededResource () {
			foreach (var br in resources) {
				if (br.numNeeded > br.numSupplied)
					return br;
			}
			return null;
		}
		public bool IsWorkDone() {
			return totalWorkNeeded > workDone;
		}
		public bool HasEstablishedFoundation() {
			return HasEntity();
		}
	}

	public class BuildObjective : Objective {
		public BuildableComponent buildableComponent;
		public AgentComponent builder;
		public Vector2 pos;

		public BuildObjective (AgentComponent builder, Vector2 pos) {
			this.builder = builder;
			this.pos = pos;
			// TODO pass in the type of building.
			buildableComponent = new BuildableComponent();
		}

		public override bool IsComplete() {
			bool isComplete = buildableComponent.HasEstablishedFoundation() && buildableComponent.NeededResource() == null && buildableComponent.IsWorkDone();
			if (isComplete) Debug.Log("BUILDING IS COMPLETE YAY!");
			return isComplete;
		}
		public override EventComponent GetWayToDo () {
			if (!buildableComponent.HasEstablishedFoundation())
				return new CreateFoundationEvent(builder, buildableComponent, pos);
			BuildingResource neededResource = buildableComponent.NeededResource();
			if (neededResource != null)
				return new AddResourceEvent(builder, buildableComponent, neededResource);
			if (buildableComponent.IsWorkDone())
				return new DoBuildingWorkEvent(builder, buildableComponent);
			return null;
		}
	}

	// TODO maybe this should be instant
	public class CreateFoundationEvent : EventComponent {
		public AgentComponent builder;
		public BuildableComponent building;
		public Vector2 pos;

		public CreateFoundationEvent (AgentComponent builder, BuildableComponent building, Vector2 pos) {
			this.builder = builder;
			this.building = building;
			this.pos = pos;
		}
		public override void Tick(float time) {
			Entity foundation = new Entity();
			PositionComponent posCom = new PositionComponent();
			posCom.position = pos;
			foundation.AddComponent (posCom);
			foundation.AddComponent (building);
			UnityView.AddEntity(foundation);
			Debug.Log("BULDING: foundation update");
		}
		public override bool IsComplete() {
			return building.HasEstablishedFoundation ();
		}
		public override List<Requirement> GetRequirments() {
			List<Requirement> requirements = new List<Requirement> ();
			requirements.Add (new RangeRequirement (pos, builder.GetEntity().GetComponent<PositionComponent>(), 0.1f));
			return requirements;
		}
	}

	public class AddResourceEvent : EventComponent {
		public AgentComponent builder;
		public BuildableComponent building;
		public BuildingResource resource;

		public AddResourceEvent (AgentComponent builder, BuildableComponent building, BuildingResource resource) {
			this.builder = builder;
			this.building = building;
			this.resource = resource;
		}
		public override void Tick(float time) {
			resource.numSupplied += 1;
			// TODO remove a thing held.
		}
		public override bool IsComplete() {
			return resource.numNeeded <= resource.numSupplied;
		}
		public override List<Requirement> GetRequirments() {
			List<Requirement> requirements = new List<Requirement> ();
			// TODO add a requirement to be holding a thing.
			requirements.Add (new RangeRequirement (building.GetEntity().GetComponent<PositionComponent>(), builder.GetEntity().GetComponent<PositionComponent>(), 0.1f));
			return requirements;
		}
	}

	public class DoBuildingWorkEvent : EventComponent {
		public AgentComponent builder;
		public BuildableComponent building;

		public DoBuildingWorkEvent (AgentComponent builder, BuildableComponent building) {
			this.builder = builder;
			this.building = building;
		}
		public override void Tick(float time) {
			building.workDone += time;
		}
		public override bool IsComplete() {
			return building.totalWorkNeeded <= building.workDone;
		}
		public override List<Requirement> GetRequirments() {
			List<Requirement> requirements = new List<Requirement> ();
			requirements.Add (new RangeRequirement (building.GetEntity().GetComponent<PositionComponent>(), builder.GetEntity().GetComponent<PositionComponent>(), 0.1f));
			return requirements;
		}
	}
}
