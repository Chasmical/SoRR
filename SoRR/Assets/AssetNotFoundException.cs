using System;

namespace SoRR
{
    /// <summary>
    ///   <para>The exception that is thrown when an asset at the specified path is not found.</para>
    /// </summary>
    public sealed class AssetNotFoundException : Exception
    {
        /// <summary>
        ///   <para>Gets the asset manager that the specified asset could not be found in.</para>
        /// </summary>
        public AssetManager Manager { get; }
        /// <summary>
        ///   <para>Gets a relative path to the asset that could not be found.</para>
        /// </summary>
        public string Path { get; }

        /// <summary>
        ///   <para>Initializes a new instance of the <see cref="AssetNotFoundException"/> class with the specified asset <paramref name="manager"/> and <paramref name="path"/>.</para>
        /// </summary>
        /// <param name="manager">The asset manager that the specified asset could not be found in.</param>
        /// <param name="path">A relative path to the asset that could not be found.</param>
        public AssetNotFoundException(AssetManager manager, string path)
        {
            Guard.ThrowIfNull(manager);
            Guard.ThrowIfNull(path);
            Manager = manager;
            Path = path;
        }

        /// <inheritdoc/>
        public override string Message => $"Asset \"{Path}\" could not be found in {Manager}.";

    }
}
