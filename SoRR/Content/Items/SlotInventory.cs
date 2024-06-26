using System;
using System.Collections.Generic;
using System.Linq;
using Chasm.Collections;

namespace SoRR
{
    public sealed class SlotInventory : Inventory
    {
        private readonly SlotInfo[] _slots;
        public ReadOnlySpan<SlotInfo> Slots => _slots;
        public int Capacity => _slots.Length;

        public SlotInventory(int capacity)
            => _slots = new SlotInfo[capacity];

        public override IEnumerable<Item> GetItems()
            => _slots.Select(slot => slot.Item).NotNull();

        public override bool TryAddItem(Item newItem)
        {
            SlotInfo[] slots = _slots;

            // Try to put the new item into an existing stack
            for (int i = 0; i < slots.Length; i++)
            {
                ref SlotInfo slot = ref slots[i];
                if (slot.Item?.CanStackWith(newItem) == true)
                {
                    slot.Item.AddStackFrom(newItem);
                    if (newItem.Count == 0) return true;
                }
            }

            // Try to put the new item into an empty slot
            for (int i = 0; i < slots.Length; i++)
            {
                ref SlotInfo slot = ref slots[i];
                if (slot.Item is null)
                {
                    slot.Item = newItem;
                    return true;
                }
            }

            // Could not find a slot
            return false;
        }

        public override bool TryRemoveItem(Item item)
        {
            SlotInfo[] slots = _slots;

            for (int i = 0; i < slots.Length; i++)
            {
                ref SlotInfo slot = ref slots[i];
                if (slot.Item == item)
                {
                    slot.Item = null;
                    return true;
                }
            }
            return false;
        }

        public struct SlotInfo
        {
            public Item? Item { get; set; }
        }
        public enum SlotFlags
        {
            Default    = 0,
            UserLocked = 1,
        }
    }
}
