﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace GameUtilsFramework
{

    public static class InputSystemMacroDefine
    {
        const string ROOT_PATH = "PowerUtilities";

        /// <summary>
        /// enabled script macros(inputSystem
        /// </summary>
        [MenuItem(ROOT_PATH+"/InputSystem/SyncInputSystemMacro")]
        public static void SyncInputSystemMacro()
        {
            var playerSettings = Resources.FindObjectsOfTypeAll<PlayerSettings>().First();
            var playerSettingObject = new SerializedObject(playerSettings);

            var activeInputHandlerSP = playerSettingObject.FindProperty("activeInputHandler");
            var inputSystemEnabled = activeInputHandlerSP.intValue>0;
            var buildTargetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;

            var scriptDefines = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);
            scriptDefines = scriptDefines.Replace(";INPUT_SYSTEM_ENABLED", "");
            if (inputSystemEnabled)
                scriptDefines += ";INPUT_SYSTEM_ENABLED";
            PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, scriptDefines);
        }
    }
}
#endif