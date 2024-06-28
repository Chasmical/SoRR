using UnityEngine;

namespace SoRR
{
    internal static class Assets
    {
        public static UnityAssetManager Manager { get; } = new(ResourcesAPI.ActiveAPI);

        public static T Load<T>(string path) where T : notnull
            => Manager.Load<T>(path);

    }
}
