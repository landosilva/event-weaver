using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Lando.EventWeaver.Editor.Windows
{
    public class EventViewerWindow : EditorWindow
    {
        private Vector2 _scrollPosition;
        private readonly Dictionary<string, bool> _foldoutStates = new();

        [MenuItem("Tools/Event Weaver/Event Viewer")]
        public static void ShowWindow()
        {
            EventViewerWindow window = GetWindow<EventViewerWindow>(utility: false);
            window.minSize = new Vector2(300, 200);

            Texture windowIcon = EditorGUIUtility.IconContent(IconName.Zoom).image;
            window.titleContent = new GUIContent(text: WindowName.EventViewer, windowIcon);
        }

        private void OnGUI()
        {
            if (!EditorApplication.isPlaying)
            {
                EditorGUILayout.HelpBox(message: "Event Viewer only works during Play mode.", MessageType.Warning);
                return;
            }

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            Type registryType = typeof(EventRegistry);
            const BindingFlags bindingAttr = BindingFlags.NonPublic | BindingFlags.Static;
            FieldInfo listenersField = registryType.GetField(name: "Listeners", bindingAttr);

            if (listenersField?.GetValue(null) is not IDictionary<Type, List<object>> listenersDictionary)
            {
                EditorGUILayout.HelpBox(message: "Could not find EventRegistry.Listeners.", MessageType.Error);
            }
            else
            {
                Dictionary<string, List<(Type eventType, object listener)>> grouping = new();
                foreach ((Type eventType, List<object> list) in listenersDictionary)
                {
                    foreach (object listener in list)
                    {
                        Type type = listener.GetType();
                        string baseName = typeof(MonoBehaviour).IsAssignableFrom(type)
                            ? nameof(MonoBehaviour)
                            : typeof(EditorWindow).IsAssignableFrom(type)
                                ? nameof(EditorWindow)
                                : typeof(ScriptableObject).IsAssignableFrom(type)
                                    ? nameof(ScriptableObject)
                                    : type.BaseType?.Name ?? "<No Base>";

                        if (!grouping.ContainsKey(baseName)) 
                            grouping[baseName] = new List<(Type, object)>();
                        grouping[baseName].Add((eventType, listener));
                    }
                }

                foreach ((string baseName, List<(Type eventType, object listener)> items) in grouping.OrderBy(p => p.Key))
                {
                    Color color20 = new(r: 0.2f, g: 0.2f, b: 0.2f);
                    Color color15 = new(r: 0.15f, g: 0.15f, b: 0.15f);
                    bool baseExpanded = _foldoutStates.TryGetValue(baseName, out bool baseState) && baseState;
                    DrawFoldout(baseIndent: 0, ref baseExpanded, label: baseName, IconName.Prefab, color20);
                    _foldoutStates[baseName] = baseExpanded;
                    if (!baseExpanded) 
                        continue;

                    foreach (IGrouping<Type, (Type eventType, object listener)> eventGroup in items.GroupBy(x => x.eventType))
                    {
                        string eventName = eventGroup.Key.Name;
                        bool eventExpanded = _foldoutStates.TryGetValue(eventName, out bool eventState) && eventState;
                        DrawFoldout(baseIndent:1, ref eventExpanded, label: eventName, IconName.Unity, color15);
                        _foldoutStates[eventName] = eventExpanded;
                        if (!eventExpanded) continue;

                        foreach ((Type _, object listener) in eventGroup)
                        {
                            DrawListenerRow(listener, indentLevel: 2);
                        }
                    }
                }
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawFoldout(int baseIndent, ref bool expanded, string label, string iconName, Color backgroundColor)
        {
            float height = EditorGUIUtility.singleLineHeight + 4;
            Rect controlRect = EditorGUILayout.GetControlRect(hasLabel: false, height);
            controlRect.xMin += baseIndent * 15;

            Color originalBg = GUI.backgroundColor;
            GUI.backgroundColor = backgroundColor;
            GUI.Box(controlRect, GUIContent.none);
            GUI.backgroundColor = originalBg;

            Rect foldRect = controlRect;
            foldRect.xMin += 4;
            Texture icon = EditorGUIUtility.IconContent(iconName).image;
            GUIContent content = new(label, icon);
            expanded = EditorGUI.Foldout(foldRect, foldout: expanded, content, toggleOnLabelClick: true);
        }

        private void DrawListenerRow(object listener, int indentLevel)
        {
            Color color1 = new(r: 0.1f, g: 0.1f, b: 0.1f);
            float height = EditorGUIUtility.singleLineHeight + 2;
            Rect controlRect = EditorGUILayout.GetControlRect(hasLabel: false, height);
            controlRect.xMin += indentLevel * 16;

            Component component = listener as Component;
            UnityEngine.Object unityObject = component != null ? component : listener as UnityEngine.Object;
            string label = component != null
                ? component.gameObject.name + " (" + component.GetType().Name + ")"
                : listener.GetType().Name;

            Texture icon = unityObject != null
                ? EditorGUIUtility.ObjectContent(unityObject, unityObject.GetType()).image
                : EditorGUIUtility.IconContent(IconName.Info).image;
            GUIContent content = new(label, icon);

            Color backgroundColor = GUI.backgroundColor;
            GUI.backgroundColor = color1;
            GUI.Box(controlRect, GUIContent.none);
            GUI.backgroundColor = backgroundColor;

            if (GUI.Button(controlRect, content, EditorStyles.label) && unityObject != null)
            {
                EditorGUIUtility.PingObject(unityObject);
            }
        }
    }
}