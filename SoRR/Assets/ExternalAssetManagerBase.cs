using System;
using System.IO;
using System.Text;
using JetBrains.Annotations;
using UnityEngine;

namespace SoRR
{
    public abstract class ExternalAssetManagerBase : AssetManager
    {
        protected abstract IExternalAssetInfo? GetAssetInfo(string assetPath);

        protected override object? LoadNewAssetOrNull(string assetPath)
        {
            IExternalAssetInfo? info = GetAssetInfo(assetPath);
            return info is null ? null : CreateAssetFromStream(info, assetPath);
        }

        private static object CreateAssetFromStream(IExternalAssetInfo info, string assetPath)
        {
            byte[] assetData;
            using (Stream assetStream = info.OpenAsset())
                assetData = assetStream.ToByteArrayDangerous();

            switch (info.Format.ToType())
            {
                case AssetType.IMAGE:
                {
                    SpriteMetadata metadata = CreateMetadataFromStream<SpriteMetadata>(info);
                    return AssetUtility.CreateSprite(assetData, metadata.region.GetValueOrDefault(), metadata.ppu);
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
                    throw new InvalidOperationException($"Asset \"{assetPath}\" is of unknown type.");
                }
            }
        }
        private static T CreateMetadataFromStream<T>(IExternalAssetInfo info) where T : struct
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
    public interface IExternalAssetInfo
    {
        [MustDisposeResource] Stream OpenAsset();
        [MustDisposeResource] Stream? OpenMetadata();
        AssetFormat Format { get; }
    }
}
