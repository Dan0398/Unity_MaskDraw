Shader "Drawable/World Coord Simple" 
{
	Properties 
	{
		_Mask ("Mask", 2D) = "black" {}
		_CTex ("Black Texture", 2D) = "black" {}
		_RTex ("Red Texture", 2D) = "black" {}
		_GTex ("Green Texture", 2D) = "black" {}
		_BTex ("Blue Texture", 2D) = "black" {}
		_BaseScale ("Base Tiling", Vector) = (1,1,1,0)
		
	}

	SubShader 
	{
		Tags {"RenderType"="Opaque"}

        Pass 
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fwdbase_fullshadows
			#include "UnityCG.cginc"
			#include "AutoLight.cginc"
            #include "UnityLightingCommon.cginc"

			sampler2D _Mask;
			sampler2D _CTex;
			sampler2D _RTex;
			sampler2D _GTex;
			sampler2D _BTex;

			fixed3 _BaseScale;

			struct appdata
			{
				float4 pos : POSITION;
				float4 normal : NORMAL;
				float2 uv : TEXCOORD0;
				float2 uv1 : TEXCOORD1;
			};

			struct v2f
			{
				float4 pos : SV_POSITION;
				float4 uv : TEXCOORD0;
				float3 ambient: COLOR0;
				float3 lightCol: COLOR1;
				float3 worldPos: TEXCOORD1;
				float3 worldNormal : TEXCOORD2;
				SHADOW_COORDS(3)
			};
			
			v2f vert (appdata v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.pos);
				o.worldNormal = UnityObjectToWorldNormal(v.normal);
				o.worldPos = mul(unity_ObjectToWorld, v.pos).xyz;
				o.uv.xy = v.uv;
				o.uv.zw = v.uv1 * unity_LightmapST.xy + unity_LightmapST.zw;
				half nl = max(0, dot(o.worldNormal, _WorldSpaceLightPos0.xyz));
				o.lightCol = nl * _LightColor0.rgb;
				o.ambient = ShadeSH9(half4(o.worldNormal,1));
				TRANSFER_SHADOW(o)
				return o;
			}

			fixed4 frag (v2f i) : SV_Target
			{
				fixed2 texXY = i.worldPos.xy * _BaseScale.z;
				fixed2 texXZ = i.worldPos.xz * _BaseScale.y;
				fixed2 texYZ = i.worldPos.yz * _BaseScale.x;
				fixed3 mask = fixed3(
					dot (i.worldNormal, fixed3(0,0,1)),
					dot (i.worldNormal, fixed3(0,1,0)),
					dot (i.worldNormal, fixed3(1,0,0)));
				fixed2 texUV = 
					texXY * abs(mask.x) +
					texXZ * abs(mask.y) +
					texYZ * abs(mask.z);
				fixed4 CTex = tex2D(_CTex, texUV);
				fixed4 RTex = tex2D(_RTex, texUV);
				fixed4 GTex = tex2D(_GTex, texUV);
				fixed4 BTex = tex2D(_BTex, texUV);
				fixed4 MaskImage = tex2D(_Mask, i.uv.xy);
				fixed FullVolume = (MaskImage.r + MaskImage.g + MaskImage.b);
				fixed4 Result = fixed4(fixed3(CTex.rgb * (1-FullVolume)
											+ RTex.rgb * MaskImage.r
											+ GTex.rgb * MaskImage.g
											+ BTex.rgb * MaskImage.b), 1);
											
				//Shadows
                fixed3 lighting = i.lightCol * SHADOW_ATTENUATION(i) + i.ambient*1.5;
				half4 bakedColorTex = UNITY_SAMPLE_TEX2D(unity_Lightmap, i.uv.zw);
				fixed3 decoded = DecodeLightmap(bakedColorTex);
				Result.rgb *= (lighting + decoded.rgb);
				
				return Result;
			}
			ENDCG
		}
		
		Pass 
		{
			cull off
			Tags {"LightMode" = "ShadowCaster"}
			
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag

			struct VertexData {
				float4 vertex : POSITION;
			};
			
			struct v2f
			{
				float4 vertex : POSITION;
			};
			
			v2f vert (VertexData v){
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				return o;
			}

			half4 frag(v2f IN) : SV_Target
			{
				return 0;
			}

			ENDCG
		}
	}
}