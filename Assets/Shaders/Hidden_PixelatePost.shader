Shader "Hidden/URP/PixelatePost"
{
    Properties
    {
        _BlitTexture("Blit Texture", 2D) = "white" {}
        _TargetSize("Target Size", Vector) = (320,180,0,0)
        _UsePosterize("Use Posterize", Float) = 1
        _ColorSteps("Color Steps", Range(2,256)) = 16
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Overlay" "RenderPipeline"="UniversalPipeline" }
        Pass
        {
            Name "Pixelate"
            Tags { "LightMode"="UniversalForward" }
            ZTest Always ZWrite Off Cull Off
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnitySampling.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnitySpatialSampling.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Fullscreen.hlsl"

            TEXTURE2D(_BlitTexture);
            SAMPLER(sampler_BlitTexture);

            float2 _TargetSize;
            float _UsePosterize;
            float _ColorSteps;

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            Varyings Vert(uint id : SV_VertexID)
            {
                Varyings o;
                o.positionCS = GetFullScreenTriangleVertexPosition(id);
                o.uv = GetFullScreenTriangleTexCoord(id);
                return o;
            }

            float4 Frag(Varyings i) : SV_Target
            {
                float2 ts = max(_TargetSize, float2(1.0,1.0));
                float2 uvq = floor(i.uv * ts) / ts;
                float4 c = SAMPLE_TEXTURE2D(_BlitTexture, sampler_BlitTexture, uvq);
                if (_UsePosterize > 0.5)
                {
                    float steps = max(_ColorSteps, 2.0);
                    c.rgb = floor(c.rgb * steps) / steps;
                }
                return c;
            }
            ENDHLSL
        }
    }
    FallBack Off
}
