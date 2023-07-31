#if UNITY_EDITOR
using PowerUtilities.RenderFeatures;
using PowerUtilities.UIElements;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using UnityEngine.UIElements;

namespace PowerUtilities
{
    public class SFCGraphWindow : BaseUXMLEditorWindow
    {
        
        [OnOpenAsset]
        static bool OnOpenSFC(int instanceId,int line)
        {
            var srpList = Selection.activeObject as SRPFeatureListSO;
            if (srpList != null)
            {
                OpenWindow();
            }
            return srpList;
        }

        public static void OpenWindow()
        {
            var win = GetWindow<SFCGraphWindow>();
            if (win.position.width < 1000)
                win.position = new Rect(100, 100, 1000, 800);
        }

        public SFCGraphWindow()
        {
            base.IsReplaceRootContainer = true;
        }

        public override void CreateGUI()
        {
            base.treeAsset = AssetDatabaseTools.FindAssetPathAndLoad<VisualTreeAsset>(out _, "SFCGraphWindow", "uxml");
            base.CreateGUI();
        }
    }
}
#endif