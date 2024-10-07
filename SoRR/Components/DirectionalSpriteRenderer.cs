using System;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace SoRR
{
    public abstract class DirectionalSpriteRenderer<TDir> : IndexedSpriteRenderer where TDir : struct, Enum
    {
        public TDir Direction
        {
            get => Unsafe.As<int, TDir>(ref _spriteIndex);
            set => SetDirection(value);
        }

        public void SetSprite(string? newSpriteName, TDir newDirection)
            => SetSprite(newSpriteName, Unsafe.As<TDir, int>(ref newDirection));
        public void SetDirection(TDir newDirection)
            => SetSpriteIndex(Unsafe.As<TDir, int>(ref newDirection));

        [Pure] protected override AssetHandle?[] GetSpriteAssets(string spriteName, AssetHandle?[]? prev)
        {
            int count = SoRR.Direction.CountOf<TDir>();
            AssetHandle?[] assets = prev?.Length == count ? prev : new AssetHandle?[count];

            Span<char> path = stackalloc char[spriteName.Length + DirExtensions.MaxToLettersLength];
            spriteName.AsSpan().CopyTo(path);

            for (int i = 0; i < count; i++)
            {
                TDir direction = Unsafe.As<int, TDir>(ref i);

                string suffix = GetDirectionSuffix(direction);
                suffix.AsSpan().CopyTo(path[spriteName.Length..]);

                int totalLength = spriteName.Length + suffix.Length;
                assets[i] = Assets.GetHandle(path[..totalLength]);
            }
            return assets;
        }

        [Pure] protected abstract string GetDirectionSuffix(TDir direction);

    }
    public sealed class Dir4SpriteRenderer : DirectionalSpriteRenderer<Dir4>
    {
        [Pure] protected override string GetDirectionSuffix(Dir4 direction)
            => direction.ToLetters();
    }
    public sealed class Dir8SpriteRenderer : DirectionalSpriteRenderer<Dir8>
    {
        [Pure] protected override string GetDirectionSuffix(Dir8 direction)
            => direction.ToLetters();
    }
    public sealed class Dir24SpriteRenderer : DirectionalSpriteRenderer<Dir24>
    {
        [Pure] protected override string GetDirectionSuffix(Dir24 direction)
            => direction.ToLetters();
    }
}
