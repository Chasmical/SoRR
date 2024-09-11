using System;
using System.Collections.Generic;

namespace SoRR
{
    public sealed class AgentMetadata
    {
        public Type Type { get; }
        public string Name { get; }
        public int UID { get; }

        private static int idCounter = 1;
        private static readonly Dictionary<Type, AgentMetadata> dict = [];

        public static AgentMetadata Get<T>()
            => Get(typeof(T));
        public static AgentMetadata Get(Type type)
        {
            if (!dict.TryGetValue(type, out AgentMetadata? metadata))
                dict.Add(type, metadata = new AgentMetadata(type));
            return metadata;
        }

        private AgentMetadata(Type type)
        {
            if (!typeof(Agent).IsAssignableFrom(type))
                throw new ArgumentException($"The specified type is not an {nameof(Agent)}.", nameof(type));

            Type = type;
            Name = type.FullName ?? type.Name;
            UID = idCounter++;
        }

    }
}
