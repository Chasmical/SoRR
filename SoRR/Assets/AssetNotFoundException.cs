using System;

namespace SoRR
{
    public sealed class AssetNotFoundException(AssetManager manager, string path) : Exception
    {
        public AssetManager Manager { get; } = manager;
        public string Path { get; } = path;

        public override string Message => $"Asset \"{Path}\" could not be found in {Manager}.";

    }
}
