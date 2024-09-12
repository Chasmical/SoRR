using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Compression;
using JetBrains.Annotations;

namespace SoRR
{
    public sealed class ZipArchiveAssetManager : ExternalAssetManagerBase, IDisposable
    {
        public ZipArchive Archive { get; }
        public override string DisplayName { get; }

        public ZipArchiveAssetManager(string archivePath)
        {
            // TODO: load zip archive into memory to allow hot reload?
            Archive = ZipFile.OpenRead(archivePath);
            DisplayName = Path.GetFileName(archivePath);

            // TODO: set up a FileSystemWatcher monitoring the archive file
        }

        public void Dispose()
        {
            Archive.Dispose();
        }

        private Dictionary<string, AssetInfo>? lookupDict;

        [MemberNotNull(nameof(lookupDict))]
        private void InitializeLookup()
        {
            Dictionary<string, AssetInfo> dict = [];

            foreach (ZipArchiveEntry entry in Archive.Entries)
            {
                string assetPath = entry.FullName;
                int extensionLength = Path.GetExtension(assetPath.AsSpan()).Length;
                string declaredName = assetPath[..^extensionLength];

                if (dict.TryGetValue(declaredName, out AssetInfo prev))
                {
                    // TODO: Resolve conflicting names (probably metadata got loaded before the asset, or vice versa)
                    continue;
                }

                string metadataPath = Path.ChangeExtension(assetPath, ".json");
                ZipArchiveEntry? metadataEntry = Archive.GetEntry(metadataPath);

                dict[declaredName] = new AssetInfo(entry, metadataEntry);
            }

            lookupDict = dict;
        }

        protected override IAssetLoadInfo? GetAssetInfo(string path)
        {
            if (lookupDict is null) InitializeLookup();
            return lookupDict.TryGetValue(path, out AssetInfo asset) ? asset : null;
        }

        public readonly record struct AssetInfo(ZipArchiveEntry Asset, ZipArchiveEntry? Metadata) : IAssetLoadInfo
        {
            // Note: when exposing a MemoryStream with byte[], make sure to make it exposable/publiclyVisible
            public AssetFormat Format => AssetUtility.DetectFormat(Asset.FullName);
            [MustDisposeResource]
            public Stream OpenAsset() => Asset.Open();
            [MustDisposeResource]
            public Stream? OpenMetadata() => Metadata?.Open();
        }

    }
}
