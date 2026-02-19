Shader "Custom/WorldSlice"
{
    SubShader
    {
        Tags { "RenderType"="Opaque" }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            float4 _WorldSlicePlane;
            float _SliceThickness;

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 worldPos : TEXCOORD0;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float dist =
                    dot(_WorldSlicePlane.xyz, i.worldPos)
                    + _WorldSlicePlane.w;

                if (abs(dist) > _SliceThickness)
                    discard;

                return fixed4(1,1,1,1);
            }
            ENDCG
        }
    }
}