using System.Collections.Generic;

namespace Lando.EventWeaver
{
    public static class EventRegistry
    {
        private static readonly Dictionary<Type, List<object>> Listeners = new();

        public static void Register<T>(IEventListener<T> listener) where T : IEvent
        {
            Type eventType = typeof(T);

            if (!Listeners.ContainsKey(eventType)) 
                Listeners[eventType] = new List<object>();

            if (Listeners[eventType].Contains(listener)) 
                return;
            
            Listeners[eventType].Add(listener);
            new OnEventRegistered(listener, eventType).Raise();
        }

        public static void Unregister<T>(IEventListener<T> listener) where T : IEvent
        {
            Type eventType = typeof(T);

            if (!Listeners.TryGetValue(eventType, out List<object> listeners)) 
                return;
            
            listeners.Remove(listener);
            new OnEventUnregistered(listener, eventType).Raise();

            if (Listeners[eventType].Count == 0) 
                Listeners.Remove(eventType);
        }

        public static void Raise<T>(T e) where T : IEvent
        {
            Type eventType = typeof(T);

            if (!Listeners.TryGetValue(eventType, out List<object> listeners)) 
                return;

            for (int i = 0; i < listeners.Count; i++)
            {
                object listener = listeners[i];
                ((IEventListener<T>)listener).OnListenedTo(e);
            }

            if (e is not OnEventRaised) 
                new OnEventRaised(e).Raise();
        }
    }
}