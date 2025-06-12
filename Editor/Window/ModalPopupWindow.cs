using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace DataEditor.Window
{
    public class ModalPopupWindow : EditorWindow
    {
        private string message = "Default Message";
        private System.Action onCloseCallback;

        private static GUIContent warningIcon;

        public static void ShowPopup(string message, System.Action onClose = null)
        {
            ModalPopupWindow window = CreateInstance<ModalPopupWindow>();
            window.titleContent = new GUIContent("[Warning]");
            window.message = message;
            window.onCloseCallback = onClose;
            window.position = new Rect(Screen.width / 2f, Screen.height / 2f, 320, 200);
            window.minSize = new Vector2(320,190);
            window.maxSize = new Vector2(400,220);
            window.ShowUtility();
        }

        private void OnEnable()
        {
            warningIcon = EditorGUIUtility.IconContent("console.warnicon"); // Ícono de advertencia estándar
        }

        private void OnGUI()
        {
            GUILayout.Space(10);

            GUILayout.BeginVertical();
            GUILayout.FlexibleSpace();

            // Ícono centrado
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label(warningIcon, GUILayout.Width(32), GUILayout.Height(32));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            // Mensaje centrado
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label(message, EditorStyles.wordWrappedLabel, GUILayout.Width(260));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.FlexibleSpace();

            // Botón OK centrado
            GUILayout.Space(5);
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("OK", GUILayout.Width(100)))
            {
                onCloseCallback?.Invoke();
                Close();
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.Space(10);
            GUILayout.EndVertical();
        }
    }

}
