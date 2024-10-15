using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace SoRR
{
    /// <summary>
    ///   <para>Represents a directional sprite renderer, that uses direction enums to index sprite groups.</para>
    /// </summary>
    /// <typeparam name="TDir">The direction enumeration type.</typeparam>
    public sealed class DirectionalSpriteRenderer<TDir> : IndexedSpriteRenderer where TDir : struct, Enum
    {
        /// <summary>
        ///   <para>Gets or sets the sprite renderer's direction.</para>
        /// </summary>
        public TDir Direction
        {
            get => Unsafe.As<int, TDir>(ref _spriteIndex);
            set => SetDirection(value);
        }

        /// <summary>
        ///   <para>Sets the sprite renderer's sprite group name and direction.</para>
        /// </summary>
        /// <param name="newSpriteName">The new sprite group name to set.</param>
        /// <param name="newDirection">The new sprite direction to set.</param>
        public void SetSprite(string? newSpriteName, TDir newDirection)
            => SetSprite(newSpriteName, Unsafe.As<TDir, int>(ref newDirection));
        /// <summary>
        ///   <para>Sets the sprite renderer's sprite direction.</para>
        /// </summary>
        /// <param name="newDirection">The new sprite direction to set.</param>
        public void SetDirection(TDir newDirection)
            => SetSpriteIndex(Unsafe.As<TDir, int>(ref newDirection));

        /// <inheritdoc/>
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

        [Pure] private static string GetDirectionSuffix(TDir direction)
        {
            if (typeof(TDir) == typeof(Dir4))
                return Unsafe.As<TDir, Dir4>(ref direction).ToLetters();
            if (typeof(TDir) == typeof(Dir8))
                return Unsafe.As<TDir, Dir8>(ref direction).ToLetters();
            if (typeof(TDir) == typeof(Dir24))
                return Unsafe.As<TDir, Dir24>(ref direction).ToLetters();

            throw new UnreachableException();
        }

    }
}
