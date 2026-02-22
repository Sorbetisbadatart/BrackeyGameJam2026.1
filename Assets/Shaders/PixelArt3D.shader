Shader "Universal Render Pipeline/PixelArt3D"
{
    Properties
    {
        _MainTex("Main Texture", 2D) = "white" {}
        _Tint("Tint", Color) = (1,1,1,1)
        _TexelScale("Texel Scale", Range(1,8)) = 1
        _ColorSteps("Color Steps", Range(2,256)) = 8
        _UseLighting("Use Lighting", Float) = 1
        _LightSteps("Light Steps", Range(1,8)) = 4
        _MinLight("Min Light", Range(0,1)) = 0.2
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" "RenderPipeline"="UniversalPipeline" }
        Pass
        {
            Name "ForwardUnlit"
            Tags { "LightMode"="UniversalForward" }
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            float4 _MainTex_TexelSize;
            float4 _Tint;
            float _TexelScale;
            float _ColorSteps;
            float _UseLighting;
            float _LightSteps;
            float _MinLight;

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float3 positionWS : TEXCOORD2;
            };

            Varyings vert(Attributes v)
            {
                Varyings o;
                float3 positionWS = TransformObjectToWorld(v.positionOS.xyz);
                o.positionHCS = TransformWorldToHClip(positionWS);
                o.uv = v.uv;
                o.normalWS = normalize(TransformObjectToWorldNormal(v.normalOS));
                o.positionWS = positionWS;
                return o;
            }

            float3 applyLighting(float3 color, float3 normalWS)
            {
                Light l = GetMainLight();
                float ndotl = saturate(dot(normalize(normalWS), l.direction));
                float steps = max(1.0, _LightSteps);
                ndotl = floor(ndotl * steps) / steps;
                ndotl = max(_MinLight, ndotl);
                return color * ndotl;
            }

            float4 frag(Varyings i) : SV_Target
            {
                float2 count = _MainTex_TexelSize.zw / max(1.0, _TexelScale);
                float2 uvq = floor(i.uv * count) / count;
                float4 c = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uvq) * _Tint;
                if (_UseLighting > 0.5)
                {
                    c.rgb = applyLighting(c.rgb, i.normalWS);
                }
                float steps = max(2.0, _ColorSteps);
                c.rgb = floor(c.rgb * steps) / steps;
                return c;
            }
            ENDHLSL
        }
    }
}
