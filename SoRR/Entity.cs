using JetBrains.Annotations;
using Unity.Netcode;
using Unity.Netcode.Components;

namespace SoRR
{
    public class Entity : NetworkBehaviour
    {
        [Inject, UsedImplicitly] private readonly NetworkObject networkObject = null!;
        // ReSharper disable once IdentifierTypo
        [Inject] public readonly NetworkRigidbody2D rigidbody = null!;
        [Inject] public new readonly NetworkTransform transform = null!;

        protected virtual void Awake()
        {
            Injector.Inject(this);
            if (!IsSpawned) NetworkObject.Spawn();
        }

    }
}
