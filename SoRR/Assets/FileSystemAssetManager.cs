using System;
using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;

namespace SoRR
{
    public sealed class FileSystemAssetManager : ExternalAssetManagerBase, IDisposable
    {
        public string DirectoryPath { get; }
        public override string DisplayName => DirectoryPath;

        private FileSystemWatcher? watcher;

        public FileSystemAssetManager(string directoryPath)
        {
            if (directoryPath is null) throw new ArgumentNullException(nameof(directoryPath));
            DirectoryPath = Path.GetFullPath(directoryPath);

            watcher = new FileSystemWatcher(directoryPath);
            watcher.Changed += HandleChangedFile;
            watcher.Created += HandleChangedFile;
            watcher.Deleted += HandleChangedFile;
            watcher.Renamed += HandleRenamedFile;
            watcher.EnableRaisingEvents = true;
        }

        public void Dispose()
        {
            if (watcher is null) return;
            watcher!.EnableRaisingEvents = false;
            watcher.Dispose();
            watcher = null;
        }

        private void HandleChangedFile(object _, FileSystemEventArgs e)
        {
            RefreshPath(e.FullPath);
        }
        private void HandleRenamedFile(object _, RenamedEventArgs e)
        {
            RefreshPath(e.OldFullPath);
            RefreshPath(e.FullPath);
        }
        private void RefreshPath(string fullPath)
        {
            string pathWithoutExtension = Path.GetFileNameWithoutExtension(fullPath);
            string assetPath = Path.GetRelativePath(DirectoryPath, pathWithoutExtension).Replace('\\', '/');
            RefreshAsset(assetPath);
        }

        protected override IAssetLoadInfo? GetAssetInfo(string assetPath)
        {
            List<string> assetPaths = [];
            string? metadataPath = null;

            foreach (string filePath in FileUtility.SearchFiles(DirectoryPath, assetPath))
            {
                ReadOnlySpan<char> extension = Path.GetExtension(filePath.AsSpan());

                if (extension is ".meta")
                    metadataPath = filePath;
                else
                    assetPaths.Add(filePath);
            }

            if (assetPaths.Count == 0) return null;
            // TODO: log a warning, if there's more than one asset path

            return new AssetInfo(assetPaths[0], metadataPath);
        }

        public readonly record struct AssetInfo(string AssetPath, string? MetadataPath) : IAssetLoadInfo
        {
            public AssetFormat Format => AssetUtility.DetectFormat(AssetPath);
            [MustDisposeResource]
            public Stream OpenAsset() => File.OpenRead(AssetPath);
            [MustDisposeResource]
            public Stream? OpenMetadata() => MetadataPath is null ? null : File.OpenRead(MetadataPath);
        }

    }
}
