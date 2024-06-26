using UnityEngine;

namespace SoRR
{
    public abstract class IndexedSpriteRenderer : Injectable
    {
        [Inject] private readonly SpriteRenderer renderer = null!;

        protected string? _spriteName;
        protected int _spriteIndex;
        [MaybeFakeNull] private Sprite?[]? indexedSprites;

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
            indexedSprites = newSpriteName is null ? null : GetIndexedSprites(newSpriteName);
        }
        private void SetRendererSpriteIndex(int index)
        {
            _spriteIndex = index;
            renderer.sprite = indexedSprites?[index];
        }

        [return: MaybeFakeNull]
        protected abstract Sprite?[] GetIndexedSprites(string spriteName);

    }
}
