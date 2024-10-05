using System;
using UnityEngine;

namespace SoRR
{
    public static class Entrypoint
    {
        public static GameObject GlobalsObject { get; private set; } = null!;

        public static void Run()
        {
            GlobalsObject = new GameObject("Globals") { isStatic = true };

            GlobalsObject.GetOrAddComponent<MainThread.MainThreadRunner>();

        }

    }
}
