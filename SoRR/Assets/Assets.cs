using System;
using System.Diagnostics.CodeAnalysis;
using Chasm.Utilities;

namespace SoRR
{
    public static class Assets
    {
        // TODO: when asset managers can use spans to index assets, replace this with a string-keyed dictionary too
        // TODO: then, ArrayEx class can be removed, since it's not used anywhere else
        private static ManagerEntry[] _entries = [];

        private readonly struct ManagerEntry(string prefix, AssetManager manager)
        {
            public string Prefix { get; } = prefix;
            public AssetManager Manager { get; } = manager;
        }

        public static bool TryLoad<T>(string query, [NotNullWhen(true)] out T? asset)
        {
            AssetManager? manager = FindManager(query, out ReadOnlySpan<char> path);
            return manager is null ? Util.Fail(out asset) : manager.TryLoad(path.ToString(), out asset);
        }
        public static T Load<T>(string query)
        {
            AssetManager? manager = FindManager(query, out ReadOnlySpan<char> path);
            if (manager is null) throw new ArgumentException("Invalid asset manager prefix.", nameof(query));
            return manager.Load<T>(path.ToString());
        }
        public static T? LoadOrDefault<T>(string query)
        {
            AssetManager? manager = FindManager(query, out ReadOnlySpan<char> path);
            return manager is null ? default : manager.LoadOrDefault<T>(path.ToString());
        }

        public static void RegisterAssetManager(string prefix, AssetManager manager)
        {
            if (manager.registeredPrefix is not null) throw new ArgumentException();
            manager.registeredPrefix = prefix;
            ArrayEx.AddItem(ref _entries, new ManagerEntry(prefix, manager));
        }
        public static bool UnRegisterAssetManager(string prefix, [NotNullWhen(true)] out AssetManager? manager)
        {
            int index = Array.FindIndex(_entries, e => e.Prefix == prefix);
            if (index == -1)
            {
                manager = null;
                return false;
            }
            manager = _entries[index].Manager;
            ArrayEx.RemoveRange(ref _entries, index, 1);
            manager.registeredPrefix = null;
            return true;
        }

        private static AssetManager? FindManager(string query, out ReadOnlySpan<char> path)
        {
            SplitPath(query, out ReadOnlySpan<char> prefix, out path);

            ManagerEntry[] entries = _entries;
            for (int i = 0; i < entries.Length; i++)
            {
                ManagerEntry entry = entries[i];
                if (prefix.SequenceEqual(entry.Prefix)) return entry.Manager;
            }
            return null;
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
