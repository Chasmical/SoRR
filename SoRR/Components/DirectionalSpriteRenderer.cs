using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace SoRR
{
    public abstract class DirectionalSpriteRenderer<TDir> : IndexedSpriteRenderer where TDir : struct, Enum
    {
        public TDir Direction
        {
            get => Unsafe.As<int, TDir>(ref _spriteIndex);
            set => SetDirection(value);
        }

        public void SetDirection(TDir newDirection)
            => SetSpriteIndex(Unsafe.As<TDir, int>(ref newDirection));
        public void SetSprite(string? newSpriteName, TDir newDirection)
            => SetSprite(newSpriteName, Unsafe.As<TDir, int>(ref newDirection));

        [return: MaybeFakeNull]
        protected sealed override Sprite?[] GetIndexedSprites(string spriteName)
        {
            int count = SoRR.Direction.CountOf<TDir>();
            Sprite?[] sprites = new Sprite?[count];
            for (int i = 0; i < count; i++)
            {
                TDir dir = Unsafe.As<int, TDir>(ref i);
                // TODO: Replace with a ResourceService
                sprites[i] = Resources.Load<Sprite>(spriteName + GetDirectionSuffix(dir));
            }
            return sprites;
        }

        protected abstract string GetDirectionSuffix(TDir dir);

    }
    public class Dir4SpriteRenderer : DirectionalSpriteRenderer<Dir4>
    {
        protected override string GetDirectionSuffix(Dir4 dir) => dir.ToLetters();
    }
    public class Dir8SpriteRenderer : DirectionalSpriteRenderer<Dir8>
    {
        protected override string GetDirectionSuffix(Dir8 dir) => dir.ToLetters();
    }
    public class Dir24SpriteRenderer : DirectionalSpriteRenderer<Dir24>
    {
        protected override string GetDirectionSuffix(Dir24 dir) => dir.ToString();
    }
}
