using System;
using UnityEngine;

namespace SoRR
{
    public sealed class UnityAssetManager : AssetManager
    {
        public ResourcesAPI Resources { get; }

        public UnityAssetManager(ResourcesAPI resourcesApi)
            => Resources = resourcesApi ?? throw new ArgumentNullException(nameof(resourcesApi));

        public override string DisplayName => "Unity Resources";

        protected override object? LoadAsset(string path)
        {
            object? asset = Resources.Load(path, typeof(UnityEngine.Object));

            // Decode the TextAsset to a string (it's later cached in the AssetManager)
            if (asset is TextAsset textAsset)
                asset = textAsset.text;

            // TODO: implement some workaround for loading byte[], maybe a name suffix
            // (Unity interprets .bytes files as TextAssets)

            return asset;
        }

    }
}
