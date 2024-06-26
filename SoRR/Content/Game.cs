using System;

namespace SoRR
{
    public static class Game
    {
        public static TItem CreateItem<TItem>(int count) where TItem : Item
            => (TItem)CreateItem(ItemMetadata.Get<TItem>(), count);
        public static Item CreateItem(Type itemType, int count)
            => CreateItem(ItemMetadata.Get(itemType), count);

        internal static Item CreateItem(ItemMetadata metadata, int count)
        {
            Item item = (Item)Activator.CreateInstance(metadata.Type);

            item.Count = count;
            item.InitialSetup(metadata);

            return item;
        }

    }
}
