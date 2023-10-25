Shader "Hidden/Universal Render Pipeline/Blit1"
{
    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline"}
        LOD 100

        Pass
        {
            Name "Blit"
            ZTest Always
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex FullscreenVert
            #pragma fragment Fragment
            #pragma multi_compile_fragment _ _LINEAR_TO_SRGB_CONVERSION _SRGB_TO_LINEAR_CONVERSION
            #pragma multi_compile _ _USE_DRAW_PROCEDURAL
            #pragma multi_compile_fragment _ DEBUG_DISPLAY

            #include "../../../../../PowerShaderLib/URPLib/Fullscreen.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Debug/DebuggingFullscreen.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"

            TEXTURE2D_X(_SourceTex);
            SAMPLER(sampler_SourceTex);
            float _Double;

            half4 Fragment(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                float2 uv = input.uv;

                half4 col = SAMPLE_TEXTURE2D_X(_SourceTex, sampler_SourceTex, uv);

                #ifdef _LINEAR_TO_SRGB_CONVERSION
                    // col = LinearToSRGB(col);
                    col.xyz = pow(col.xyz,lerp(0.4545,0.25,_Double));
                    //col.xyz += float3(0,0.1,0);
                #elif _SRGB_TO_LINEAR_CONVERSION
                    col = SRGBToLinear(col);
                    //col.xyz += float3(0.1,0,0);
                #endif

                #if defined(DEBUG_DISPLAY)
                half4 debugColor = 0;

                if(CanDebugOverrideOutputColor(col, uv, debugColor))
                {
                    return debugColor;
                }
                #endif

                return col;
            }
            ENDHLSL
        }
    }
}
