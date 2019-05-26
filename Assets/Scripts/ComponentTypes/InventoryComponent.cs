using System;
using System.Collections.Generic;
using UnityEngine;

namespace CSD
{
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
		public InventoryComponent carrier;
		public CarriableComponent carried;

		public PickUpEvent(InventoryComponent carrier, CarriableComponent carried) {
			this.carrier = carrier;
			this.carried = carried;
		}

		public override void Initialize () {
			if (carrier.haulingSlot.IsFree()) {
				carrier.haulingSlot.item = carried;
				carried.carrier = carrier;
			}
		}
	}

	public class DropEvent : InstantEvent
	{
		public InventoryComponent carrier;
		public InventorySlotResource slot;

		public DropEvent(InventoryComponent carrier, InventorySlotResource slot) {
			this.carrier = carrier;
			this.slot = slot;
		}

		public override void Initialize () {
			if (slot.item.carrier == carrier)
				slot.item.carrier = null;
			slot.item = null;
		}
	}
}
