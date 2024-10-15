using System;
using JetBrains.Annotations;

namespace SoRR
{
    /// <summary>
    ///   <para>Represents an indexed sprite renderer, that preloads a set of assets and efficiently switches between them.</para>
    /// </summary>
    public abstract class IndexedSpriteRenderer : AssetSpriteRenderer
    {
        private AssetHandle?[]? spriteAssets;
        /// <summary>
        ///   <para>The current sprite group name.</para>
        /// </summary>
        protected string? _spriteName;
        /// <summary>
        ///   <para>The current sprite index.</para>
        /// </summary>
        protected int _spriteIndex;

        /// <inheritdoc cref="AssetSpriteRenderer.Handle"/>
        [Obsolete($"{nameof(IndexedSpriteRenderer)}'s asset handle should not be set from outside.")]
        public new AssetHandle? Handle => base.Handle;

        /// <summary>
        ///   <para>Gets or sets the sprite renderer's sprite group name.</para>
        /// </summary>
        public string? SpriteName
        {
            get => _spriteName;
            set => SetSprite(value);
        }
        /// <summary>
        ///   <para>Gets or sets the sprite renderer's sprite index.</para>
        /// </summary>
        public int SpriteIndex
        {
            get => _spriteIndex;
            set => SetSpriteIndex(value);
        }

        /// <summary>
        ///   <para>Sets the sprite renderer's sprite group name.</para>
        /// </summary>
        /// <param name="newSpriteName">The new sprite group name to set.</param>
        public void SetSprite(string? newSpriteName)
            => SetSprite(newSpriteName, _spriteIndex);
        /// <summary>
        ///   <para>Sets the sprite renderer's sprite group name and index.</para>
        /// </summary>
        /// <param name="newSpriteName">The new sprite group name to set.</param>
        /// <param name="newIndex">The new sprite index to set.</param>
        public void SetSprite(string? newSpriteName, int newIndex)
        {
            if (newSpriteName != _spriteName) UpdateIndexedSprites(newSpriteName);
            else if (newIndex == _spriteIndex) return;

            SetRendererSpriteIndex(newIndex);
        }
        /// <summary>
        ///   <para>Sets the sprite renderer's sprite index.</para>
        /// </summary>
        /// <param name="newIndex">The new sprite index to set.</param>
        public void SetSpriteIndex(int newIndex)
        {
            if (newIndex == _spriteIndex) return;
            SetRendererSpriteIndex(newIndex);
        }

        /// <summary>
        ///   <para>Reloads the assets used by this sprite renderer.</para>
        /// </summary>
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

        /// <summary>
        ///   <para>When overridden in a derived class, returns an array of asset handles of sprites to use, optionally re-using an existing array.</para>
        /// </summary>
        /// <param name="spriteName">The sprite group name to get the asset handles of.</param>
        /// <param name="prevAssets">An array of asset handles to optionally re-use, or <see langword="null"/>.</param>
        /// <returns>An array of asset handles of sprites to use.</returns>
        [Pure] protected abstract AssetHandle?[] GetSpriteAssets(string spriteName, AssetHandle?[]? prevAssets);

    }
}
