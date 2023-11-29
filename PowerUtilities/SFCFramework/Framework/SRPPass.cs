﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace PowerUtilities.RenderFeatures
{
    /// <summary>
    /// urp renderPass controlled by SRPFeatureControl
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class SRPPass<T> : ScriptableRenderPass
        where T : SRPFeature
    {
        public T Feature { get; private set; }

        protected Camera camera;
        protected ScriptableRenderContext context;
        public string featureName;

        public SRPPass(T feature)
        {
            Feature = feature;
            featureName = feature.name;
        }

        /// <summary>
        /// Is pass need reset last render target
        /// </summary>
        /// <returns></returns>
        public virtual bool IsTryRestoreLastTargets() => false;

        /// <summary>
        /// Compare with tag or name
        /// </summary>
        /// <param name="cam"></param>
        /// <returns></returns>
        public bool IsGameCameraValid(Camera cam)
        {
            switch (Feature.gameCameraCompareType)
            {
                case SRPFeature.CameraCompareType.Name:
                    return camera.gameObject.name.IsMatch(Feature.gameCameraTag, StringEx.NameMatchMode.Full);
                default:
                    return camera.CompareTag(Feature.gameCameraTag);

            }
        }

        /// <summary>
        /// Reset render target to LastColorTargetIds
        /// </summary>
        /// <param name="cmd"></param>
        public void TryRestoreCameraTargets(CommandBuffer cmd)
        {
            // unity 2021, use nameId, dont use this
#if UNITY_2022_1_OR_NEWER
            if (RenderTargetHolder.IsLastTargetValid())
            {
                //ConfigureTarget(RenderTargetHolder.LastColorTargetRTs, RenderTargetHolder.LastDepthTargetRT);

                cmd.SetRenderTarget(RenderTargetHolder.LastColorTargetIds, RenderTargetHolder.LastDepthTargetRT.nameID);
            }
#endif
        }

        /// <summary>
        /// This pass can execute
        /// 1 check Feature 
        /// 2 check cameraType,
        ///     2.1 check gameCameraTag when cameraType is Game
        /// </summary>
        /// <returns></returns>
        public virtual bool CanExecute()
        { 
            if(Feature == null || !Feature.enabled)
                return false;

            if (Feature.isSceneCameraOnly)
                return camera.cameraType == CameraType.SceneView;

            if (Feature.isEditorOnly)
                return Application.isEditor;

            if (camera.cameraType == CameraType.Game &&!string.IsNullOrEmpty(Feature.gameCameraTag))
                return IsGameCameraValid(camera);

            return true;
            // > sceneView, use urp pass
            //return camera.cameraType <= CameraType.SceneView;
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            ref var cameraData = ref renderingData.cameraData;
            this.camera = cameraData.camera;
            this.context = context;

            if (! CanExecute())
                return;


            var cmd = CommandBufferPool.Get(featureName);
            cmd.Execute(ref context);

            if(IsTryRestoreLastTargets())
                TryRestoreCameraTargets(cmd);

            OnExecute(context, ref renderingData,cmd);

            cmd.Execute(ref context);
        }

        public override void OnFinishCameraStackRendering(CommandBuffer cmd)
        {
            RenderTargetHolder.Clear();
        }

        public abstract void OnExecute(ScriptableRenderContext context, ref RenderingData renderingData, CommandBuffer cmd);
    }
}
