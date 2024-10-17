using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading;
using JetBrains.Annotations;

namespace SoRR
{
    /// <summary>
    ///   <para>Represents an asset manager, that loads assets from a specified archive file.</para>
    /// </summary>
    public sealed class ZipArchiveAssetManager : ExternalAssetManagerBase
    {
        /// <summary>
        ///   <para>Gets the full path to the archive file that the asset manager loads assets from.</para>
        /// </summary>
        public string ArchivePath { get; }
        /// <inheritdoc/>
        public override string DisplayName => $"\"{ArchivePath}\"";

        private readonly object stateLock = new();

        private ZipArchive? _archive;
        private IReadOnlyDictionary<string, AssetInfo>? _lookup;

        private FileSystemWatcher? _watcher;
        private bool observeChanges = true;
        /// <summary>
        ///   <para>Gets or sets whether the changes in the archive file should trigger a reload.</para>
        /// </summary>
        public bool ObserveChanges
        {
            get => observeChanges;
            set
            {
                if (observeChanges == value) return;
                lock (stateLock)
                {
                    observeChanges = value;
                    if (value) CreateWatcher();
                    else DisposeWatcher();
                }
            }
        }

        /// <summary>
        ///   <para>Initializes a new instance of the <see cref="ZipArchiveAssetManager"/> class with the specified <paramref name="archivePath"/>.</para>
        /// </summary>
        /// <param name="archivePath">A path to the archive to load assets from.</param>
        /// <exception cref="ArgumentNullException"><paramref name="archivePath"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="archivePath"/> is not a valid file path.</exception>
        /// <exception cref="NotSupportedException"><paramref name="archivePath"/> contains a colon (":") that is not part of a volume identifier (for example, "c:\").</exception>
        /// <exception cref="PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length.</exception>
        public ZipArchiveAssetManager(string archivePath)
        {
            if (archivePath is null) throw new ArgumentNullException(nameof(archivePath));
            ArchivePath = Path.GetFullPath(archivePath);

            // TODO: implement the archive file watcher
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            DisposeArchive();
            DisposeWatcher();
            _lookup = null;
        }
        /// <inheritdoc/>
        protected override IExternalAssetInfo? GetAssetInfo(string assetPath)
        {
            var lookup = Locked.Get(ref _lookup, stateLock, InitializeLookup);
            return lookup.TryGetValue(assetPath, out AssetInfo info) ? info : null;
        }

        private IReadOnlyDictionary<string, AssetInfo> InitializeLookup()
        {
            Debug.Assert(Monitor.IsEntered(stateLock));

            // Before reading, ensure the archive file is being watched, if so specified
            if (observeChanges && _watcher is null) CreateWatcher();

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
                    if (assetExtension is "" or ".meta") continue;

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
        private void DisposeArchive()
        {
            Interlocked.Exchange(ref _archive, null)?.Dispose();
        }

        private void CreateWatcher()
        {
            Debug.Assert(_watcher is null);
            Debug.Assert(Monitor.IsEntered(stateLock));

            _watcher = new FileSystemWatcher(Path.GetDirectoryName(ArchivePath)!, Path.GetFileName(ArchivePath));

            FileSystemEventHandler handler = HandleChangedFile;
            _watcher.Changed += handler;
            _watcher.Created += handler;
            _watcher.Deleted += handler;
            _watcher.Renamed += new RenamedEventHandler(handler);

            _watcher.EnableRaisingEvents = true;
        }
        private void DisposeWatcher()
        {
            Interlocked.Exchange(ref _archive, null)?.Dispose();
        }

        private void HandleChangedFile(object sender, FileSystemEventArgs args)
        {
            if (sender != _watcher) return;

            lock (stateLock)
            {
                var oldLookup = _lookup;
                // If the lookup hasn't been initialized yet, then there aren't any assets that need reloading
                if (oldLookup is null) return;

                // Dispose the archive, and reinitialize archive and lookup
                DisposeArchive();
                var newLookup = InitializeLookup();
                _lookup = newLookup;

                // Enqueue the asset reloading on the main thread
                MainThread.Enqueue(() =>
                {
                    foreach (var (path, oldAsset) in oldLookup)
                    {
                        // If the asset was removed or its Crc32 changed, trigger a reload
                        if (!newLookup.TryGetValue(path, out AssetInfo newAsset) || newAsset.Crc32 != oldAsset.Crc32)
                        {
                            ReloadAssetPath(path);
                        }
                    }
                });
            }
        }

        private readonly struct AssetInfo(ZipArchiveEntry assetEntry, ZipArchiveEntry? metadataEntry) : IExternalAssetInfo
        {
            public uint Crc32 => assetEntry.Crc32;
            public AssetFormat Format => AssetUtility.DetectFormat(assetEntry.FullName);

            [MustDisposeResource]
            public Stream OpenAsset() => assetEntry.Open();
            [MustDisposeResource]
            public Stream? OpenMetadata() => metadataEntry?.Open();
        }

    }
}
