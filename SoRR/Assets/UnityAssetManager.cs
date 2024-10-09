using System;
using UnityEngine;

namespace SoRR
{
    public sealed class UnityAssetManager : AssetManager
    {
        public ResourcesAPI ResourcesApi { get; }
        public override string DisplayName => "Unity Resources";

        public UnityAssetManager(ResourcesAPI resourcesApi)
        {
            if (resourcesApi is null) throw new ArgumentNullException(nameof(resourcesApi));
            ResourcesApi = resourcesApi;
        }

        protected override void Dispose(bool disposing)
        {
            if (ReferenceEquals(this, Instance))
            {
                disposed = false;
                throw new InvalidOperationException($"{nameof(UnityAssetManager)} singleton cannot be disposed.");
            }
        }

        public static UnityAssetManager Instance { get; } = new UnityAssetManager(ResourcesAPI.ActiveAPI);

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
