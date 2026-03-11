using System;
using System.IO;
using UnityEditor;
using UnityEngine;

// SIGNATURE :)

namespace Assets.quatworks.INFRASEC.Editor {

    public static class EditorUtilities {

        public static string GetSelectedFolderPath() {
            string folderPath = AssetDatabase.GetAssetPath(Selection.activeObject);
            if(string.IsNullOrEmpty(folderPath)) return "";
            if(!AssetDatabase.IsValidFolder(folderPath))
                return Path.GetDirectoryName(folderPath);
            return folderPath;
        }

        public static string GetNamespaceFromFolder(string folderPath) {
            string relativePath = folderPath.Replace('/', '.');
            return string.IsNullOrEmpty(relativePath) ? "INFRA" : relativePath;
        }

        /// <summary>
        /// Adds a completely empty MonoBehaviour with an automatic
        /// namespace declaration based on its filepath destination.
        /// <para/>
        /// If no path is specified, the file will be written to the currently
        /// selected folder.
        /// </summary>
        /// <param name="name"></param>
        public static void AddBlankScript(string name, string path = "") {

            if(string.IsNullOrEmpty(path)) path = GetSelectedFolderPath();
            if(string.IsNullOrEmpty(path)) {
                Debug.LogError("Invalid folder path");
                return;
            }
            
            if(string.IsNullOrEmpty(name)) {
                Debug.LogError("Invalid asset name");
                return;
            }

            string namespacePath = GetNamespaceFromFolder(path);

            string content = 

$@"
// SIGNATURE :)

using UnityEngine;

namespace {namespacePath} {{

    public class {name} : MonoBehaviour {{
        
    }}
}}";

            File.WriteAllText(Path.Combine(path, name + ".cs"), content);
            AssetDatabase.Refresh();
        }
    }

    public class CustomScriptAdder : UnityEditor.Editor {
        [MenuItem("Assets/INFRA - New Script")]
        private static void InitiateDialog() {
            NamingWindow.InvokePopup(EditorUtilities.GetSelectedFolderPath(), "INFRA - New Script");
        }
    }

    public class NamingWindow : EditorWindow {
        private string assetName = "NewAsset";
        private string folderPath;
        private string windowName;

        private bool invokeNeedsFocus = true;

        public static void InvokePopup(string folderPath, string windowName) {
            NamingWindow popup = GetWindow<NamingWindow>(windowName);
            popup.windowName = windowName;
            popup.folderPath = folderPath;
            popup.minSize = new Vector2(10, 10);
            popup.Show();
        }


        private void OnGUI() {
            EditorGUILayout.LabelField(windowName, EditorStyles.boldLabel);

            GUI.SetNextControlName("ItemNameField");
            assetName = EditorGUILayout.TextField("Name", assetName);
            if(invokeNeedsFocus) {
                EditorGUI.FocusTextInControl("ItemNameField");
                invokeNeedsFocus = false;
            }

            if(IsEnterPressed()) {
                if (string.IsNullOrWhiteSpace(assetName)) {
                    Debug.LogError("Asset name cannot be empty.");
                    return;
                }

                EditorUtilities.AddBlankScript(assetName, folderPath);
                Close();
            }
        }

        private bool IsEnterPressed() {
            Event e = Event.current;
            return e.type == EventType.KeyDown && e.keyCode == KeyCode.Return;
        }
    }
}
