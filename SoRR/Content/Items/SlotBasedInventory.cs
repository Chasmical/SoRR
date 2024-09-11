using System;
using System.Collections.Generic;
using System.Linq;
using Chasm.Collections;

namespace SoRR
{
    public sealed class SlotBasedInventory : Inventory
    {
        private InventorySlot[] _slots;
        public Span<InventorySlot> Slots => _slots;
        public int Capacity => _slots.Length;

        public SlotBasedInventory() : this(0) { }

        public SlotBasedInventory(int capacity)
        {
            _slots = new InventorySlot[capacity];
        }

        public void Resize(int newCapacity)
            => Array.Resize(ref _slots, newCapacity);

        public override IEnumerable<Item> GetItems()
            => _slots.Select(static slot => slot.Item).NotNull();

        public override bool AddItem(Item newItem)
        {
            if (newItem is null) return false;
            InventorySlot[] slots = _slots;

            // Try to put the new item into an existing stack
            int index = FindStackableIndex(slots, newItem);
            if (index >= 0)
            {
                newItem.TransferStackTo(slots[index].Item!, newItem.Count);
                if (newItem.Count == 0) return true;
            }

            // Try to put the new item into an empty slot
            return ReplaceItem(null, newItem);
        }
        public override bool ReplaceItem(Item? oldItem, Item? newItem)
        {
            int index = FindItemIndex(_slots, oldItem);
            if (index < 0) return false;
            ExchangeItemAndSetInventory(ref _slots[index]._item, newItem);
            return true;
        }
        public override bool ContainsItem(Item? item)
        {
            if (item is null) return false;

            InventorySlot[] slots = _slots;
            for (int i = 0; i < slots.Length; i++)
                if (slots[i].Item == item)
                    return true;
            return false;
        }

        private static int FindItemIndex(InventorySlot[] slots, Item? item)
        {
            for (int i = 0; i < slots.Length; i++)
                if (slots[i].Item == item)
                    return i;
            return -1;
        }
        private static int FindStackableIndex(InventorySlot[] slots, Item otherItem)
        {
            for (int i = 0; i < slots.Length; i++)
                if (slots[i].Item?.CanStackWith(otherItem) == true)
                    return i;
            return -1;
        }

    }
    public struct InventorySlot
    {
        internal Item? _item;
        public readonly Item? Item => _item;
    }
}
