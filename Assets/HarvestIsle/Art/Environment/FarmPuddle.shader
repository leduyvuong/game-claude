Shader "Harvest Isle/Farm Puddle"
{
    Properties
    {
        _Rain ("Rain", Range(0,2)) = 0
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Transparent" "RenderType"="Transparent" }
        Pass
        {
            Name "Puddle"
            Tags { "LightMode"="UniversalForward" }
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Back
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            CBUFFER_START(UnityPerMaterial)
                float _Rain;
            CBUFFER_END
            struct Attributes { float4 positionOS:POSITION; float4 color:COLOR; float2 uv:TEXCOORD0; };
            struct Varyings { float4 positionCS:SV_POSITION; float3 world:TEXCOORD0; half4 color:COLOR; float2 uv:TEXCOORD1; };
            Varyings Vert(Attributes input)
            {
                Varyings output;
                output.world=TransformObjectToWorld(input.positionOS.xyz);
                output.positionCS=TransformWorldToHClip(output.world);
                output.color=input.color;
                output.uv=input.uv;
                return output;
            }
            half4 Frag(Varyings input):SV_Target
            {
                float2 p=input.world.xz;
                float t=_Time.y;
                float drift=sin(p.x*8+p.y*5+t*.55)*.5+.5;
                half3 water=lerp(half3(.09,.17,.14),half3(.21,.34,.29),drift*.42+input.color.r*.35);
                // A narrow, broken sky reflection, not an ocean-style foam rim.
                float streak=abs(sin(p.x*18+p.y*7+sin(p.y*21+t*.5)*.22));
                float shine=(1-smoothstep(.015,.065,streak))*smoothstep(.3,.85,sin(p.y*33-p.x*7)*.5+.5);
                water+=half3(.24,.27,.21)*shine*.35;
                float2 cell=floor(p*5);
                float seed=frac(sin(dot(cell,float2(127.1,311.7)))*43758.5453);
                float phase=frac(t*.75+seed);
                float2 drop=frac(p*5)-float2(.28+seed*.4,.65-seed*.3);
                float ripple=(1-smoothstep(.012,.032,abs(length(drop)-phase*.48)))*(1-phase);
                water+=half3(.20,.26,.23)*ripple*saturate(_Rain)*.48*step(.55,seed);
                // Vertex alpha feathers the shore into the wet soil; peaks occlude the flat water.
                // Max opacity stays below .65 so ridges and young crops still read through standing water.
                return half4(water,smoothstep(0,1,input.color.a)*.62);
            }
            ENDHLSL
        }
    }
}
