using System;
using System.Collections.Generic;
using System.Linq;

namespace SoRR
{
    public abstract class Inventory
    {
        public abstract IEnumerable<Item> GetItems();
        
        public abstract bool TryAddItem(Item newItem);

        public abstract bool TryRemoveItem(Item item);

        public IEnumerable<TItem> GetItems<TItem>()
            => GetItems().OfType<TItem>();
        public TItem? GetItem<TItem>()
            => GetItems<TItem>().FirstOrDefault();

        public IEnumerable<Item> GetItems(Type itemType)
            => GetItems().Where(item => item.Metadata.Type == itemType);
        public Item? GetItem(Type itemType)
            => GetItems(itemType).FirstOrDefault();

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
