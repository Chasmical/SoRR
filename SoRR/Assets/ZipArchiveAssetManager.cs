using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading;
using JetBrains.Annotations;

namespace SoRR
{
    public sealed class ZipArchiveAssetManager : ExternalAssetManagerBase
    {
        public string ArchivePath { get; }
        public override string DisplayName => $"\"{ArchivePath}\"";

        private readonly object stateLock = new();

        private ZipArchive? _archive;
        private IReadOnlyDictionary<string, AssetInfo>? _lookup;

        public ZipArchiveAssetManager(string archivePath)
        {
            if (archivePath is null) throw new ArgumentNullException(nameof(archivePath));
            ArchivePath = Path.GetFullPath(archivePath);

            // TODO: implement the archive file watcher
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            Interlocked.Exchange(ref _archive, null)?.Dispose();
            _lookup = null;
        }

        private void InitArchive()
        {
            Debug.Assert(_archive is null);
            Debug.Assert(Monitor.IsEntered(stateLock));

            try
            {
                MemoryStream memory = new(File.ReadAllBytes(ArchivePath));
                _archive = new ZipArchive(memory, ZipArchiveMode.Read);
            }
            catch
            {
                // If the file doesn't exist, or if there's a file-sharing error, set to null
                _archive = null;
            }
        }

        private IReadOnlyDictionary<string, AssetInfo> CreateLookup()
        {
            Debug.Assert(_lookup is null);
            Debug.Assert(Monitor.IsEntered(stateLock));

            // Try to read the archive file
            if (_archive is null) InitArchive();

            Dictionary<string, AssetInfo> lookup = [];
            if (_archive is not null)
            {
                // Enumerate the entries to populate the lookup
                foreach (ZipArchiveEntry entry in _archive.Entries)
                {
                    string assetPath = entry.FullName;
                    ReadOnlySpan<char> assetExtension = Path.GetExtension(assetPath.AsSpan());
                    if (assetExtension is ".meta") continue;

                    string declaredName = assetPath[..^assetExtension.Length];

                    if (lookup.TryGetValue(declaredName, out AssetInfo prev))
                    {
                        // TODO: log a warning, if there's more than one asset path
                        continue;
                    }

                    string metadataPath = Path.ChangeExtension(assetPath, ".meta");
                    ZipArchiveEntry? metadataEntry = _archive.GetEntry(metadataPath);

                    lookup[declaredName] = new AssetInfo(entry, metadataEntry);
                }
            }
            return lookup;
        }

        protected override IExternalAssetInfo? GetAssetInfo(string assetPath)
        {
            IReadOnlyDictionary<string, AssetInfo>? lookup = _lookup;
            if (lookup is null)
                lock (stateLock)
                {
                    lookup = _lookup;
                    if (lookup is null) _lookup = lookup = CreateLookup();
                }

            return lookup.TryGetValue(assetPath, out AssetInfo info) ? info : null;
        }

        public readonly struct AssetInfo(ZipArchiveEntry assetEntry, ZipArchiveEntry? metadataEntry) : IExternalAssetInfo
        {
            public AssetFormat Format => AssetUtility.DetectFormat(assetEntry.FullName);
            [MustDisposeResource]
            public Stream OpenAsset() => assetEntry.Open();
            [MustDisposeResource]
            public Stream? OpenMetadata() => metadataEntry?.Open();
        }

    }
}
