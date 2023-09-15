#if UNITY_EDITOR
namespace PowerUtilities
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEditor;
    using System;

    /// <summary>
    /// helper CustomEditor
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class PowerEditor<T> : Editor where T : class
    {
        /// <summary>
        /// Show version in inspector when not empty
        /// </summary>
        public virtual string Version { get; } ="";

        public (string, bool) foldInfo = ("Options",true);

        /// <summary>
        /// Draw Default Inspector
        /// </summary>
        /// <returns></returns>
        public virtual bool NeedDrawDefaultUI() => false;

        public override void OnInspectorGUI()
        {
            if (!string.IsNullOrEmpty(Version))
            {
                EditorGUITools.DrawColorLabel(new Rect(100, 5, 100, 16), new GUIContent(Version), Color.cyan);
            }

            if (NeedDrawDefaultUI())
                DrawDefaultInspector();

            var inst = target as T;
            serializedObject.UpdateIfRequiredOrScript();

            GUILayout.BeginVertical("Box");
            DrawInspectorUI(inst);
            GUILayout.EndVertical();

            serializedObject.ApplyModifiedProperties();
        }

        public abstract void DrawInspectorUI(T inst);
    }
}
#endif