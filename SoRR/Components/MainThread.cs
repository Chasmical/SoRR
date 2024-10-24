﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Chasm.Utilities;
using JetBrains.Annotations;
using UnityEngine;

namespace SoRR
{
    /// <summary>
    ///   <para>Provides a set of static methods for running code on the main Unity thread.</para>
    /// </summary>
    public static class MainThread
    {
        private static readonly ConcurrentQueue<Action> queue = [];
        private static MainThreadRunner? activeRunner;

        /// <summary>
        ///   <para>Determines whether the current thread is the main Unity thread.</para>
        /// </summary>
        /// <returns><see langword="true"/>, if the current thread is the main Unity thread; otherwise, <see langword="false"/>.</returns>
        [Pure] public static bool IsCurrent()
            => activeRunner is { } runner && runner.ThreadId == Thread.CurrentThread.ManagedThreadId;

        /// <summary>
        ///   <para>Enqueues the specified <paramref name="action"/> to run on the main Unity thread.</para>
        /// </summary>
        /// <param name="action">The action to run on the main Unity thread.</param>
        /// <exception cref="ArgumentNullException"><paramref name="action"/> is <see langword="null"/>.</exception>
        public static void Enqueue(Action action)
        {
            Guard.ThrowIfNull(action);
            // TODO: log a warning if activeRunner is null or disabled
            queue.Enqueue(action);
        }

        /// <summary>
        ///   <para>Enqueues the specified <paramref name="action"/> to run on the main Unity thread and returns a <see cref="Task"/> that represents it.</para>
        /// </summary>
        /// <param name="action">The action to run on the main Unity thread.</param>
        /// <exception cref="ArgumentNullException"><paramref name="action"/> is <see langword="null"/>.</exception>
        /// <returns>A task representing the specified <paramref name="action"/> queued to execute on the main Unity thread.</returns>
        [MustUseReturnValue] public static Task RunAsync(Action action)
        {
            Guard.ThrowIfNull(action);
            return RunAsync<object?>(() => { action(); return null; });
        }
        /// <summary>
        ///   <para>Enqueues the specified <paramref name="function"/> to run on the main Unity thread and returns a <see cref="Task{T}"/> that represents it.</para>
        /// </summary>
        /// <param name="function">The function to run on the main Unity thread.</param>
        /// <exception cref="ArgumentNullException"><paramref name="function"/> is <see langword="null"/>.</exception>
        /// <returns>A task representing the specified <paramref name="function"/> queued to execute on the main Unity thread.</returns>
        [MustUseReturnValue] public static Task<T> RunAsync<T>(Func<T> function)
        {
            Guard.ThrowIfNull(function);
            TaskCompletionSource<T> tcs = new();

            Enqueue(() =>
            {
                try { tcs.SetResult(function()); }
                catch (Exception ex) { tcs.SetException(ex); }
            });

            return tcs.Task;
        }

        /// <summary>
        ///   <para>Represents a component responsible for querying and executing the queue of tasks scheduled to run on the main Unity thread.</para>
        /// </summary>
        public sealed class MainThreadRunner : MonoBehaviour
        {
            /// <summary>
            ///   <para>Gets the unique identifier of the thread that the runner was initialized on.</para>
            /// </summary>
            public int ThreadId { get; private set; }

            // Store lists to avoid allocating them every time
            private readonly List<Action> actionsList = [];
            private readonly List<Exception> exceptionsList = [];

            /// <summary/>
            public void Awake()
                => ThreadId = Thread.CurrentThread.ManagedThreadId;
            /// <summary/>
            public void OnEnable()
                => enabled = Interlocked.CompareExchange(ref activeRunner, this, null) is null;
            /// <summary/>
            public void OnDisable()
                => Interlocked.CompareExchange(ref activeRunner, null, this);

            /// <summary/>
            public void FixedUpdate()
            {
                Debug.Assert(IsCurrent());

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

        }
    }
}
