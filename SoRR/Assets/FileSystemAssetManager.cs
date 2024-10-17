using System;
using System.IO;
using System.Threading;
using JetBrains.Annotations;

namespace SoRR
{
    /// <summary>
    ///   <para>Represents an asset manager, that loads assets from a specified directory.</para>
    /// </summary>
    public sealed class FileSystemAssetManager : ExternalAssetManagerBase
    {
        /// <summary>
        ///   <para>Gets the full path to the directory that the asset manager loads assets from.</para>
        /// </summary>
        public string DirectoryPath { get; }
        /// <inheritdoc/>
        public override string DisplayName => $"\"{DirectoryPath}{Path.DirectorySeparatorChar}\"";

        private readonly object stateLock = new();

        private FileSystemWatcher? _watcher;
        private bool observeChanges = true;
        /// <summary>
        ///   <para>Gets or sets whether the changes in files of the directory should trigger a reload.</para>
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
        ///   <para>Initializes a new instance of the <see cref="FileSystemAssetManager"/> class with the specified <paramref name="directoryPath"/>.</para>
        /// </summary>
        /// <param name="directoryPath">A path to the directory to load assets from.</param>
        /// <exception cref="ArgumentNullException"><paramref name="directoryPath"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="directoryPath"/> is not a valid directory path.</exception>
        /// <exception cref="NotSupportedException"><paramref name="directoryPath"/> contains a colon (":") that is not part of a volume identifier (for example, "c:\").</exception>
        /// <exception cref="PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length.</exception>
        public FileSystemAssetManager(string directoryPath)
        {
            if (directoryPath is null) throw new ArgumentNullException(nameof(directoryPath));
            DirectoryPath = Path.GetFullPath(directoryPath);
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            DisposeWatcher();
        }
        /// <inheritdoc/>
        protected override IExternalAssetInfo? GetAssetInfo(string assetPath)
        {
            string? mainPath = null;
            string? metadataPath = null;

            foreach (string filePath in FileUtility.SearchFiles(DirectoryPath, assetPath))
            {
                ReadOnlySpan<char> extension = Path.GetExtension(filePath.AsSpan());

                if (extension is ".meta")
                    metadataPath = filePath;
                else
                {
                    if (mainPath is null)
                        mainPath = filePath;
                    else
                    {
                        // TODO: log a warning, if there's more than one asset path
                    }
                }
            }

            if (mainPath is null) return null;

            if (observeChanges && _watcher is null) CreateWatcher();

            return new AssetInfo(mainPath, metadataPath);
        }

        private void CreateWatcher()
        {
            Debug.Assert(_watcher is null);
            Debug.Assert(Monitor.IsEntered(stateLock));

            _watcher = new FileSystemWatcher(DirectoryPath, "*");

            FileSystemEventHandler handler = HandleChangedFile;
            _watcher.Changed += handler;
            _watcher.Created += handler;
            _watcher.Deleted += handler;
            _watcher.Renamed += HandleRenamedFile;

            _watcher.EnableRaisingEvents = true;
        }
        private void DisposeWatcher()
        {
            _watcher?.Dispose();
            _watcher = null;
        }

        private void HandleChangedFile(object sender, FileSystemEventArgs args)
        {
            if (sender != _watcher) return;
            HandleUpdatedPath(args.FullPath);
        }
        private void HandleRenamedFile(object sender, RenamedEventArgs args)
        {
            if (sender != _watcher) return;
            HandleUpdatedPath(args.OldFullPath);
            HandleUpdatedPath(args.FullPath);
        }
        private void HandleUpdatedPath(ReadOnlySpan<char> path)
        {
            // Make sure that the file is indeed in the watched directory
            if (!path.StartsWith(DirectoryPath)) return;

            // Get the relative path ("C:\dir\path\to\asset.png" ⇒ "path\to\asset.png")
            path = path[DirectoryPath.Length..];
            if (path[0] == '/') path = path[1..];

            // Remove the extension from the path ("path\to\asset.png" ⇒ "path\to\asset")
            path = path[..^Path.GetExtension(path).Length];

            // Normalize the path (replace '\' with '/'), no alloc
            Span<char> normalizedPath = stackalloc char[path.Length];
            path.CopyTo(normalizedPath);

            for (int i = 0; i < normalizedPath.Length; i++)
                if (normalizedPath[i] == '\\')
                    normalizedPath[i] = '/';

            if (cache.TryGetKey(normalizedPath, out string? pathString))
            {
                // Enqueue the reload to be executed on the main thread
                MainThread.Enqueue(() => ReloadAssetPath(pathString));
            }
        }

        private readonly struct AssetInfo(string assetPath, string? metadataPath) : IExternalAssetInfo
        {
            public AssetFormat Format => AssetUtility.DetectFormat(assetPath);
            [MustDisposeResource]
            public Stream OpenAsset() => File.OpenRead(assetPath);
            [MustDisposeResource]
            public Stream? OpenMetadata() => metadataPath is null ? null : File.OpenRead(metadataPath);
        }

    }
}
