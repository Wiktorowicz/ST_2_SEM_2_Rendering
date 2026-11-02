Shader "Custom/FresnelShaderHLSL"
{
    Properties
    {
        _GlowColor ("Kolor poswiaty", Color) = (0, 0.9, 1, 1)
        _Power ("Szerokosc krawedzi", Range(0.5, 8)) = 3
        _Intensity ("Intensywnosc", Range(0, 5)) = 2
        _BaseColor ("Kolor srodka", Color) = (0.02, 0.05, 0.08, 1)
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "RenderType" = "Opaque"
            "Queue" = "Geometry"
        }

        Pass
        {
            Name "FresnelPass"
            Tags { "LightMode" = "SRPDefaultUnlit" }

            Cull Back
            ZWrite On
            ZTest LEqual

            HLSLPROGRAM

            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _GlowColor;
                float _Power;
                float _Intensity;
                float4 _BaseColor;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 normalWS : TEXCOORD0;
                float3 viewDirWS : TEXCOORD1;
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;

                VertexPositionInputs positionInputs = GetVertexPositionInputs(input.positionOS.xyz);

                output.positionHCS = positionInputs.positionCS;
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.viewDirWS = GetWorldSpaceViewDir(positionInputs.positionWS);

                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                float3 normal = normalize(input.normalWS);
                float3 viewDir = normalize(input.viewDirWS);

                float fresnel = 1.0 - saturate(dot(normal, viewDir));
                fresnel = pow(fresnel, _Power);

                float3 glow = _GlowColor.rgb * fresnel * _Intensity;
                float3 baseColor = _BaseColor.rgb;

                float3 finalColor = baseColor + glow;

                return half4(finalColor, 1);
            }

            ENDHLSL
        }
    }
}
