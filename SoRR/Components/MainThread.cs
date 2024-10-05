using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using Chasm.Utilities;
using JetBrains.Annotations;
using UnityEngine;

namespace SoRR
{
    public static class MainThread
    {
        private static readonly ConcurrentQueue<Action> queue = [];
        private static MainThreadRunner? activeRunner;

        public static void Enqueue(Action action)
        {
            // TODO: log a warning, if activeRunner is null
            queue.Enqueue(action);
        }

        [UsedImplicitly]
        public sealed class MainThreadRunner : MonoBehaviour
        {
            private readonly List<Action> actionsList = [];
            private readonly List<Exception> exceptionsList = [];

            public void FixedUpdate()
            {
                List<Action> actions = actionsList;
                List<Exception> exceptions = exceptionsList;

                while (queue.TryDequeue(out Action? action))
                    actions.Add(action);

                for (int i = 0; i < actions.Count; i++)
                {
                    if (Util.Catch(actions[i]) is { } exception)
                        exceptions.Add(exception);
                }
                actions.Clear();

                if (exceptions.Count > 0)
                {
                    Exception exception = exceptions.Count == 1 ? exceptions[0] : new AggregateException(exceptions);
                    exceptions.Clear();
                    throw exception;
                }
            }

            // There can only be one
            public void OnEnable()
                => enabled = Interlocked.CompareExchange(ref activeRunner, this, null) is null;
            public void OnDisable()
                => Interlocked.CompareExchange(ref activeRunner, null, this);

        }
    }
}
