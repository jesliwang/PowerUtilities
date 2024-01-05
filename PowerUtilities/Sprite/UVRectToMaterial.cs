namespace PowerUtilities
{
#if UNITY_EDITOR
    using UnityEditor;
#endif
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using System;
    using UnityEngine.UI;
    using Unity.Mathematics;
    using Object = UnityEngine.Object;

#if UNITY_EDITOR

    [CustomEditor(typeof(UVRectToMaterial))]
    public class UVRectToMaterialEditor : PowerEditor<UVRectToMaterial>
    {
        public override bool NeedDrawDefaultUI() => true;
        public override string Version => "0.0.1";

        public override void DrawInspectorUI(UVRectToMaterial inst)
        {
            if (GUILayout.Button("Set sprite uv"))
            {
                inst.SetUV();
            }
        }
    }
#endif
    /// <summary>
    /// send sprite's uv in altas to material(_MainTex_ST
    /// </summary>
    [ExecuteAlways]
    public class UVRectToMaterial : MonoBehaviour
    {
        [HelpBox]
        public const string helpStr = "Send sprite's uv in atlas to material";

        public Sprite sprite;
        Sprite lastSprite;

        [Header("Material Options")]
        [Tooltip("generate material instance")]
        public bool isUseMaterialInstance;

        [Tooltip("shader corresponding texture name")]
        public string _MainTexName = "_MainTex";

        [Tooltip("sprite's start uv")]
        public string _SpriteUVStartName = "_SpriteUVStart";

        [Header("PowerVFX Options")]
        [Tooltip("disable texture auto offset")]
        public bool isDisableMainTexAutoOffset = true;

        [Tooltip("use powervfx minVersion")]
        public bool isUseMinVersion = true;


        Renderer render; // 3d renderer
        Image uiImage; // ui
        Material mat; // current used material
        //=========================
        [Header("DebugInfo")]
        //[EditorGroup("DebugInfo",true)]
        [SerializeField]
        //[ListItemDraw("x,y,z,w", "100,100,100,100")]
        Vector4 spriteRect;

        //[EditorGroup("DebugInfo")]
        [SerializeField]
        //[ListItemDraw("x:,x,y:,y,z:,z,w:,w", "15,50,15,80,15,80,15,80")]
        Vector4 spriteUVST;

        Object lastSelectionObject;

        private void Update()
        {
            if(lastSprite != sprite)
            {
                lastSprite = sprite;
                SetUV();
            }
        }

        private void OnDisable()
        {
            if (mat)
            {
                //mat.SetVector($"{_MainTexName}_ST", spriteUVST);
                mat.SetVector(_SpriteUVStartName, new Vector2(0,0));
            }
        }
        public void SetUV()
        {
            if (!render)
                render = GetComponent<Renderer>();

            if (!uiImage)
            {
                uiImage = GetComponent<Image>();
            }

            // setup Image
            if (uiImage && uiImage.sprite)
            {
                sprite = uiImage.sprite;
                uiImage.sprite = null; // only use material's texture
            }

            if (!sprite || ( !render && !uiImage))
                return;

            mat = GetMaterial();
            if (!mat)
                return;

            mat.SetTexture(_MainTexName, sprite.texture);

            var rect = sprite.rect;
            spriteRect = new Vector4(rect.x, rect.y, rect.width, rect.height);
            spriteUVST = sprite.GetSpriteUVScaleOffset();
            mat.SetVector($"{_MainTexName}_ST", spriteUVST);
            mat.SetVector(_SpriteUVStartName, new Vector2(spriteUVST.z, spriteUVST.w));

            TrySetupPowerVFXMat(mat);
        }

        Material GetMaterial()
        {
            if (render)
                return isUseMaterialInstance ? render.material : render.sharedMaterial;

            if (uiImage)
            {
                // dont change default UI material
                if (uiImage.materialForRendering == uiImage.defaultMaterial)
                    return null;

                return uiImage.materialForRendering;
            }

            return null;
        }

        private void TrySetupPowerVFXMat(Material mat)
        {
            if (!mat.shader.name.Contains("PowerVFX"))
                return;

            mat.SetFloat("_MainTexOffsetStop", isDisableMainTexAutoOffset?1:0);

            if(isUseMinVersion != mat.IsKeywordEnabled(ShaderKeywords.MIN_VERSION))
            {
                mat.SetKeyword(ShaderKeywords.MIN_VERSION, isUseMinVersion);


#if UNITY_EDITOR
                StartCoroutine(WaitForSelectAgain());
#endif
            }
        }
#if UNITY_EDITOR
        IEnumerator WaitForSelectAgain()
        {
            lastSelectionObject = Selection.activeObject;
            Selection.activeObject = null;
            yield return new WaitForEndOfFrame();
            Selection.activeObject = lastSelectionObject;
        }
#endif
    }
}
