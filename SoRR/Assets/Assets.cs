using System;
using System.Diagnostics.CodeAnalysis;

namespace SoRR
{
    public static class Assets
    {
        private static readonly StringKeyedDictionary<AssetManager> managers = new();

        public static void RegisterAssetManager(AssetManager manager, string prefix)
        {
            managers.Add(prefix, manager);
            manager.registeredPrefix = prefix;
        }
        public static bool UnRegisterAssetManager(AssetManager manager)
        {
            if (manager.registeredPrefix is not null && managers.Remove(manager.registeredPrefix))
            {
                manager.registeredPrefix = null;
                return true;
            }
            return false;
        }

        public static AssetHandle? GetHandle(string fullPath)
            => GetHandle(fullPath.AsSpan());
        public static AssetHandle? GetHandle(ReadOnlySpan<char> fullPath)
        {
            SplitPath(fullPath, out var prefix, out var relativePath);
            return managers.TryGetValue(prefix, out AssetManager? manager)
                ? manager.GetHandle(relativePath)
                : null;
        }

        public static T? Load<T>(string fullPath)
            => Load<T>(fullPath.AsSpan());
        public static T? Load<T>(ReadOnlySpan<char> fullPath)
        {
            SplitPath(fullPath, out var prefix, out var relativePath);
            return managers.TryGetValue(prefix, out AssetManager? manager)
                ? manager.Load<T>(relativePath)
                : throw new ArgumentException("Could not find specified asset manager prefix.", nameof(fullPath));
        }

        public static bool TryLoad<T>(string fullPath, [NotNullWhen(true)] out T? asset)
            => TryLoad(fullPath.AsSpan(), out asset);
        public static bool TryLoad<T>(ReadOnlySpan<char> fullPath, [NotNullWhen(true)] out T? asset)
        {
            SplitPath(fullPath, out var prefix, out var relativePath);
            if (managers.TryGetValue(prefix, out AssetManager? manager))
                return manager.TryLoad(relativePath, out asset);
            asset = default;
            return false;
        }

        private static void SplitPath(ReadOnlySpan<char> query, out ReadOnlySpan<char> prefix, out ReadOnlySpan<char> path)
        {
            int separatorIndex = query.IndexOf(":/");
            if (separatorIndex == -1)
            {
                if (query.Length > 0 && query[0] == '/')
                    query = query[1..];
                // [ '/' ] <path>
                prefix = default;
                path = query;
                return;
            }
            // <prefix> ':/' <path>
            prefix = query[..separatorIndex];
            path = query[(separatorIndex + 2)..];
        }

    }
}
