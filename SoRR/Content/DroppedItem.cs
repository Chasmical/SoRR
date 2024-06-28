using UnityEngine;

namespace SoRR
{
    public class DroppedItem : Entity
    {
        public Item Item { get; private set; } = null!;

        internal void InitialSetup(Item item)
        {
            Item = item;
        }

    }
    public class SimpleDroppedItem : DroppedItem
    {
        [Inject] private readonly SpriteRenderer spriteRenderer = null!;

        public void Start()
        {
            spriteRenderer.sprite = Assets.Load<Sprite>($"Sprites/Items/{Item.Metadata.Name}");
        }

    }
}
