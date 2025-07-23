using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using DataEngine.Data;
using System.Linq;
using System.Reflection;
using DataEditor.Procedural;
using DataEditor.Window;

namespace DataEditor.Data
{
    [CustomEditor(typeof(ObjectLoader))]
    public class ObjectLoaderEditor : Editor
    {
        private static PersistentUniqueGenerator s_generator = new PersistentUniqueGenerator();

        private static void OnEditorQuit()
        {
            s_generator.Save();
        }

        private ObjectLoader m_target;
        private bool m_isGeneratedId;
        private GUIContent m_content;

        private void OnEnable()
        {
            m_target = (ObjectLoader)target;
            EditorApplication.quitting += OnEditorQuit;

            s_generator.Load();
        }

        public override void OnInspectorGUI()
        {
            KeyInspector();
            DrawDefaultInspector();
            if(m_target.components.Where(x => x.GetType() == typeof(ObjectLoader)).Any())
            {
                EditorGUILayout.HelpBox("No Self Container Allowed", MessageType.Error);
            }
            GUILayout.Space(10);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("mode"));
            if(m_target.mode == ObjectLoader.LoadMode.UseId)
            {
                IdInspector();
            }
            EditorGUILayout.PropertyField(serializedObject.FindProperty("encryption"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("threadLock"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("folder"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("fileName"));
            
            GUILayout.Space(10);
            LoadSaveInspector();
            ConfigInspector();

            serializedObject.ApplyModifiedProperties();
        }

        private void KeyInspector()
        {
            BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
            var keyField = m_target.GetType().GetField("m_key", flags);
            GUI.enabled = false;
            EditorGUILayout.TextField((string)keyField.GetValue(m_target));
            GUI.enabled = true;
        }

        private void IdInspector()
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("id", GUILayout.MaxWidth(20));

            GUI.enabled = !m_isGeneratedId;
            EditorGUILayout.PropertyField(serializedObject.FindProperty("id"), new GUIContent(string.Empty));
            GUI.enabled = true;

            if (GUILayout.Button("[+]", GUILayout.MaxWidth(35))) 
            {
                m_target.id = s_generator.GenerateValue();
                m_isGeneratedId = true;
            }

            if (GUILayout.Button("[-]", GUILayout.MaxWidth(35))) 
            {
                s_generator.RemoveValue(m_target.id);
                m_isGeneratedId = false;
                m_target.id = 0;
            }

            if (GUILayout.Button("[Clear]", GUILayout.MaxWidth(40)))
            {
                s_generator.Clear();
                m_target.id = 0;
            }
            EditorGUILayout.EndHorizontal();
        }

        private void LoadSaveInspector()
        {
            EditorGUILayout.BeginHorizontal("Box");
            if (GUILayout.Button("Load"))
            {
                m_target.Load();
            }
            if (GUILayout.Button("Save"))
            {
                m_target.Save();
            }
            EditorGUILayout.EndHorizontal();
        }

        private void ConfigInspector()
        {
            EditorGUILayout.BeginHorizontal("Box");
            if (GUILayout.Button("Path"))
            {
                System.Diagnostics.Process.Start(Application.persistentDataPath);
            }
            if (GUILayout.Button("Setup"))
            {
                var components = m_target.GetComponents<Component>();
                m_target.components = components.Where(x => x != m_target).ToList();
            }
            if (GUILayout.Button("Delete"))
            {
                if(m_target.Delete())
                    ModalPopupWindow.ShowPopup("File Deleted");
                else
                    ModalPopupWindow.ShowPopup("Falied To Delete File");
            }
            EditorGUILayout.EndHorizontal();
        }
    }
}
