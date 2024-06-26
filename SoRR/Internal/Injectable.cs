using UnityEngine;

namespace SoRR
{
    public abstract class Injectable : MonoBehaviour
    {
        protected virtual void Awake()
            => Injector.Inject(this);

    }
}
