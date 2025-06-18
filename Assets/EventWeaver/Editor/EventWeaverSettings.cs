using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Lando.EventWeaver.Editor
{
    [CreateAssetMenu(menuName = "Event Weaver/Settings", fileName = "EventWeaverSettings")]
    public class EventWeaverSettings : ScriptableObject
    {
        [SerializeField] private EventWeaverLogSettings _logSettings;
        public static EventWeaverLogSettings Log => _instance._logSettings;
        
        private static EventWeaverSettings _instance;

#if UNITY_EDITOR
        [InitializeOnLoadMethod]
        private static void EnsureSettingsExist() => LoadOrCreateSettings();
        
        private static void LoadOrCreateSettings()
        {
            const string scriptName = nameof(EventWeaverSettings);
            _instance ??= Resources.Load<EventWeaverSettings>(scriptName) ?? CreateSettings();
        }
        
        private static EventWeaverSettings CreateSettings()
        {
            const string scriptName = nameof(EventWeaverSettings);
            string scriptableObjectName = $"{scriptName}.asset";
            
            string filter = $"t:MonoScript {scriptName}";
            string scriptPath = AssetDatabase.FindAssets(filter).Select(AssetDatabase.GUIDToAssetPath).ElementAt(0);
            string directory = Path.GetDirectoryName(scriptPath) ?? string.Empty;
            string path = Path.Combine(directory, FolderName.Resources);
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            string fullPath = Path.Combine(path, scriptableObjectName);
            EventWeaverSettings settings = CreateInstance<EventWeaverSettings>();
            AssetDatabase.CreateAsset(settings, fullPath);
            AssetDatabase.SaveAssets();

            EventWeaverDebug.Log($"{InformationMessage.SettingsCreatedAt}{path}");
            return settings;
        }
#endif
    }
}