using System;
using JetBrains.Annotations;

namespace SoRR
{
    public static class Locked
    {
        public static T Get<T>(ref T? field, object lockObj, [InstantHandle] Func<T> initializeFunc) where T : class
        {
            T? current = field;
            if (current is null)
            {
                lock (lockObj)
                {
                    current = field;
                    if (current is null) field = current = initializeFunc();
                }
            }
            return current;
        }

        public static T GetConcurrent<T>(ref T? field, object lockObj, [InstantHandle] Func<T> initializeFunc) where T : class
        {
            T? current = field;
            if (current is null)
            {
                current = initializeFunc();
                lock (lockObj) current = field ??= current;
            }
            return current;
        }

    }
}
