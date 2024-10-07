using System;
using System.Threading;
using JetBrains.Annotations;

namespace SoRR
{
    /// <summary>
    ///   <para>Provides a set of operations for variables shared by multiple threads.</para>
    /// </summary>
    public static class Locked
    {
        /// <summary>
        ///   <para>Gets a value from the specified <paramref name="field"/>, if it's not null, or uses the specified <paramref name="initializeFunc"/> to initialize its value, while locked onto the specified <paramref name="lockObj"/>.</para>
        /// </summary>
        /// <typeparam name="T">The type of the value in the field.</typeparam>
        /// <param name="field">The location of the value to get.</param>
        /// <param name="lockObj">The object to lock onto, while initializing the value.</param>
        /// <param name="initializeFunc">The function to initialize the field's value with.</param>
        /// <returns>The stored or initialized value.</returns>
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

        /// <summary>
        ///   <para>Gets a value from the specified <paramref name="field"/>, if it's not null, or uses the specified <paramref name="initializeFunc"/> to initialize its value. Allows concurrent executions of the <paramref name="initializeFunc"/>.</para>
        /// </summary>
        /// <typeparam name="T">The type of the value in the field.</typeparam>
        /// <param name="field">The location of the value to get.</param>
        /// <param name="initializeFunc">The function to initialize the field's value with.</param>
        /// <returns>The stored or initialized value.</returns>
        public static T GetConcurrent<T>(ref T? field, [InstantHandle] Func<T> initializeFunc) where T : class
        {
            T? current = field;
            if (current is null)
            {
                current = initializeFunc();
                Interlocked.CompareExchange(ref field, current, null);
            }
            return current;
        }

    }
}
