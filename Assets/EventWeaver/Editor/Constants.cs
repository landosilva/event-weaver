namespace Lando.EventWeaver.Editor
{
    public static class WarningMessage
    {
        public const string EventRegistryNotFound = "EventRegistry not found.";
        public const string RegisterUnregisterNotFound = "Register/Unregister methods not found.";
        public const string FailedToPatchAssembly = "Failed to patch assembly ";
    }

    public static class InformationMessage
    {
        public const string PatchingAssembly = "Patching assembly: ";
        public const string ManuallyPatching = "Manually patching: ";
        public const string SettingsCreatedAt = "Settings created at: ";
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
        public const string Resources = "../Resources";
    }

    public static class SearchPattern
    {
        public const string DLL = "*.dll";
    }
    
    public static class Variable
    {
        public const string InitialPatchDone = "Lando.EventWeaver.InitialPatchDone";
    }

    public static class WindowName
    {
        public const string EventViewer = "Event Viewer";
    }

    public static class IconName
    {
        public const string Prefab = "d_Prefab Icon";
        public const string Unity = "d_UnityLogo";
        public const string Zoom = "d_ViewToolZoom";
        public const string Info = "d_console.infoicon";
    }
}
