using UnityEngine;

namespace SoRR
{
    public class DroppedItem : Entity
    {
        public Item Item { get; private set; } = null!;

        [Inject] protected readonly SpriteRenderer spriteRenderer = null!;

        internal void InitialSetup(Item item)
        {
            Item = item;
        }

        protected virtual void Start()
        {
            spriteRenderer.sprite = Assets.Load<Sprite>($"Sprites/Items/{Item.Metadata.Name}");
        }

    }
}
