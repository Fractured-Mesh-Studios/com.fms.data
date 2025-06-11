using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace DataEngine.Window
{
    public class DataWindow : EditorWindow
    {
        protected static DataWindow s_window;
        protected static SerializedObject s_windowObject;
    }
}
