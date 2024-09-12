using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;

namespace SoRR
{
    public static class Assets
    {
        private static readonly Dictionary<string, AssetManager> managers = [];
        private static ReadOnlyDictionary<string, AssetManager>? managersReadonly;

        public static ReadOnlyDictionary<string, AssetManager> AssetManagers
            => managersReadonly ??= new(managers);

        public static T Load<T>(string query)
        {
            SplitPath(query, out ReadOnlySpan<char> prefix, out ReadOnlySpan<char> path);

            if (!managers.TryGetValue(prefix.ToString(), out AssetManager? manager))
                throw new ArgumentException("Invalid asset manager prefix.", nameof(query));

            return manager.Load<T>(path.ToString());
        }

        public static void RegisterModulePrefix(string prefix, AssetManager manager)
            => managers.Add(prefix, manager);
        public static bool UnRegisterModulePrefix(string prefix, [NotNullWhen(true)] out AssetManager? manager)
            => managers.Remove(prefix, out manager);

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
