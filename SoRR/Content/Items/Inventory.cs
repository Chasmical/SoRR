using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace SoRR
{
    public abstract class Inventory : Injectable
    {
        public abstract IEnumerable<Item> GetItems();

        [MustUseReturnValue]
        public abstract bool AddItem(Item newItem);

        [MustUseReturnValue]
        public abstract bool ReplaceItem(Item? oldItem, Item? newItem);

        public abstract bool ContainsItem(Item? item);

        [MustUseReturnValue]
        public bool RemoveItem(Item? item)
            => item is not null && ReplaceItem(item, null);

        protected void ExchangeItemAndSetInventory(ref Item? storedItem, Item? newItem)
        {
            if (storedItem == newItem) return;
            if (storedItem is not null) Item.InternalSetInventory(storedItem, null);
            storedItem = newItem;
            if (storedItem is not null) Item.InternalSetInventory(storedItem, this);
        }



        public IEnumerable<TItem> GetItems<TItem>()
            => GetItems().OfType<TItem>();
        public TItem? GetItem<TItem>()
            => GetItems<TItem>().FirstOrDefault();

        public bool HasItem<TItem>(int minCount = 1)
            => HasItem<TItem>(minCount, true);
        public bool HasItem<TItem>(int minCount, bool checkMultipleStacks)
        {
            int total = 0;
            foreach (Item item in GetItems())
                if (item is TItem)
                {
                    int compareCount = checkMultipleStacks ? total += item.Count : item.Count;
                    if (compareCount >= minCount) return true;
                }
            return false;
        }

    }
}
