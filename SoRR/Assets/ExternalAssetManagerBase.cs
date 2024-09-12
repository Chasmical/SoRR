using System;
using System.IO;
using System.Text;
using JetBrains.Annotations;
using UnityEngine;

namespace SoRR
{
    public abstract class ExternalAssetManagerBase : AssetManager
    {
        protected abstract IAssetLoadInfo? GetAssetInfo(string path);

        protected override object? LoadAsset(string path)
        {
            IAssetLoadInfo? info = GetAssetInfo(path);
            return info is null ? null : CreateAssetFromStream(info, path);
        }

        private static object CreateAssetFromStream(IAssetLoadInfo info, string path)
        {
            byte[] assetData;
            using (Stream assetStream = info.OpenAsset())
                assetData = assetStream.ToByteArrayDangerous();

            switch (info.Format.ToType())
            {
                case AssetType.IMAGE:
                {
                    SpriteMetadata metadata = CreateMetadataFromStream<SpriteMetadata>(info);
                    return AssetUtility.CreateSprite(assetData, metadata.region, metadata.ppu);
                }
                case AssetType.AUDIO:
                {
                    return AssetUtility.CreateAudioClipAsync(assetData).Result;
                }
                case AssetType.VIDEO:
                {
                    throw new NotImplementedException("Video asset loading not implemented yet.");
                }
                case AssetType.TEXT:
                {
                    // Note: do not use TextAsset, as it decodes the string from bytes every time it's accessed
                    return Encoding.UTF8.GetString(assetData);
                }
                case AssetType.BINARY:
                {
                    return assetData;
                }
                default:
                {
                    throw new InvalidOperationException($"Asset '{path}' is of unknown type.");
                }
            }
        }
        private static T? CreateMetadataFromStream<T>(IAssetLoadInfo info) where T : new()
        {
            Stream? metadataStream = info.OpenMetadata();
            return metadataStream is null ? new T() : FileUtility.ReadJson<T>(metadataStream);
        }

        public struct SpriteMetadata
        {
            public SpriteMetadata() { }
            public float ppu = 64f;
            public Rect? region;
        }
    }
    public interface IAssetLoadInfo
    {
        [MustDisposeResource] Stream OpenAsset();
        [MustDisposeResource] Stream? OpenMetadata();
        AssetFormat Format { get; }
    }
}
