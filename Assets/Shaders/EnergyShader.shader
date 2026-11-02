Shader "Custom/EnergyShader"
{
    Properties
    {
        _ColorA ("Kolor podstawowy", Color) = (0, 0.8, 1, 1)
        _ColorB ("Kolor dodatkowy", Color) = (0.1, 0.1, 1, 1)
        _AnimationSpeed ("Szybkosc animacji", Range(0, 5)) = 1
        _PatternScale ("Skala wzoru", Range(1, 20)) = 8
        _EmissionStrength ("Intensywnosc emisji", Range(0, 5)) = 2
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
            Name "EnergyPass"
            Tags { "LightMode" = "SRPDefaultUnlit" }

            Cull Back
            ZWrite On
            ZTest LEqual

            HLSLPROGRAM

            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _ColorA;
                float4 _ColorB;
                float _AnimationSpeed;
                float _PatternScale;
                float _EmissionStrength;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;

                VertexPositionInputs positionInputs = GetVertexPositionInputs(input.positionOS.xyz);

                output.positionHCS = positionInputs.positionCS;
                output.positionWS = positionInputs.positionWS;

                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                float3 position = input.positionWS;

                float time = _Time.y * _AnimationSpeed;

                float waveA = sin(position.x * _PatternScale + time);
                float waveB = sin(position.y * _PatternScale - time * 1.3);
                float waveC = sin(position.z * _PatternScale + time * 0.7);

                float pattern = (waveA + waveB + waveC) / 3.0;
                pattern = pattern * 0.5 + 0.5;

                float3 energyColor = lerp(_ColorA.rgb, _ColorB.rgb, pattern);

                float3 finalColor = energyColor * _EmissionStrength;

                return half4(finalColor, 1);
            }

            ENDHLSL
        }
    }
}
