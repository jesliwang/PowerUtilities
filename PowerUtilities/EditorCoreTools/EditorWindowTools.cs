#if UNITY_EDITOR
namespace PowerUtilities
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEditor;
    using UnityEngine;

    public static class EditorWindowTools
    {

        public static EditorWindow[] GetWindows()
        {
            return Resources.FindObjectsOfTypeAll<EditorWindow>();
        }

        /// <summary>
        /// Get opened window
        /// </summary>
        /// <param name="typeName"></param>
        /// <returns></returns>
        public static EditorWindow GetWindow(string typeName)
        {
            return Resources.FindObjectsOfTypeAll<EditorWindow>()
                .Where(w => w.GetType().Name ==typeName)
                .FirstOrDefault()
                ;
        }

        public static bool IsFocused(EditorWindow window) => EditorWindow.focusedWindow == window;
        
        public static void OpenWindow<T>(string title="") where T : EditorWindow
        {
            var win = EditorWindow.GetWindow<T>();
            if (!win)
                return;
            if(string.IsNullOrEmpty(title))
            {
                title = typeof(T).Name;
            }
            win.titleContent = new GUIContent(title);
            if (win.position.width <= 0)
                win.position = new Rect(100, 100, 800, 600);
        }
    }
}
#endif