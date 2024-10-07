using System;
using JetBrains.Annotations;

namespace SoRR
{
    public abstract class IndexedSpriteRenderer : AssetSpriteRenderer
    {
        [MaybeFakeNull] private AssetHandle?[]? spriteAssets;
        protected string? _spriteName;
        protected int _spriteIndex;

        [Obsolete($"{nameof(IndexedSpriteRenderer)}'s asset handle should not be set from outside.")]
        public new AssetHandle? Handle => base.Handle;

        public string? SpriteName
        {
            get => _spriteName;
            set => SetSprite(value);
        }
        public int SpriteIndex
        {
            get => _spriteIndex;
            set => SetSpriteIndex(value);
        }

        public void SetSprite(string? newSpriteName)
            => SetSprite(newSpriteName, _spriteIndex);
        public void SetSprite(string? newSpriteName, int newIndex)
        {
            if (newSpriteName != _spriteName) UpdateIndexedSprites(newSpriteName);
            else if (newIndex == _spriteIndex) return;

            SetRendererSpriteIndex(newIndex);
        }
        public void SetSpriteIndex(int newIndex)
        {
            if (newIndex == _spriteIndex) return;
            SetRendererSpriteIndex(newIndex);
        }

        public void Refresh()
        {
            UpdateIndexedSprites(_spriteName);
            SetRendererSpriteIndex(_spriteIndex);
        }

        private void UpdateIndexedSprites(string? newSpriteName)
        {
            _spriteName = newSpriteName;
            spriteAssets = newSpriteName is null ? null : GetSpriteAssets(newSpriteName, spriteAssets);
        }
        private void SetRendererSpriteIndex(int index)
        {
            _spriteIndex = index;
            base.Handle = spriteAssets?[index];
        }

        [Pure] protected abstract AssetHandle?[] GetSpriteAssets(string spriteName, AssetHandle?[]? prevAssets);

    }
}
