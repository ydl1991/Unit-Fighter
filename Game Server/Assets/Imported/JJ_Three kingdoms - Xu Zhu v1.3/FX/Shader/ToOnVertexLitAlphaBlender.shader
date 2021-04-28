Shader "ToOn/VertexLitAlphaBlend" 
{
	Properties 
	{
		_Color ("Main Color", Color) = (1,1,1,1)
		_SpecColor ("Spec Color", Color) = (1,1,1,1)
		_Emission ("Emissive Color", Color) = (0,0,0,0)
		_Shininess ("Shininess", Range (0.01, 1)) = 0.7
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_Alpha ("Alpha Value", Range(0.0, 1.0)) = 1.0
	}

	SubShader 
	{
		Tags { "Queue"="Transparent" "RenderType"="Transparent" }
		LOD 100
	
		// Non-lightmapped
		Pass 
		{
			Tags { "LightMode" = "Vertex" }
			Blend SrcAlpha OneMinusSrcAlpha

			Material 
			{
				Diffuse [_Color] 
				Ambient [_Color]
				Shininess [_Shininess]
				Specular [_SpecColor]
				Emission [_Emission]
			} 

			Lighting On
			SeparateSpecular On
			SetTexture [_MainTex] 
			{
				constantColor(1,1,1, [_Alpha])
				Combine texture * primary DOUBLE, constant
			} 
		}
		
		// Pass to render object as a shadow caster
		Pass 
		{
			Name "ShadowCaster"
			Tags { "LightMode" = "ShadowCaster" }
		
			Fog {Mode Off}
			ZWrite On ZTest LEqual Cull Off
			Offset 1, 1

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_shadowcaster
			#include "UnityCG.cginc"

			struct v2f 
			{ 
				V2F_SHADOW_CASTER;
			};

			v2f vert( appdata_base v )
			{
				v2f o;
				TRANSFER_SHADOW_CASTER(o)
				return o;
			}

			float4 frag( v2f i ) : SV_Target
			{
				SHADOW_CASTER_FRAGMENT(i)
			}
		ENDCG
		}
	
		// Pass to render object as a shadow collector
		Pass 
		{
			Name "ShadowCollector"
			Tags { "LightMode" = "ShadowCollector" }
		
			Fog {Mode Off}
			ZWrite On ZTest LEqual

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_shadowcollector 

			#define SHADOW_COLLECTOR_PASS
			#include "UnityCG.cginc"

			struct appdata 
			{
				float4 vertex : POSITION;
			};

			struct v2f 
			{
				V2F_SHADOW_COLLECTOR;
			};

			v2f vert (appdata v)
			{
				v2f o;
				TRANSFER_SHADOW_COLLECTOR(o)
				return o;
			}

			fixed4 frag (v2f i) : SV_Target
			{
				SHADOW_COLLECTOR_FRAGMENT(i)
			}
			ENDCG
		}
	}
}
