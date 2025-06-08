using UnityEngine;

namespace Lando.EventWeaver.Editor
{
    public static class WarningMessage
    {
        private const string PREFIX = "<b>[EventWeaver]</b> ";

        public const string EventRegistryNotFound = PREFIX + "EventRegistry not found.";
        public const string RegisterUnregisterNotFound = PREFIX + "Register/Unregister methods not found.";
        public const string FailedToPatchAssembly = PREFIX + "Failed to patch assembly ";
        public const string FailedToResolveBaseType = PREFIX + "Failed to resolve base type for ";
    }

    public static class InformationMessage
    {
        private const string PREFIX = "<b>[EventWeaver]</b> ";

        public const string PatchingAssembly = PREFIX + "Patching assembly: ";
    }

    public static class MethodName
    {
        public const string OnEnable = "OnEnable";
        public const string OnDisable = "OnDisable";

        public const string Register = "Register";
        public const string Unregister = "Unregister";

        public const string Constructor = ".ctor";
        public const string Finalize = "Finalize";
    }

    public static class ClassName
    {
        public const string EventRegistry = "EventRegistry";
        public const string EventListener = "IEventListener`1";

        public const string MonoBehaviour = "MonoBehaviour";
        public const string SystemObject = "System.Object";
    }

    public static class AssemblyName
    {
        public const string EventWeaver = "Lando.EventWeaver";
    }

    public static class FolderName
    {
        public const string Runtime = "Runtime";
        public const string Managed = "Managed";
        public const string UnityEngine = "UnityEngine";
        public const string ScriptAssemblies = "../Library/ScriptAssemblies";
    }
}
