namespace PowerUtilities.RenderFeatures
{
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

#if UNITY_EDITOR
    using UnityEditor;
    [CustomEditor(typeof(SRPRenderFeatureControl))]
    public class SRPRenderFeatureControlEditor : Editor
    {
        Editor featureListEditor;
        private void OnEnable()
        {
            var featureListSO = serializedObject.FindProperty(nameof(SRPRenderFeatureControl.featureListSO));
            featureListEditor = CreateEditor(featureListSO.objectReferenceValue);
        }

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();
            DrawDefaultInspector();
            if (EditorGUI.EndChangeCheck())
            {
                OnEnable();
            }


            serializedObject.UpdateIfRequiredOrScript();

            var isFoldout = serializedObject.FindProperty("isFoldout");
            isFoldout.boolValue = EditorGUILayout.Foldout(isFoldout.boolValue, EditorGUITools.TempContent("featureList Details"), true, EditorStylesEx.FoldoutHeader);
            if (isFoldout.boolValue)
            {
                EditorGUI.indentLevel++;
                featureListEditor?.OnInspectorGUI();
                EditorGUI.indentLevel--;
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
#endif

    public class SRPRenderFeatureControl : ScriptableRendererFeature
    {
        public SRPFeatureListSO featureListSO;

        [SerializeField]
        bool isFoldout;


        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if ( featureListSO == null || featureListSO.featureList.Count == 0)
                return;

            foreach (var feature in featureListSO.featureList)
            {
                if (feature == null)
                    continue;

                var pass = feature.GetPass();
                if (pass == null || !feature.enabled)
                    continue;

                pass.renderPassEvent = feature.renderPassEvent + feature.renderPassEventOffset;
                renderer.EnqueuePass(pass);

                if (feature.interrupt)
                    break;
            }
            
        }

        public override void Create()
        {
        }
    }
}
