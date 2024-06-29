using System;
using System.IO;
using Chasm.Collections;

namespace SoRR
{
    public sealed class FileSystemAssetManager : ExternalAssetManagerBase
    {
        public string DirectoryPath { get; }
        public override string DisplayName => DirectoryPath;

        public FileSystemAssetManager(string directoryPath)
            => DirectoryPath = Path.GetFullPath(directoryPath);

        protected override IAssetLoadInfo? GetAssetInfo(string path)
        {
            string assetName = Path.GetFileName(path);
            string? assetDirectory = Path.GetDirectoryName(path);

            // Append the asset's directory to the current one, so that we can use a pattern search
            string searchDirectory = DirectoryPath;
            if (assetDirectory is not null)
                searchDirectory = Path.Combine(searchDirectory, assetDirectory);

            if (!Directory.Exists(searchDirectory)) return null;

            // Pattern search is used here as a sort of Bloom filter, to narrow down the files
            string[] matches = Directory.GetFiles(searchDirectory, assetName + ".*");

            // Manually filter out unexpected results, such as "Sprite.2.png" for "Sprite.*"
            matches = matches.FindAll(file => Path.GetFileNameWithoutExtension(file) == assetName);

            if (matches.Length == 0) return null;

            // Find the metadata file, and pick the first non-metadata file as the asset
            int metadataIndex = Array.FindIndex(matches, file => Path.GetExtension(file) == ".yml");
            int assetIndex = metadataIndex > 0 ? 0 : metadataIndex + 1;

            if (assetIndex >= matches.Length) return null;

            return new AssetInfo(
                matches[assetIndex],
                metadataIndex >= 0 ? matches[metadataIndex] : null
            );
        }

        public readonly record struct AssetInfo(string AssetPath, string? MetadataPath) : IAssetLoadInfo
        {
            public AssetFormat Format => AssetUtility.DetectFormat(AssetPath);
            public Stream OpenAsset() => File.OpenRead(AssetPath);
            public Stream? OpenMetadata() => MetadataPath is null ? null : File.OpenRead(MetadataPath);
        }
    }
}
