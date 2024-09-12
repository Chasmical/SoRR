using System;
using System.IO;
using JetBrains.Annotations;

namespace SoRR
{
    public sealed class FileSystemAssetManager : ExternalAssetManagerBase
    {
        public string DirectoryPath { get; }
        public override string DisplayName => DirectoryPath;

        public FileSystemAssetManager(string directoryPath)
        {
            if (directoryPath is null) throw new ArgumentNullException(nameof(directoryPath));
            DirectoryPath = Path.GetFullPath(directoryPath);

            // TODO: set up a FileSystemWatcher monitoring the directory's contents
        }

        protected override IAssetLoadInfo? GetAssetInfo(string path)
        {
            // TODO: find the files matching the specified path

            // TODO: determine which one is the main asset, and which one is the metadata

            // TODO: if the main asset could not be found, return null

            throw new NotImplementedException();
        }

        public readonly record struct AssetInfo(string AssetPath, string? MetadataPath) : IAssetLoadInfo
        {
            // Note: when exposing a MemoryStream with byte[], make sure to make it exposable/publiclyVisible
            public AssetFormat Format => AssetUtility.DetectFormat(AssetPath);
            [MustDisposeResource]
            public Stream OpenAsset() => File.OpenRead(AssetPath);
            [MustDisposeResource]
            public Stream? OpenMetadata() => MetadataPath is null ? null : File.OpenRead(MetadataPath);
        }

    }
}
