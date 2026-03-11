
// SIGNATURE :)

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.quatworks.INFRASEC.Editor {

    public class USSPersistence : EditorWindow {

        private VisualElement selected = null;

        [MenuItem("Window/INFRAUIDump")]
        public static void ShowWindow() {
            USSPersistence window = GetWindow<USSPersistence>("INFRAUIDump");
            window.Show();
        }

        private void OnGUI() {
            EditorGUILayout.LabelField("USS Dump", EditorStyles.boldLabel);

            if(GUILayout.Button("Update Element"))
                selected = GetSelectedVisualElement();

            if(selected == null) {
                GUI.enabled = false;
                GUILayout.Button($"Dump 'NONE'");
                GUI.enabled = true;
                return;
            }

            if(GUILayout.Button($"Dump '{selected.name}'"))
                DumpStyles();
        }

        public void DumpStyles() {

            if(selected == null) {
                Debug.LogError("No focused VisualElement found for USS dump.");
                return;
            }

            string path = EditorUtilities.GetSelectedFolderPath();
            path = Path.Combine(path, $"{selected.name}_dump.txt");

            StringBuilder dumps = new();
            DumpStyleOf(selected, dumps, 0);
            File.WriteAllText(path, dumps.ToString());
            AssetDatabase.Refresh();
        }

        private static void DumpStyleOf(VisualElement element, StringBuilder dumps, int depth) {
            string indent = new string(' ', depth * 2);
            string sel = !string.IsNullOrEmpty(element.name) ? $"#{element.name}" : element.GetType().Name;
            
            dumps.AppendLine($"{indent}{sel} {{");

            foreach(PropertyInfo style in element.resolvedStyle.GetType().GetProperties()) {
                object styleValue = style.GetValue(element.resolvedStyle);
                if(styleValue == null) continue;
                dumps.AppendLine($"{indent} {style.Name}: {styleValue}");
            }

            dumps.AppendLine($"{indent}}}");

            foreach(VisualElement child in element.Children())
                DumpStyleOf(child, dumps, depth++);
        }


        private static VisualElement GetSelectedVisualElement() {

            Assembly asm = AppDomain.CurrentDomain.GetAssemblies().ToList().FirstOrDefault(a => a.GetName().Name == "UnityEditor.UIElementsModule");
            if(asm == null) {
                Debug.LogError("UIElementsModule assembly couldn't found");
                return null;
            }

            Type dtype = asm.GetType("UnityEditor.UIElements.Debugger.UIElementsDebugger");
            if(dtype == null) {
                Debug.LogError("UIDebugger couldn't be located in the assembly (??)");
                return null;
            }

            UnityEngine.Object[] windowInstances = Resources.FindObjectsOfTypeAll(dtype);
            if(windowInstances.Length <= 0) {
                Debug.LogError("No UIDebugger windows are open");
                return null;
            }

            Type window = windowInstances[0].GetType();
            
            FieldInfo ctx = window.GetField("m_DebuggerContext", BindingFlags.NonPublic | BindingFlags.Instance);
            if(ctx == null) {
                Debug.LogError("No selected UIDebugger or UIDebugger element could be found");
                return null;
            }

            object instance = ctx.GetValue(windowInstances[0]);

            PropertyInfo target = instance.GetType().GetProperty("selectedElement", BindingFlags.Public | BindingFlags.Instance);
            if(target == null) {
                Debug.LogError("No selected VisualElement within the current UIDebugger instance could be found");
                return null;
            }

            return (VisualElement)target.GetValue(instance);
        }
    }
    
}