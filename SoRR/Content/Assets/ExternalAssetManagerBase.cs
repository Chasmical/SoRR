using System;
using System.Buffers;
using System.IO;
using System.Text;
using UnityEngine;
using YamlDeserializer = YamlDotNet.Serialization.Deserializer;

namespace SoRR
{
    public abstract class ExternalAssetManagerBase : AssetManager
    {
        protected abstract IAssetLoadInfo? GetAssetInfo(string path);

        protected override object? LoadAsset(string path)
        {
            IAssetLoadInfo? info = GetAssetInfo(path);
            return info is null ? null : ResolveAsset(info, path);
        }

        private static object ResolveAsset(IAssetLoadInfo info, string path)
        {
            byte[] assetData;
            using (Stream assetStream = info.OpenAsset())
                assetData = ReadAssetStream(assetStream);

            switch (info.Type)
            {
                case AssetType.IMAGE:
                {
                    SpriteMetadata metadata = ResolveMetadata<SpriteMetadata>(info);
                    return AssetUtility.CreateSprite(assetData, metadata.region, metadata.ppu);
                }

                case AssetType.AUDIO:
                    return AssetUtility.CreateAudioClip(assetData);

                case AssetType.VIDEO:
                    throw new NotImplementedException("Video asset loading not implemented yet.");

                case AssetType.TEXT:
                    return Encoding.UTF8.GetString(assetData);

                case AssetType.BINARY:
                    return assetData;

                default:
                    throw new InvalidOperationException($"Asset '{path}' is of unknown type.");
            }
        }
        private static T? ResolveMetadata<T>(IAssetLoadInfo info) where T : new()
        {
            Stream? metadataStream = info.OpenMetadata();
            if (metadataStream is null) return new T();

            using (metadataStream)
            using (StreamReader reader = new StreamReader(metadataStream))
            {
                return new YamlDeserializer().Deserialize<T>(reader);
            }
        }

        private static byte[] ReadAssetStream(Stream stream)
        {
            if (!TryGetStreamLength(stream, out int byteLength))
            {
                // If the asset's size can't be determined, use a MemoryStream
                MemoryStream memory = new();
                stream.CopyTo(memory);
                return memory.ToArray();
            }

            // If the asset's size is known, rent and use a suitable buffer
            using (IMemoryOwner<byte> buffer = MemoryPool<byte>.Shared.Rent(byteLength))
            {
                Memory<byte> memory = buffer.Memory;
                if (stream.Read(memory.Span) != byteLength)
                    throw new InvalidOperationException("Stream byte length mismatch.");
                return memory.ToArray();
            }
        }
        private static bool TryGetStreamLength(Stream stream, out int byteLength)
        {
            // Try to determine the stream's length in bytes
            try
            {
                byteLength = checked((int)stream.Length);
                return true;
            }
            catch (NotSupportedException)
            {
                byteLength = -1;
                return false;
            }
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
        public Stream OpenAsset();
        public Stream? OpenMetadata();
        public AssetType Type { get; }
    }
}
