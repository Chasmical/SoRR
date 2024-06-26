using UnityEngine;

namespace SoRR
{
    public class Entity : Injectable
    {
        // ReSharper disable once IdentifierTypo
        [Inject] public readonly Rigidbody2D rigidbody = null!;

        protected override void Awake()
        {
            base.Awake();
        }
    }
}
