using System;
using System.IO;
using System.Text;
using JetBrains.Annotations;
using UnityEngine;

namespace SoRR
{
    /// <summary>
    ///   <para>Implements a base for asset managers, that load assets from an external source.</para>
    /// </summary>
    public abstract class ExternalAssetManagerBase : AssetManager
    {
        /// <summary>
        ///   <para>Gets information about an external asset at the specified path.</para>
        /// </summary>
        /// <param name="assetPath">A relative path to the external asset to load.</param>
        /// <returns>An <see cref="IExternalAssetInfo"/> object representing information about the specified external asset, or <see langword="null"/> if it is not found.</returns>
        protected abstract IExternalAssetInfo? GetAssetInfo(string assetPath);

        /// <inheritdoc/>
        protected internal override object? LoadNewAssetOrNull(string assetPath)
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

        /// <summary>
        ///   <para>Contains information on how an external sprite should be loaded.</para>
        /// </summary>
        public struct SpriteMetadata()
        {
            /// <summary>
            ///   <para>The external sprite's pixels-per-unit.</para>
            /// </summary>
            public float ppu = 64f;
            /// <summary>
            ///   <para>The external sprite's texture region, or <see langword="null"/> to use the entire texture.</para>
            /// </summary>
            public Rect? region;
        }
    }
    /// <summary>
    ///   <para>Defines methods for loading an external asset.</para>
    /// </summary>
    public interface IExternalAssetInfo
    {
        /// <summary>
        ///   <para>Opens the external asset as a stream.</para>
        /// </summary>
        /// <returns>The stream that represents the external asset's raw data.</returns>
        [MustDisposeResource] Stream OpenAsset();
        /// <summary>
        ///   <para>Opens the external asset's associated metadata as a stream.</para>
        /// </summary>
        /// <returns>The stream that represents the external asset's raw metadata, or <see langword="null"/> if there is no metadata.</returns>
        [MustDisposeResource] Stream? OpenMetadata();
        /// <summary>
        ///   <para>Gets the external asset's format.</para>
        /// </summary>
        AssetFormat Format { get; }
    }
}
