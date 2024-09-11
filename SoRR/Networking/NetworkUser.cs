using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace SoRR.Networking
{
    public sealed class NetworkUser
    {
        public Network Network { get; }
        public ulong Id { get; }

        internal readonly List<INetworkObject> ownedObjects;
        public ReadOnlyCollection<INetworkObject> OwnedObjects { get; }

        public NetworkUser(Network network, ulong id)
        {
            Network = network;
            Id = id;
            ownedObjects = [];
            OwnedObjects = ownedObjects.AsReadOnly();
        }

    }
}
