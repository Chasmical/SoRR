using UnityEngine;

namespace SoRR
{
    public abstract class Service : MonoBehaviour
    {
        protected virtual void Awake()
            => Injector.Inject(this);

    }
}
