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

        public int Capacity { get => _slots.Length; set => Resize(value); }

        private void Resize(int newCapacity)
        {
            InventorySlot[] oldSlots = _slots;

            if (newCapacity > oldSlots.Length)
            {
                // Simply add new empty slots
                Array.Resize(ref _slots, newCapacity);
                return;
            }
            if (newCapacity == oldSlots.Length) return;

            InventorySlot[] newSlots = oldSlots[..newCapacity];

            // Move items from removed slots into available empty slots
            int j = 0;
            for (int i = newCapacity; i < oldSlots.Length; i++)
            {
                if (oldSlots[i].Item is null) continue;

                while (newSlots[j].Item is not null)
                {
                    j++;
                    if (j == newSlots.Length) throw new InvalidOperationException("Could not resize the inventory.");
                }
                newSlots[j] = oldSlots[i];
                j++;
            }
            _slots = newSlots;
        }

        public SlotBasedInventory(int capacity)
        {
            _slots = new InventorySlot[capacity];
        }

        public override IEnumerable<Item> GetItems()
            => _slots.Select(static slot => slot.Item).NotNull();

        private static int FindItemIndex(InventorySlot[] slots, Item? item)
        {
            for (int i = 0; i < slots.Length; i++)
                if (slots[i].Item == item)
                    return i;
            return -1;
        }

        public override bool AddItem(Item newItem)
        {
            if (newItem is null) return false;
            InventorySlot[] slots = _slots;

            // Try to put the new item into an existing stack
            int index = Array.FindIndex(slots, slot => slot.Item?.CanStackWith(newItem) == true);
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

    }
    public struct InventorySlot
    {
        internal Item? _item;
        public readonly Item? Item => _item;
    }
}
