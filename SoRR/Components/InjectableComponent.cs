using UnityEngine;

namespace SoRR
{
    /// <summary>
    ///   <para>Represents the base class for components using the SoRR Dependency Injection system.</para>
    /// </summary>
    public abstract class InjectableComponent : MonoBehaviour
    {
        /// <summary/>
        public virtual void Awake()
            => Injector.Inject(this);

    }
}
