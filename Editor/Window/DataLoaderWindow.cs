﻿
using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Security.Cryptography;
using UnityEngine;

using DataEngine.Data;
using DataEngine.Window;
using static PlasticPipe.Server.MonitorStats;

namespace DataEditor.Window
{
    public class DataLoaderWindow : DataWindow
    {
        [MenuItem("Tools/Data Loader")]
        private static void ShowWindow()
        {
            GUIContent content = EditorGUIUtility.IconContent("SaveActive");
            content.text = "Data Loader";
            content.tooltip = "Show properties of the local data persistent system";

            if (s_window == null)
            {
                s_window = GetWindow<DataLoaderWindow>();
                s_window.titleContent = content;
                ScriptableObject target = s_window;
                s_windowObject = new SerializedObject(target);
                s_window.Show();
            }
            else
            {
                s_window.titleContent = content;
                ScriptableObject target = s_window;
                s_windowObject = new SerializedObject(target);

                if (s_window.docked)
                    s_window.ShowTab();
                else
                    s_window.Show();
            }
        }

        private int m_keySizeFactor = 2;
        private Vector2 m_scrollView;
        private ObjectLoader[] m_loaders;
        private Texture2D m_localTexture, m_libraryTexture;

        private void OnEnable()
        {
            m_loaders = FindObjectsOfType<ObjectLoader>(true);
            m_libraryTexture = AssetDatabase.LoadAssetAtPath<Texture2D>("Library/PackageCache/com.fms.core/Assets/Icons/save.png");
            m_libraryTexture = AssetDatabase.LoadAssetAtPath<Texture2D>("Packages/com.fms.core/Assets/Icons/save.png");

            if (EditorPrefs.HasKey("DataLoader.encryption"))
            {
                DataLoader.encryption = EditorPrefs.GetBool("DataLoader.encryption");
            }
            if (EditorPrefs.HasKey("DataLoader.autoSave"))
            {
                DataLoader.autoSave = EditorPrefs.GetBool("DataLoader.autoSave");
            }
            if (EditorPrefs.HasKey("DataLoader.key"))
            {
                DataLoader.key = EditorPrefs.GetString("DataLoader.key");
            }
            if (EditorPrefs.HasKey("DataLoader.keySize"))
            {
                m_keySizeFactor = EditorPrefs.GetInt("DataLoader.keySize");
            }
            if (EditorPrefs.HasKey("DataLoader.pathType"))
            {
                DataLoader.pathType = (DataLoader.DataLoaderPath)EditorPrefs.GetInt("DataLoader.pathType");
            }
            if (EditorPrefs.HasKey("DataLoader.path"))
            {
                DataLoader.path = EditorPrefs.GetString("DataLoader.path");
            }
            if (EditorPrefs.HasKey("DataLoader.currentPath"))
            {
                DataLoader.path = EditorPrefs.GetString("DataLoader.currentPath");
            }
        }

        private void OnDisable()
        {
            EditorPrefs.SetInt("DataLoader.pathType", (int)DataLoader.pathType);
            EditorPrefs.SetInt("DataLoader.keySize", m_keySizeFactor);
            EditorPrefs.SetString("DataLoader.path", DataLoader.path);
            EditorPrefs.SetString("DataLoader.key", DataLoader.key);
            EditorPrefs.SetBool("DataLoader.encryption", DataLoader.encryption);
            EditorPrefs.SetBool("DataLoader.autoSave", DataLoader.autoSave);
            EditorPrefs.SetString("DataLoader.currentPath", DataLoader.currentPath);
        }

        private Color oldColor = Color.white;

        private void OnGUI()
        {
            //GENERAL
            GUIStyle style = EditorStyles.boldLabel;
            style.alignment = TextAnchor.MiddleCenter;

            EditorGUILayout.BeginVertical("Box");
            GUILayout.Label("General Settings", style); Separator(2); GUILayout.Space(4);
            DataLoader.autoSave = EditorGUILayout.Toggle("Auto Save", DataLoader.autoSave);
            DataLoader.encryption = EditorGUILayout.Toggle("Encryption", DataLoader.encryption);
            DataLoader.key = EditorGUILayout.TextField("Key", DataLoader.key);

            int keySize = AesOperation.ALGORITHM_BASE * m_keySizeFactor;
            m_keySizeFactor = EditorGUILayout.IntSlider($"Key Size [{keySize}]", m_keySizeFactor, 2, 4);

            EditorGUILayout.BeginHorizontal();
            if(GUILayout.Button("Generate Key"))
            {
                var aes = Aes.Create();
                aes.KeySize = AesOperation.ALGORITHM_BASE * m_keySizeFactor;
                aes.GenerateKey();
                DataLoader.key = Convert.ToBase64String(aes.Key);
                aes.Dispose();
            }
            if (GUILayout.Button("Reset Key"))
            {
                DataLoader.key = AesOperation.KEY;
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();

            //PATH
            EditorGUILayout.BeginVertical("Box");
            GUILayout.Label("Path Settings", style); 
            Separator(2);
            GUILayout.Space(4);
            DataLoader.pathType = (DataLoader.DataLoaderPath)EditorGUILayout.EnumPopup("Path Type",DataLoader.pathType);
            EditorGUILayout.BeginHorizontal();
            DataLoader.path = EditorGUILayout.TextField("Path", DataLoader.path);
            if (GUILayout.Button("[...]", GUILayout.MaxWidth(55)))
            {
                DataLoader.path = EditorUtility.OpenFolderPanel("Select Data Path", string.Empty, string.Empty);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.TextField("Current Path", DataLoader.currentPath);
            if(GUILayout.Button("[...]", GUILayout.MaxWidth(55)))
            {
                System.Diagnostics.Process.Start(DataLoader.currentPath);
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();

            //LOADERS
            EditorGUILayout.BeginVertical("Box");
            EditorGUILayout.LabelField($"Loaders ({EditorSceneManager.GetActiveScene().name})", EditorStyles.boldLabel);
            GUIContent content;

            Separator(2);
            m_scrollView = EditorGUILayout.BeginScrollView(m_scrollView);

            if(m_loaders == null || m_loaders.Length <= 0)
            {
                EditorGUILayout.LabelField("No Object Loaders Found");
            }

            foreach (var loader in m_loaders)
            {
                if(!loader) { continue; }

                EditorGUILayout.BeginHorizontal();
                content = new GUIContent(m_localTexture != null ? m_localTexture : m_libraryTexture);
                EditorGUILayout.LabelField(content, GUILayout.MaxWidth(20));
                EditorGUILayout.LabelField(loader.name);
                if (GUILayout.Button("Select", GUILayout.MaxWidth(80))){
                    Selection.activeObject = loader;
                    EditorGUIUtility.PingObject(loader);
                }
                GUI.color = Color.red;
                if (GUILayout.Button("Delete", GUILayout.MaxWidth(80)))
                {
                    if (loader.Delete())
                    {
                        ModalPopupWindow.ShowPopup("Deleted File");
                    }
                    else
                    {
                        ModalPopupWindow.ShowPopup("Falied To Delete File");
                    }
                }
                GUI.color = Color.white;
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical(); 

        }

        private void Separator(int height)
        {
            var rect = GUILayoutUtility.GetLastRect();
            rect.height = height;
            rect.y += EditorGUIUtility.singleLineHeight + height; 
            EditorGUI.DrawRect(rect, Color.grey);
            GUILayout.Space(height * 2);
        }
    }
}