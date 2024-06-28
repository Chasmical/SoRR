using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Compression;

namespace SoRR
{
    public sealed class ZipArchiveAssetManager : ExternalAssetManagerBase
    {
        public ZipArchive Archive { get; }
        public override string DisplayName { get; }

        private string? prefixDirectory;
        public string? PrefixDirectory
        {
            get => prefixDirectory;
            set
            {
                if (value is not null)
                {
                    if (value.StartsWith('/')) value = value[1..];
                    if (!value.EndsWith('/')) value += '/';
                }
                if (value != prefixDirectory)
                {
                    prefixDirectory = value;
                    lookupDict = null;
                }
            }
        }

        private Dictionary<string, AssetInfo>? lookupDict;

        public ZipArchiveAssetManager(string archivePath)
        {
            Archive = ZipFile.OpenRead(archivePath);
            DisplayName = Path.GetFileName(archivePath);
        }

        [MemberNotNull(nameof(lookupDict))]
        private void InitializeLookupDict()
        {
            Dictionary<string, AssetInfo> lookup = [];
            string? prefix = PrefixDirectory;

            foreach (ZipArchiveEntry entry in Archive.Entries)
            {
                string assetPath = entry.FullName;
                if (prefix is not null && !assetPath.StartsWith(prefix)) continue;

                string extension = Path.GetExtension(assetPath);
                if (extension == ".yml") continue;

                // Determine the asset's declared name, by which it will be accessed
                string declaredName = assetPath;
                if (prefix is not null)
                    declaredName = declaredName[prefix.Length..];

                // Try to find the asset's metadata file
                string metadataPath = Path.ChangeExtension(assetPath, ".yml");
                ZipArchiveEntry? metadata = Archive.GetEntry(metadataPath);

                lookup.Add(declaredName, new AssetInfo(entry, metadata));
            }
            lookupDict = lookup;
        }

        protected override IAssetLoadInfo? GetAssetInfo(string path)
        {
            if (lookupDict is null) InitializeLookupDict();

            if (!lookupDict.TryGetValue(path, out AssetInfo assetInfo))
                return null;

            return assetInfo;
        }

        public readonly record struct AssetInfo(ZipArchiveEntry Asset, ZipArchiveEntry? Metadata) : IAssetLoadInfo
        {
            public AssetType Type => AssetUtility.DetectAssetType(Asset.FullName);
            public Stream OpenAsset() => Asset.Open();
            public Stream? OpenMetadata() => Metadata?.Open();
        }
    }
}
