using System;

namespace SoRR
{
    public abstract class Item
    {
        public Inventory? Inventory { get; private set; }

        public ItemMetadata Metadata { get; private set; } = null!;

        private int _count;
        public int Count
        {
            get => _count;
            set
            {
                if (value < 0) throw new ArgumentOutOfRangeException(nameof(value), value, $"{nameof(Count)} cannot be less than 0.");
                if (value == 0) _ = Inventory?.RemoveItem(this);
                _count = value;
            }
        }

        internal static void InternalSetInventory(Item item, Inventory? value)
        {
            if (item.Inventory is not null && value is not null)
            {
                if (!item.Inventory.RemoveItem(item))
                    throw new InvalidOperationException($"Could not remove item {item} from its previous inventory.");
            }
            item.Inventory = value;
        }

        internal void InitialSetup(ItemMetadata metadata)
        {
            Metadata = metadata;
        }

        protected virtual Item? SplitStackOff(int count)
        {
            Count -= count;
            return Game.CreateItem(Metadata, count);
        }

        public bool CanStackWith(Item other)
            => TransferStackTo(other, 0);

        public virtual bool TransferStackTo(Item other, int count)
        {
            if (Metadata != other.Metadata) return false;
            Count -= count;
            other.Count += count;
            return true;
        }

    }
    public sealed class LampOfCubey : Item
    {
        protected override Item? SplitStackOff(int count) => null;
    }
}
