using UnityEngine;

namespace Lando.EventWeaver.Editor
{
    public static class EventWeaverDebug
    {
        private const string PREFIX = "<b>[EventWeaver]</b> ";
        
        public static void Log(string message)
        {
            if (!EventWeaverSettings.Log.Message)
                return;
            
            Debug.Log(PREFIX + message);
        }

        public static void LogWarning(string message)
        {
            if (!EventWeaverSettings.Log.Warning)
                return;
            
            Debug.LogWarning(PREFIX + message);
        }

        public static void LogError(string message)
        {
            if (!EventWeaverSettings.Log.Error)
                return;

            Debug.LogError(PREFIX + message);
        }
    }
}