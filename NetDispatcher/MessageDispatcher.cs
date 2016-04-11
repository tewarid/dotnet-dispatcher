using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace NetDispatcher
{
    public sealed class MessageDispatcher<T, U>
    {
        static ConcurrentDictionary<T, System.Delegate> handlers =
            new ConcurrentDictionary<T, System.Delegate>();

        /// <summary>
        /// Dispatches a message to all handlers. Blocks until all handlers
        /// have been called. Handlers are called asynchronously.
        /// </summary>
        /// <param name="type">Type object.</param>
        /// <param name="message">Message object to dispatch.</param>
        public static void Dispatch(T type, U message)
        {
            System.Delegate eventHandlers;
            handlers.TryGetValue(type, out eventHandlers);
            if (eventHandlers != null)
            {
                try
                {
                    List<Task> taskList = new List<Task>(); 
                    foreach (Delegate eventHandler in eventHandlers.GetInvocationList())
                    {
                        taskList.Add(Task.Run(delegate
                        {
                            ((Action<U>)eventHandler).Invoke(message);
                        }));
                    }
                    Task.WaitAll(taskList.ToArray());
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    Debug.WriteLine(ex.StackTrace);
                }
            }
        }

        /// <summary>
        /// Register a handler for a message type.
        /// </summary>
        /// <param name="type">Type object.</param>
        /// <param name="handler">Message handler delegate.</param>
        public static void RegisterHandler(T type, Action<U> handler)
        {
            Delegate d;
            handlers.TryGetValue(type, out d);

            if (d == null)
            {
                handlers[type] = null;
            }

            handlers[type] = (Action<U>)handlers[type] + handler;
        }

        /// <summary>
        /// Removes handler for a message type.
        /// </summary>
        /// <param name="type">Type object.</param>
        /// <param name="handler">Message handler delegate.</param>
        public static void DeregisterHandler(T type, Action<U> handler)
        {
            Delegate d;
            handlers.TryGetValue(type, out d);

            if (d != null)
            {
                handlers[type] = (Action<U>)handlers[type] - handler;
            }
        }

        /// <summary>
        /// Clear all message handlers of a particular type.
        /// </summary>
        /// <param name="type">Type object.</param>
        public static void Clear(T type)
        {
            Delegate d;
            handlers.TryGetValue(type, out d);

            if (d != null)
            {
                handlers[type] = null;
            }
        }
    }
}
