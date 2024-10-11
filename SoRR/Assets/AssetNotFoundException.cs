using System;

namespace SoRR
{
    /// <summary>
    ///   <para>The exception that is thrown when an asset at the specified path is not found.</para>
    /// </summary>
    /// <param name="manager">The asset manager that the specified asset could not be found in.</param>
    /// <param name="path">A relative path to the asset that could not be found.</param>
    public sealed class AssetNotFoundException(AssetManager manager, string path) : Exception
    {
        /// <summary>
        ///   <para>Gets the asset manager that the specified asset could not be found in.</para>
        /// </summary>
        public AssetManager Manager { get; } = manager;
        /// <summary>
        ///   <para>Gets a relative path to the asset that could not be found.</para>
        /// </summary>
        public string Path { get; } = path;

        /// <inheritdoc/>
        public override string Message => $"Asset \"{Path}\" could not be found in {Manager}.";

    }
}
