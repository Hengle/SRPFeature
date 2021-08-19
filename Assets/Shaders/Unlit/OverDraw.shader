Shader "SRPFeature/OverDraw"
{
    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent" "RenderPipeline" = "SRPDefaultUnlit" }

        Pass
        {
            HLSLPROGRAM

            #pragma exclude_renderers gles gles3 glcore
            
            #pragma vertex VertDefault
            #pragma fragment FragForward

            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"

            struct Arrtibutes
            {
                float4 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
            };

            CBUFFER_START(UnityPerDraw)
            float4x4 unity_ObjectToWorld;
            float4x4 unity_WorldToObject;
            float4 unity_LODFade;  // x is the fade value ranging within [0,1]. y is x quantized into 16 levels
            real4 unity_WorldTransformParams; // w is usually 1.0, or -1.0 for odd-negative scale transforms
            CBUFFER_END
            
            float4x4 unity_MatrixVP;

            Varyings VertDefault (Arrtibutes input)
            {
                Varyings output;
                output.positionCS = mul(unity_MatrixVP, mul(unity_ObjectToWorld, float4(input.positionOS.xyz, 1.0)));
                return output;
            }

            half4 FragForward (Varyings input) : SV_Target
            {
                return half4(0.1, 0.04, 0.02, 0);
            }
            ENDHLSL
        }
    }
}
