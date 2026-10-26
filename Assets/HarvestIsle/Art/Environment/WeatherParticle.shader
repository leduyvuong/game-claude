Shader "Harvest Isle/Weather Particle"
{
    Properties { _BaseColor("Color",Color)=(1,1,1,1) _Shape("Shape: streak/ring/snow/cloud/solid/leaf",Float)=0 }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Transparent" "RenderType"="Transparent" }
        Pass
        {
            Tags { "LightMode"="UniversalForward" }
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                float _Shape;
            CBUFFER_END
            struct Attributes { float4 positionOS:POSITION; float2 uv:TEXCOORD0; half4 color:COLOR; };
            struct Varyings { float4 positionCS:SV_POSITION; float2 uv:TEXCOORD0; half4 color:COLOR; };
            Varyings Vert(Attributes input)
            { Varyings o;o.positionCS=TransformObjectToHClip(input.positionOS.xyz);o.uv=input.uv;o.color=input.color;return o; }
            half4 Frag(Varyings input):SV_Target
            {
                float2 p=input.uv-.5;float r=length(p)*2;float alpha;
                if(_Shape<.5)alpha=(1-smoothstep(.12,.48,abs(p.x)))*sin(saturate(input.uv.y)*3.14159);
                else if(_Shape<1.5)alpha=(1-smoothstep(.06,.16,abs(r-.73)))*(1-smoothstep(.9,1,r));
                else if(_Shape<2.5)alpha=1-smoothstep(.30,.90,r);
                else if(_Shape<3.5)alpha=pow(saturate(1-r*r),1.7);
                else if(_Shape<4.5)alpha=1;
                else alpha=1-smoothstep(.23,.40,abs(p.x)+abs(p.y)*.6);
                return half4(_BaseColor.rgb*input.color.rgb,_BaseColor.a*input.color.a*alpha);
            }
            ENDHLSL
        }
    }
    Fallback Off
}
