using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace SoRR.Networking
{
    public sealed class Network
    {
        public static Network Current { get; } = new Network();

        public NetworkUser LocalUser { get; }

        private readonly List<NetworkUser> users;
        public ReadOnlyCollection<NetworkUser> Users { get; }

        public Network()
        {
            LocalUser = new NetworkUser(this, 0);
            users = [LocalUser];
            Users = users.AsReadOnly();
        }

        private ulong idCounter;
        public NetworkId NextId() => (NetworkId)(++idCounter);

    }
    public abstract class NetworkObject
    {
        public Network Network { get; }
        public NetworkId Id { get; }
        public NetworkUser Owner { get; private set; }

        protected NetworkObject(NetworkUser owner)
        {
            Network = owner.Network;
            Id = Network.NextId();
            Owner = owner;
        }

    }
    public abstract class NetworkBehaviour : MonoBehaviour
    {
        public NetworkObject NetworkObject { get; } = new NetworkBehaviourObject();

        private sealed class NetworkBehaviourObject : NetworkObject
        {
            public NetworkBehaviourObject() : base(NetworkContext.InferOwner()) { }
        }
    }
    public sealed class NetworkVariable<T> : NetworkObject
    {
        private T _value;
        public T Value
        {
            get => _value;
            set
            {
                _value = value;
                SendChange(value);
            }
        }

        public NetworkVariable([ConstantExpected] T defaultValue) : base(NetworkContext.InferOwner())
        {
            _value = defaultValue;
        }

        private void SendChange(T newValue)
        {

        }

    }
    public static class NetworkContext
    {
        private static NetworkUser? inferredOwner;
        public static NetworkUser InferOwner()
            => inferredOwner ?? throw new InvalidOperationException();

        public static void StartInferringOwner(NetworkUser owner)
            => inferredOwner = owner;
        public static void StopInferringOwner()
            => inferredOwner = null;

    }
}
