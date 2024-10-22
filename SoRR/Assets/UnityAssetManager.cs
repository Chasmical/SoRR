using System;
using UnityEngine;

namespace SoRR
{
    /// <summary>
    ///   <para>Represents an asset manager, that loads assets from Unity's <see cref="ResourcesAPI"/>.</para>
    /// </summary>
    public sealed class UnityAssetManager : AssetManager
    {
        /// <summary>
        ///   <para>Gets the <see cref="ResourcesAPI"/> used by the asset manager.</para>
        /// </summary>
        public ResourcesAPI ResourcesApi { get; }
        /// <inheritdoc/>
        public override string DisplayName => "Unity Resources";

        /// <summary>
        ///   <para>Initializes a new instance of the <see cref="UnityAssetManager"/> with the specified <paramref name="resourcesApi"/>.</para>
        /// </summary>
        /// <param name="resourcesApi">The <see cref="ResourcesAPI"/> to use.</param>
        /// <exception cref="ArgumentNullException"><paramref name="resourcesApi"/> is <see langword="null"/>.</exception>
        public UnityAssetManager(ResourcesAPI resourcesApi)
        {
            Guard.ThrowIfNull(resourcesApi);
            ResourcesApi = resourcesApi;
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (ReferenceEquals(this, Instance))
            {
                disposed = false;
                throw new InvalidOperationException($"{nameof(UnityAssetManager)} singleton cannot be disposed.");
            }
        }

        /// <summary>
        ///   <para>Gets the <see cref="UnityAssetManager"/> singleton instance.</para>
        /// </summary>
        public static UnityAssetManager Instance { get; } = new UnityAssetManager(ResourcesAPI.ActiveAPI);

        /// <inheritdoc/>
        protected internal override object? LoadNewAssetOrNull(string assetPath)
        {
            UnityEngine.Object? asset = ResourcesApi.Load(assetPath, typeof(UnityEngine.Object));
            if (!asset) return null;

            // Load textures as sprites (since they can then be extracted through the texture property)
            if (asset is Texture2D)
                return ResourcesApi.Load(assetPath, typeof(Sprite));

            // Decode the TextAsset to a string (it's later cached in the AssetManager)
            if (asset is TextAsset textAsset)
                return textAsset.text;

            // TODO: Implement some workaround for loading byte[], maybe a name suffix
            // (Unity interprets .bytes files as TextAssets, so that's problematic...)

            return asset;
        }

    }
}
