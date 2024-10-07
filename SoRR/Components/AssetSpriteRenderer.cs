using System;
using UnityEngine;

namespace SoRR
{
    public class AssetSpriteRenderer : MonoBehaviour
    {
        [field: Inject] public SpriteRenderer Renderer { get; } = null!;

        private AssetHandle? _handle;
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

        public Color Color
        {
            get => Renderer.color;
            set => Renderer.color = value;
        }

        private void RefreshAsset(AssetHandle? handle)
            => Renderer.sprite = handle?.Value as Sprite;

        public virtual void Awake()
            => Injector.Inject(this);

        public virtual void OnDestroy()
        {
            _handle?.RemoveListener(RefreshAsset);
            _handle = null;
        }

    }
}
