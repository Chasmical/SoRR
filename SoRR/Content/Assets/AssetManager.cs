using System.Collections.Generic;
using UnityEngine;

namespace SoRR
{
    public abstract class AssetManager
    {
        private readonly Dictionary<string, object?> assets = [];

        public abstract string DisplayName { get; }

        protected abstract object? LoadAsset(string path);

        private object? GetAsset(string path)
        {
            if (path.Contains('\\')) path = path.Replace('\\', '/');
            if (path.StartsWith('/')) path = path[1..];

            if (!assets.TryGetValue(path, out object? asset))
                assets.Add(path, asset = LoadAsset(path));

            return asset;
        }
        private object GetAssetNotNull(string path)
            => GetAsset(path) ?? throw new AssetNotFoundException(this, path);

        public Sprite LoadSprite(string path)
            => (Sprite)GetAssetNotNull(path);
        public Texture2D LoadTexture(string path)
            => LoadSprite(path).texture;
        public AudioClip LoadAudio(string path)
            => (AudioClip)GetAssetNotNull(path);
        public string LoadText(string path)
            => (string)GetAssetNotNull(path);
        public byte[] LoadBinary(string path)
            => (byte[])GetAssetNotNull(path);

    }
}
