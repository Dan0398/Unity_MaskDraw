Shader "Drawable/Utils/UV Projector"
{
    Properties
    { }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Cull off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal: NORMAL;
                float2 uv :TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv :TEXCOORD0;
                float2 proj: TEXCOORD1;
                float3 normal: COLOR0;
                float3 worldPos : COLOR1;
            };
            
            float Size;
            float4 DrawDirection;
            float3 DrawWorldPos;
            float MaxDistance;
            
            v2f vert (appdata v)
            {
                v2f o;
                o.uv = v.uv;
                float4 uv = float4(0, 0, 0, 1);
                uv.xy = float2(1, _ProjectionParams.x) * (v.uv.xy * 2 - 1);
				o.vertex = uv; 
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.proj = UnityObjectToClipPos(v.vertex).xy /2+0.5f;
                o.normal = mul(unity_ObjectToWorld, v.normal);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                /*
                if (dot(DrawDirection, i.normal) < 0)
                {
                }
                */
                fixed4 Result = fixed4(0,0,0,0);
                if (distance(DrawWorldPos, i.worldPos) < MaxDistance)
                {
                    if (i.proj.x == frac(i.proj.x) && i.proj.y == frac(i.proj.y)) 
                    {
                        Result = fixed4(i.proj.xy,0,1);
                    }
                }
                return Result;
            }
            ENDCG
        }
    }
}
