using JetBrains.Annotations;
using UnityEngine;

namespace SoRR
{
    public abstract class IndexedSpriteRenderer : MonoBehaviour
    {
        [field: Inject] public SpriteRenderer Renderer { get; } = null!;

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
        public Color Color
        {
            get => Renderer.color;
            set => Renderer.color = value;
        }

        protected virtual void Awake()
            => Injector.Inject(this);

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
            Renderer.sprite = indexedSprites?[index];
        }

        [Pure] [return: MaybeFakeNull]
        protected abstract Sprite?[] GetIndexedSprites(string spriteName);

    }
}
