using System;

namespace SoRR
{
    public abstract class Item
    {
        public Inventory? Inventory { get; set; }
        public ItemMetadata Metadata { get; private set; } = null!;

        private int _count;
        public int Count
        {
            get => _count;
            set
            {
                if (value < 0) throw new ArgumentOutOfRangeException(nameof(value), value, $"{nameof(Count)} cannot be less than 0.");
                _count = value;
            }
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

        public virtual bool CanStackWith(Item other)
            => Metadata == other.Metadata;

        public virtual void AddStackFrom(Item other)
        {
            Count += other.Count;
            other.Count = 0;
        }

    }
    public sealed class LampOfCubey : Item
    {
        protected override Item? SplitStackOff(int count) => null;
        public override bool CanStackWith(Item other) => false;
        public override void AddStackFrom(Item other) { }
    }
}
