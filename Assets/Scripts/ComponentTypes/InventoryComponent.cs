using System;
using System.Collections.Generic;
using UnityEngine;

namespace CSD
{
	public interface IViewHolder{
		void HoldThing (IEntity entity);
		void DropThing (IEntity entity);
	}

	public class InventorySlotResource : Resource
	{
		public string slotName = "UNNAMED INVENTORY SLOT";

		public CarriableComponent item;

		public override bool IsFree() {
			return item == null;
		}
	}

	public class InventoryComponent : Component
	{
		public InventorySlotResource haulingSlot = new InventorySlotResource();
	}

	public class CarriableComponent : Component
	{
		public InventoryComponent carrier;
	}

	public class PickUpEvent : InstantEvent
	{
		public readonly InventoryComponent carrier;
		public readonly CarriableComponent carried;
		public readonly IViewHolder viewHolder;

		public PickUpEvent(InventoryComponent carrier, CarriableComponent carried, IViewHolder viewHolder) {
			this.carrier = carrier;
			this.carried = carried;
			this.viewHolder = viewHolder;
		}

		public override void Initialize () {
			base.Initialize ();
			if (carrier.haulingSlot.IsFree()) {
				carrier.haulingSlot.item = carried;
				carried.carrier = carrier;
				if (viewHolder != null)
					viewHolder.HoldThing (carried.GetEntity ());
			}
		}

		public override List<Requirement> GetRequirments(){
			List<Requirement> requirements = new List<Requirement> ();
			requirements.Add (new RangeRequirement (carried.GetEntity().GetComponent<PositionComponent>(), carrier.GetEntity().GetComponent<PositionComponent>(), 3f));
			return requirements;
		}

		public override string ToString ()
		{
			return string.Format ("[Picking Up]: "+carried.GetEntity().ToString());
		}

		public override Transform DbgGetTarget ()
		{
			if (carried == null)
				return null;
			return carried.GetEntity ().GetComponent<UnityMeshComponent> ().gameObject.transform;
		}
	}

	public class DropEvent : InstantEvent
	{
		public readonly InventoryComponent carrier;
		public readonly InventorySlotResource slot;
		public readonly IViewHolder viewHolder;

		public DropEvent(InventoryComponent carrier, InventorySlotResource slot, IViewHolder viewHolder) {
			this.carrier = carrier;
			this.slot = slot;
			this.viewHolder = viewHolder;
		}

		public override void Initialize () {
			base.Initialize ();
			if (viewHolder != null)
				viewHolder.DropThing (slot.item.GetEntity ());
			if (slot.item.carrier == carrier)
				slot.item.carrier = null;
			slot.item = null;
		}

		public override string ToString ()
		{
			if (slot == null || slot.item == null)
				return "[null drop: " + (slot == null) + " / " + (slot.item == null) + "]";
			Math.Sqrt (2);
			return string.Format ("[Dropping Up]: "+slot.item.GetEntity().ToString());
		}
		/*
		public override List<Requirement> GetRequirments(){
			List<Requirement> requirements = new List<Requirement> ();
			requirements.Add (new RangeRequirement (food, eater.GetEntity().GetComponent<PositionComponent>(), 0.1f));
			return requirements;
		}*/
	}
}
