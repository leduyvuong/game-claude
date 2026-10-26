Shader "Harvest Isle/Stylized Water"
{
    Properties
    {
        _BaseColor ("Water color", Color) = (0.35,0.70,0.69,1)
        _FoamColor ("Shore foam", Color) = (0.82,0.94,0.87,1)
        _WaveStrength ("Ripple strength", Range(0,1)) = 0.32
        _Speed ("Ripple speed", Range(0,2)) = 0.45
        _PreviewTime ("Preview time (-1 uses game time)", Float) = -1
        _Opacity ("Water opacity", Range(0,1)) = 1
        [HideInInspector] _ZWrite ("Depth write", Float) = 1
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" "Queue"="Geometry" }
        Pass
        {
            Name "Water"
            Tags { "LightMode"="UniversalForward" }
            Cull Back
            ZWrite [_ZWrite]
            Blend SrcAlpha OneMinusSrcAlpha
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma target 2.0
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                half4 _FoamColor;
                float _WaveStrength;
                float _Speed;
                float _PreviewTime;
                float _Opacity;
            CBUFFER_END
            struct Attributes { float4 positionOS : POSITION; half4 color : COLOR; };
            struct Varyings { float4 positionCS : SV_POSITION; float3 positionWS : TEXCOORD0; half4 color : COLOR; };
            Varyings Vert(Attributes input)
            {
                Varyings output;
                output.positionWS = TransformObjectToWorld(input.positionOS.xyz);
                output.positionCS = TransformWorldToHClip(output.positionWS);
                output.color = input.color;
                return output;
            }
            half4 Frag(Varyings input) : SV_Target
            {
                float time = (_PreviewTime >= 0 ? _PreviewTime : _Time.y) * _Speed;
                float2 p = input.positionWS.xz;
                float broad = sin(dot(p, float2(0.83,0.57)) - time)
                    + sin(dot(p, float2(-0.46,1.17)) + time * 0.77);
                float wave = sin(dot(p, float2(3.4,1.8)) + broad * 0.8 - time * 1.8);
                float breakUp = sin(dot(p, float2(-1.7,2.3)) + time * 0.6);
                float glint = smoothstep(0.84,0.99,wave) * smoothstep(-0.1,0.8,breakUp);
                half3 water = _BaseColor.rgb * input.color.rgb * (0.98 + broad * 0.025 * _WaveStrength);
                water = lerp(water, _FoamColor.rgb, glint * _WaveStrength * 0.22);
                float shore = input.color.a * (0.30 + 0.22 * sin(broad + time));
                water = lerp(water, _FoamColor.rgb, saturate(shore));
                return half4(water,_Opacity);
            }
            ENDHLSL
        }
    }
    Fallback Off
}
