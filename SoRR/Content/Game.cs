using System;
using UnityEngine;

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

        public static DroppedItem DropItem(Item item, Vector2 position)
        {
            GameObject go = new("Dropped Item: " + item.Metadata.Type.Name);
            go.transform.position = position;

            DroppedItem dropped = (DroppedItem)go.AddComponent(item.Metadata.DroppedItemType);
            dropped.InitialSetup(item);

            return dropped;
        }

        public static TAgent SpawnAgent<TAgent>(Vector2 position) where TAgent : Agent
            => (TAgent)SpawnAgent(AgentMetadata.Get<TAgent>(), position);
        public static Agent SpawnAgent(AgentMetadata metadata, Vector2 position)
        {
            GameObject go = new GameObject("Agent: " + metadata.Type.Name);
            go.transform.position = position;

            Agent agent = (Agent)go.AddComponent(metadata.Type);
            agent.InitialSetup(metadata);

            return agent;
        }

    }
}
