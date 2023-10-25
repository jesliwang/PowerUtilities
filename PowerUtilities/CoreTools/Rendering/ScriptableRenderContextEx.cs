﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Rendering;

namespace PowerUtilities
{
    /// <summary>
    /// handle compatibility
    /// </summary>
    public static class ScriptableRenderContextEx
    {
        /// <summary>
        /// use cmd.DrawRendererList(unity 2023) or context.DrawRenderers(unity 2021)
        /// </summary>
        /// <param name="context"></param>
        /// <param name="cmd"></param>
        /// <param name="cullingResults"></param>
        /// <param name="drawingSettings"></param>
        /// <param name="filteringSettings"></param>
        public static void DrawRenderers(this ScriptableRenderContext context, CommandBuffer cmd,CullingResults cullingResults, ref DrawingSettings drawingSettings, ref FilteringSettings filteringSettings)
        {
#if UNITY_2023_1_OR_NEWER
            var param = new RendererListParams(cullingResults, drawingSettings, filteringSettings);
            var list = context.CreateRendererList(ref param);
            cmd.DrawRendererList(list);
#else // below 2021
            context.DrawRenderers(cullingResults, ref drawingSettings, ref filteringSettings);
#endif
        }
    }
}
