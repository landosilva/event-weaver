using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Lando.EventWeaver.Events;
using UnityEngine;

namespace Lando.EventWeaver.Editor.History
{
    public class EventHistory :
        IEventListener<OnEventRegistered>,
        IEventListener<OnEventRaised>,
        IEventListener<OnEventUnregistered>
    {
        private static EventHistory _instance;
        
        private static readonly List<EventLog> Logs = new();

        public static bool ShowRegister = true;
        public static bool ShowRaise = true;
        public static bool ShowUnregister = true;

        private static readonly string[] IgnoreNamespaces = { AssemblyName.EventWeaver, FolderName.UnityEngine };

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Initialize()
        {
            _instance = new EventHistory();
        }

        private static void Add(string message, EventLogType type)
        {
            Logs.Add(new EventLog
            {
                Message = message,
                Type = type,
                Timestamp = DateTime.Now
            });
        }

        public static void Clear() => Logs.Clear();

        public static List<EventLog> GetFilteredLogs()
        {
            return Logs.Where(log =>
                (ShowRegister && log.Type == EventLogType.Register) ||
                (ShowRaise && log.Type == EventLogType.Raise) ||
                (ShowUnregister && log.Type == EventLogType.Unregister)
            ).ToList();
        }

        public static List<EventLog> GetLogs() => new(collection: Logs);

        public void OnListenedTo(OnEventRegistered e)
        {
            if (e.Listener == this) 
                return;
            string message = $"{e.Listener.GetType().Name} registered for {e.EventType.Name}";
            Add(message, EventLogType.Register);
        }

        public void OnListenedTo(OnEventRaised e)
        {
            if (e.Event is OnEventRegistered or OnEventUnregistered) 
                return;
            
            StackTrace stackTrace = new();
            string path = "";
            for (int i = stackTrace.FrameCount - 1; i > 0; i--)
            {
                MethodBase method = stackTrace.GetFrame(i).GetMethod();

                if (IgnoreNamespaces.Any(Match))
                    continue;
                
                path += $"{method.DeclaringType?.Name}.{method.Name} > ";
                
                continue;

                bool Match(string ns) => method.DeclaringType?.Namespace?.Contains(ns) == true;
            }
            path = path.TrimEnd(' ', '>', ' ');
            string message = $"{e.Event.GetType().Name} raised by {path}";
            Add(message, EventLogType.Raise);
        }

        public void OnListenedTo(OnEventUnregistered e)
        {
            if (e.Listener == this) 
                return;
            string message = $"{e.Listener.GetType().Name} unregistered for {e.EventType.Name}";
            Add(message, EventLogType.Unregister);
        }
    }
}