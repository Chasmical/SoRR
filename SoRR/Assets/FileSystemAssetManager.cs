using System;
using System.IO;
using JetBrains.Annotations;

namespace SoRR
{
    public sealed class FileSystemAssetManager : ExternalAssetManagerBase
    {
        public string DirectoryPath { get; }
        public override string DisplayName => $"\"{DirectoryPath}{Path.DirectorySeparatorChar}\"";

        public FileSystemAssetManager(string directoryPath)
        {
            if (directoryPath is null) throw new ArgumentNullException(nameof(directoryPath));
            DirectoryPath = Path.GetFullPath(directoryPath);

            // TODO: reimplement the directory watcher
        }

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
            return new AssetInfo(mainPath, metadataPath);
        }

        public readonly struct AssetInfo(string assetPath, string? metadataPath) : IExternalAssetInfo
        {
            public AssetFormat Format => AssetUtility.DetectFormat(assetPath);
            [MustDisposeResource]
            public Stream OpenAsset() => File.OpenRead(assetPath);
            [MustDisposeResource]
            public Stream? OpenMetadata() => metadataPath is null ? null : File.OpenRead(metadataPath);
        }

    }
}
