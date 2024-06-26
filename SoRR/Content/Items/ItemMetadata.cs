using System;
using System.Collections.Generic;

namespace SoRR
{
    public sealed class ItemMetadata
    {
        public Type Type { get; }
        public int UID { get; }

        private static int idCounter = 1;
        private static readonly Dictionary<Type, ItemMetadata> dict = [];

        public static ItemMetadata Get<T>()
            => Get(typeof(T));
        public static ItemMetadata Get(Type type)
        {
            if (!dict.TryGetValue(type, out ItemMetadata? metadata))
                dict.Add(type, metadata = new ItemMetadata(type));
            return metadata;
        }

        private ItemMetadata(Type type)
        {
            if (!typeof(Item).IsAssignableFrom(type))
                throw new ArgumentException("The specified type is not an Item.", nameof(type));

            Type = type;
            UID = idCounter++;
        }

    }
}
