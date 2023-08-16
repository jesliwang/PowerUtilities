﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Rendering;
using static PowerUtilities.MaterialGroupTools;

namespace PowerUtilities
{
    /// <summary>
    /// draw instanced config
    /// </summary>
    public class DrawChildrenInstancedSO : ScriptableObject
    {
        [Header("Lightmap")]
        public Texture2D[] lightmaps;
        public Texture2D[] shadowMasks;
        public bool enableLightmap = true;

        [Tooltip("Lighting Mode is subtractive?")]
        public bool isSubstractiveMode = false;

        [Header("销毁概率,[0:no, 1:all]")]
        [Range(0, 1)] public float culledRatio = 0f;
        public bool forceRefresh;

        [Header("Children Filters")]
        [Tooltip("object 's layer ")]
        public LayerMask layers = -1;

        [Tooltip("find children include inactive")]
        public bool includeInactive;

        [Header("When done")]
        [Tooltip("disable children when add to instance group")]
        public bool disableChildren = true;

        [Space(10)]
        public List<InstancedGroupInfo> groupList = new List<InstancedGroupInfo>();

        [HideInInspector] public Renderer[] renders;
        /// <summary>
        /// index of all DrawChildrenInstanced
        /// </summary>
        public int drawChildrenId;

        static MaterialPropertyBlock block;

        public void Update()
        {
            if (forceRefresh)
            {
                forceRefresh = false;

                CullInstances(1 - culledRatio);
            }
        }

        public bool IsLightMapEnabled()
        {
            return enableLightmap && lightmaps != null && lightmaps.Length > 0;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="rootGo"></param>
        public void SetupChildren(GameObject rootGo)
        {
            // fitler children
            SetupRenderers(rootGo);

            if (renders.Length == 0)
                return;

            groupList.Clear();
            SetupGroupList(renders, groupList);
            SetupLightmaps();

            // lightmaps handle
            //if (IsLightMapEnabled())
            //{
            //    SetupGroupLightmapInfo();
            //}

            if (disableChildren)
                rootGo.SetChildrenActive(false);

            // refresh culling
            forceRefresh = true; 
        }

        /// <summary>
        /// find valid renders,
        /// (layer,tag( exclude EditorOnly,
        /// need sharedMaterial,sharedMesh
        /// </summary>
        /// <param name="rootGo"></param>
        /// <param name="includeInactive"></param>
        /// <param name="layers"></param>
        /// <param name="renders"></param>
        public void SetupRenderers(GameObject rootGo)
        {
            var q = rootGo.GetComponentsInChildren<MeshRenderer>(includeInactive)
                .Where(r =>
                {
                    // layer
                    var isValid = LayerMaskEx.Contains(layers, r.gameObject.layer);
                    // exclude EditorOnly
                    isValid = isValid && !r.gameObject.CompareTag(Tags.EditorOnly);
                    // material
                    isValid = isValid && r.sharedMaterial;

                    // open isntaced
                    if (isValid && !r.sharedMaterial.enableInstancing)
                        r.sharedMaterial.enableInstancing = true;

                    var mf = r.GetComponent<MeshFilter>();
                    return isValid && mf && mf.sharedMesh;
                });

            renders = q.ToArray();
        }

        /// <summary>
        /// group renders by(lgihtmapIndex,sharedMesh,shaderMaterial)
        /// fill groupList
        /// </summary>
        /// <param name="renders"></param>
        /// <param name="groupList"></param>
        public static void SetupGroupList(Renderer[] renders, List<InstancedGroupInfo> groupList)
        {
            // use lightmapIndex,mesh as group
            var lightmapMeshGroups = renders.GroupBy(r => new { r.lightmapIndex, r.GetComponent<MeshFilter>().sharedMesh,r.sharedMaterial });
            lightmapMeshGroups.ForEach((group, groupId) =>
            {
                // a instanced group
                var groupInfo = new InstancedGroupInfo();
                groupList.Add(groupInfo);

                group.ForEach((r, renderId) =>
                {
                    var boundSphereSize = Mathf.Max(r.bounds.extents.x, Mathf.Max(r.bounds.extents.y, r.bounds.extents.z));
                    groupInfo.AddRender(boundSphereSize, r.GetComponent<MeshFilter>().sharedMesh,r.sharedMaterial, r.transform.localToWorldMatrix, r.lightmapScaleOffset, r.lightmapIndex);
                });
            });
        }

        public void Clear()
        {
            groupList.Clear();
        }

        public void DestroyOrHiddenChildren(bool destroyGameObject)
        {
            foreach (var item in renders)
            {
                // destroy items;
                if (destroyGameObject)
                {
#if UNITY_EDITOR
                    DestroyImmediate(item.gameObject);
#else
                    Destroy(item.gameObject);
#endif
                }
                else
                    item.gameObject.SetActive(false);
            }
        }

        public void SetupLightmaps()
        {
            lightmaps = new Texture2D[LightmapSettings.lightmaps.Length];
            shadowMasks = new Texture2D[LightmapSettings.lightmaps.Length];

            LightmapSettings.lightmaps.ForEach((lightmapData, id) =>
            {
                lightmaps[id] = lightmapData.lightmapColor;
                shadowMasks[id] = lightmapData.shadowMask;
            });
        }

        public static bool IsLightmapValid(int lightmapId, Texture[] lightmaps)
        => lightmapId >-1 && lightmapId< lightmaps.Length && lightmaps[lightmapId];

        void UpdateSegmentBlock(InstancedGroupInfo group, List<Vector4> lightmapCoords, MaterialPropertyBlock block)
        {
            if (IsLightmapValid(group.lightmapId, shadowMasks))
            {
                block.SetTexture("unity_ShadowMask", shadowMasks[group.lightmapId]);
            }

            if (IsLightmapValid(group.lightmapId, lightmaps))
            {
                block.SetTexture("unity_Lightmap", lightmaps[group.lightmapId]);
            }
            if (lightmapCoords.Count >0)
            {
                //block.SetVectorArray("_LightmapST", lightmapGroup.lightmapCoords);
                //block.SetInt("_DrawInstanced", 1);
                block.SetVectorArray("unity_LightmapST", lightmapCoords);
            }

            if (isSubstractiveMode)
                block.SetVector("unity_LightData", Vector4.zero);
        }

        /// <summary>
        /// update group.displayTransformsGroupList,whith culledRatio
        /// </summary>
        /// <param name="culledRatio"></param>
        public void CullInstances(float culledRatio)
        {
            //foreach (var group in groupList)
            groupList.ForEach((group, groupId) =>
            {
                for (int i = 0; i < group.originalTransformsGroupList.Count; i++)
                {
                    var transforms = group.originalTransformsGroupList[i].transforms;
                    // clear all
                    group.originalTransformsGroupList[i].transformShuffleCullingList.SetData(false);

                    // set visible
                    var retainCount = (int)(transforms.Count * Mathf.Clamp01(culledRatio));
                    var shuffleIds = RandomTools.GetShuffle(retainCount);
                    shuffleIds.ForEach(shuffleId =>
                    {
                        group.originalTransformsGroupList[i].transformShuffleCullingList[shuffleId] = true;
                    });
                }
            });
        }

        public void DrawGroupList()
        {
            // objs can visible
            var transforms = new List<Matrix4x4>();
            var lightmapSTs = new List<Vector4>();

            var shadowCasterMode = QualitySettings.shadowmaskMode == ShadowmaskMode.DistanceShadowmask ? ShadowCastingMode.On : ShadowCastingMode.Off;
            groupList.ForEach((group, groupId) =>
            {
                //update material LIGHTMAP_ON
                var lightmapEnable = group.lightmapId != -1 && IsLightMapEnabled();
                LightmapSwitch(group.mat, lightmapEnable);

                group.originalTransformsGroupList.ForEach((segment, sid) =>
                {
                    transforms.Clear();
                    lightmapSTs.Clear();

                    segment.transformVisibleList.ForEach((tr, trId) =>
                    {
                        if (segment.transformVisibleList[trId] && segment.transformShuffleCullingList[trId])
                        {
                            transforms.Add(segment.transforms[trId]);
                            lightmapSTs.Add(group.lightmapCoordsList[sid].lightmapCoords[trId]);
                        }
                    });

                    if (transforms.Count==0)
                        return;

                    //if (group.blockList.Count <= sid)
                    //    group.blockList.Add(new MaterialPropertyBlock());

                    //var block = group.blockList[sid];
                    if (block == null)
                        block = new MaterialPropertyBlock();

                    UpdateSegmentBlock(group, lightmapSTs, block);

                    Graphics.DrawMeshInstanced(group.mesh, 0, group.mat, transforms, block, shadowCasterMode);
                    block.Clear();
                });
            });
        }

        private static void LightmapSwitch(Material mat, bool lightmapEnable)
        {
            if (mat.IsKeywordEnabled("LIGHTMAP_ON") != lightmapEnable)
                mat.SetKeyword("LIGHTMAP_ON", lightmapEnable);
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            groupList.ForEach((group, groupId) =>
            {
                sb.AppendLine(groupId.ToString());

                for (int i = 0; i < group.originalTransformsGroupList.Count; i++)
                {
                    var instancedGroup = group.originalTransformsGroupList[i];

                    sb.AppendLine("tr group : "+i);
                    sb.AppendLine(instancedGroup.ToString());
                }
            });

            return sb.ToString();
        }
    }
}
