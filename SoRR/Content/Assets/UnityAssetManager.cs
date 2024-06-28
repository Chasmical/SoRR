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
            => Resources.Load(path, typeof(UnityEngine.Object));

    }
}
