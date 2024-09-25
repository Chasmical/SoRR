using System;
using System.Diagnostics.CodeAnalysis;
using Chasm.Utilities;

namespace SoRR
{
    public static class Assets
    {
        private static readonly StringKeyedDictionary<AssetManager> managers = new();

        public static bool TryLoad<T>(string query, [NotNullWhen(true)] out T? asset)
            => TryLoad(query.AsSpan(), out asset);
        public static T Load<T>(string query)
            => Load<T>(query.AsSpan());
        public static T? LoadOrDefault<T>(string query)
            => LoadOrDefault<T>(query.AsSpan());

        public static bool TryLoad<T>(ReadOnlySpan<char> query, [NotNullWhen(true)] out T? asset)
        {
            AssetManager? manager = FindManager(query, out ReadOnlySpan<char> path);
            return manager is null ? Util.Fail(out asset) : manager.TryLoad(path, out asset);
        }
        public static T Load<T>(ReadOnlySpan<char> query)
        {
            AssetManager? manager = FindManager(query, out ReadOnlySpan<char> path);
            if (manager is null) throw new ArgumentException("Invalid asset manager prefix.", nameof(query));
            return manager.Load<T>(path);
        }
        public static T? LoadOrDefault<T>(ReadOnlySpan<char> query)
        {
            AssetManager? manager = FindManager(query, out ReadOnlySpan<char> path);
            return manager is null ? default : manager.LoadOrDefault<T>(path);
        }

        public static void RegisterAssetManager(string prefix, AssetManager manager)
        {
            if (manager.registeredPrefix is not null) throw new ArgumentException();
            manager.registeredPrefix = prefix;
            managers.Add(prefix, manager);
        }
        public static bool UnRegisterAssetManager(string prefix, [NotNullWhen(true)] out AssetManager? manager)
        {
            bool res = managers.Remove(prefix, out manager);
            if (res) manager!.registeredPrefix = null;
            return res;
        }

        private static AssetManager? FindManager(ReadOnlySpan<char> query, out ReadOnlySpan<char> path)
        {
            SplitPath(query, out ReadOnlySpan<char> prefix, out path);
            return managers.TryGetValue(prefix, out AssetManager? manager) ? manager : null;
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
