using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.Events;

namespace PowerUtilities
{
    /// <summary>
    /// ui components(Graphic) use material clone
    /// Renderers use MaterialPropertyBlock update variables
    /// </summary>
    public abstract class BaseUIMaterialPropUpdater : MonoBehaviour
    {
        [Header("UI Graphs")]
        public Graphic[] graphs;
        [SerializeField]List<Material> graphSharedMaterialList = new List<Material>();

        [Header("Renderers")]
        public Renderer[] renderers;
        
        static MaterialPropertyBlock rendererBlock;
        // private vars
        [HideInInspector][SerializeField] bool isInited;

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

            var isFirstRun = graphSharedMaterialList.Count == 0
                && !isInited;

            if (enabled && isFirstRun)
            {
                var firstMat = isRenderersValid ? renderers[0].sharedMaterial : graphs[0].materialForRendering;
                if (firstMat)
                    ReadFirstMaterial(firstMat);
            }

            if(isFirstRun)
                SaveGraphMaterials();

            InstantiateGraphMaterials();
        }

        void InstantiateGraphMaterials()
        {
            graphs.ForEach((g, id) =>
            {
                var targetMat = graphSharedMaterialList[id] ?? g.material;
                if (targetMat && !g.material.name.Contains("Clone"))
                    g.material = Instantiate(targetMat);
            });
        }

        private void SaveGraphMaterials()
        {
            graphs.ForEach((g) => graphSharedMaterialList.Add(g.material));
        }

        private void LateUpdate()
        {
            if (rendererBlock == null)
                rendererBlock= new MaterialPropertyBlock();

            MaterialPropCodeGenTools.UpdateComponentsMaterial(graphs, (graph, id) =>
            {
                UpdateMaterial(graph.materialForRendering);
                //graph.SetMaterialDirty();
            });
            MaterialPropCodeGenTools.UpdateComponentsMaterial(renderers, (render, id) =>
            {
                rendererBlock.Clear();
                render.GetPropertyBlock(rendererBlock);
                UpdateBlock(rendererBlock);
                render.SetPropertyBlock(rendererBlock);
            });
        }

        void ResetGraphsMaterial()
        {
            graphs.ForEach((g, id) => g.material = graphSharedMaterialList[id]);
            graphSharedMaterialList.Clear();
        }

        private void OnDestroy()
        {
            ResetGraphsMaterial();
            rendererBlock =null;
        }

        [ContextMenu("Reset")]
        private void Reset()
        {
            ResetGraphsMaterial();
            if (Application.isEditor)
                Start();
        }

        public abstract void ReadFirstMaterial(Material mat);

        public abstract void UpdateMaterial(Material mat);
        public abstract void UpdateBlock(MaterialPropertyBlock block);
    }
}
