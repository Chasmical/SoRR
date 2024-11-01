using System;
using UnityEngine;

namespace SoRR
{
    /// <summary>
    ///   <para>Represents a sprite renderer that uses an <see cref="AssetHandle"/> to load and reload sprites.</para>
    /// </summary>
    public class AssetSpriteRenderer : InjectableComponent
    {
        /// <summary>
        ///   <para>Gets the <see cref="SpriteRenderer"/> controlled by this asset sprite renderer.</para>
        /// </summary>
        [field: Inject] public SpriteRenderer Renderer { get; } = null!;

        private AssetHandle? _handle;
        /// <summary>
        ///   <para>Gets or sets the asset handle to use the sprite of.</para>
        /// </summary>
        public AssetHandle? Handle
        {
            get => _handle;
            set
            {
                AssetHandle? prev = _handle;
                if (prev == value) return;
                _handle = value;

                Action<AssetHandle> listener = RefreshAsset;
                prev?.RemoveListener(listener);
                value?.AddListener(listener);
                RefreshAsset(value);
            }
        }

        /// <summary>
        ///   <para>Gets or sets the sprite renderer's color.</para>
        /// </summary>
        public Color Color
        {
            get => Renderer.color;
            set => Renderer.color = value;
        }

        private void RefreshAsset(AssetHandle? handle)
        {
            Sprite? sprite = null;

            if (handle?.Value is { } value)
            {
                sprite = value as Sprite;
                // TODO: log a warning if sprite is null (not Sprite type)
            }
            Renderer.sprite = sprite;
        }

        /// <summary/>
        public virtual void OnDestroy()
        {
            _handle?.RemoveListener(RefreshAsset);
            _handle = null;
        }

    }
}
