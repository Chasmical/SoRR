using System;
using System.IO;
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

        /// <summary>
        ///   <para>Initializes a new instance of the <see cref="FileSystemAssetManager"/> class with the specified <paramref name="directoryPath"/>.</para>
        /// </summary>
        /// <param name="directoryPath">A path to the directory to load assets from.</param>
        /// <exception cref="ArgumentNullException"><paramref name="directoryPath"/> is <see langword="null"/>.</exception>
        public FileSystemAssetManager(string directoryPath)
        {
            if (directoryPath is null) throw new ArgumentNullException(nameof(directoryPath));
            DirectoryPath = Path.GetFullPath(directoryPath);

            // TODO: reimplement the directory watcher
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
            return new AssetInfo(mainPath, metadataPath);
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
