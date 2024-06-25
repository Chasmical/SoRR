using UnityEngine;

namespace SoRR
{
    public class Bootstrap : MonoBehaviour
    {
        public void Awake()
        {
            Entrypoint.Run();
        }
    }
}
