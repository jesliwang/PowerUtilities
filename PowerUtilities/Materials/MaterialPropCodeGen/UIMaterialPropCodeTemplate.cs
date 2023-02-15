using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

namespace PowerUtilities
{
    //[ExecuteInEditMode]
    public class UIMaterialPropCodeTemplate : MonoBehaviour
    {
        public Color color = Color.white;

        [Header("UI Graphs")]
        public Graphic[] graphs;
        public bool useGraphMaterialInstance;

        [Header("Renderers")]
        public Renderer[] renderers;
        
        static MaterialPropertyBlock rendererBlock;

        private void Start()
        {
            if (graphs == null || graphs.Length == 0)
            {
                if (gameObject.TryGetComponent<Graphic>(out var comp))
                    graphs =  new[] { comp };
            }

            if (renderers == null || renderers.Length == 0)
            {
                if (gameObject.TryGetComponent<Renderer>(out var comp))
                    renderers =  new[] { comp };
            }

            var isRenderersValid = (renderers != null && renderers.Length>0);
            var isGraphsValid = (graphs != null && graphs.Length>0);
            enabled = isRenderersValid || isGraphsValid;

            if (useGraphMaterialInstance)
                graphs.ForEach(graph => graph.material = Instantiate(graph.material));

            if (enabled)
            {
                var firstMat = isRenderersValid ? renderers[0].sharedMaterial : graphs[0].material;
                if (firstMat)
                    ReadFirstMaterial(firstMat);
            }
        }

        private void Update()
        {
            if(rendererBlock == null)
                rendererBlock= new MaterialPropertyBlock();

            MaterialPropCodeGenTools.UpdateComponentsMaterial(graphs, (graph,id) => {
                UpdateMaterial(graph.material);
                graph.SetMaterialDirty();
            });
            MaterialPropCodeGenTools.UpdateComponentsMaterial(renderers, (render,id) => {
                rendererBlock.Clear();
                render.GetPropertyBlock(rendererBlock);
                UpdateBlock(rendererBlock);
                render.SetPropertyBlock(rendererBlock);
            });
        }

        private void OnDestroy()
        {
            rendererBlock =null;
        }
        private void Reset()
        {
            if(Application.isEditor)
                Start();
        }

        void ReadFirstMaterial(Material mat)
        {
            color= mat.color;
        }

        void UpdateMaterial(Material mat)
        {
            if (!mat)
                return;

            mat.SetColor("_Color", color);
        }

        void UpdateBlock(MaterialPropertyBlock block)
        {
            if (block == null)
                return;

            block.SetColor("_Color",color);
        }
    }
}
