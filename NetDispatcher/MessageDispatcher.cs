using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace NetDispatcher
{
    public delegate void MessageHandler<U>(U message);

    public sealed class MessageDispatcher<T, U>
    {
        static ConcurrentDictionary<T, System.Delegate> handlers =
            new ConcurrentDictionary<T, System.Delegate>();

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
                            ((MessageHandler<U>)eventHandler)(message);
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

        public static void RegisterHandler(T type, MessageHandler<U> handler)
        {
            Delegate d;
            handlers.TryGetValue(type, out d);

            if (d == null)
            {
                handlers[type] = null;
            }

            handlers[type] = (MessageHandler<U>)handlers[type] + handler;
        }

        public static void DeregisterHandler(T type, MessageHandler<U> handler)
        {
            Delegate d;
            handlers.TryGetValue(type, out d);

            if (d != null)
            {
                handlers[type] = (MessageHandler<U>)handlers[type] - handler;
            }
        }
    }
}
