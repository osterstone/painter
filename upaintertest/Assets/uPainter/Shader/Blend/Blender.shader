Shader "Wing/uPainter/Blender"
{
	Properties
	{
		_MainTex("MainTex", 2D) = "white" {}
		_RawTex("RawTex", 2D) = "white" {}
		_OverlayTex ("OverlayTex", 2D) = "white" {}
		_UVRemapTex("UVRemapTex", 2D) = "white" {}
	}
	
	SubShader
	{
		// No culling or depth
		Cull Off ZWrite Off ZTest Always

		CGINCLUDE
			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}

			sampler2D _MainTex;
			sampler2D _RawTex;
			sampler2D _OverlayTex;
#ifdef UPAINTER_ENABLE_REMAP_UV
			sampler2D _UVRemapTex;
#endif
			uniform int _BlendType;
		ENDCG

		Pass
		{
			CGPROGRAM

#include "../Lib/uPainterBlend.cginc"
#pragma multi_compile UPAINTER_LAYER_BLEND_NORMAL UPAINTER_LAYER_BLEND_RESTORE UPAINTER_LAYER_BLEND_REPLACE UPAINTER_LAYER_BLEND_DARKEN UPAINTER_LAYER_BLEND_MUTIPY UPAINTER_LAYER_BLEND_COLORBURN UPAINTER_LAYER_BLEND_LINEARDARK UPAINTER_LAYER_BLEND_LIGHTEN UPAINTER_LAYER_BLEND_COLORSCREEN UPAINTER_LAYER_BLEND_COLORDODGE UPAINTER_LAYER_BLEND_LINEARDODGE UPAINTER_LAYER_BLEND_OVERLAY UPAINTER_LAYER_BLEND_HARDLIGHT UPAINTER_LAYER_BLEND_SOFTLIGHT UPAINTER_LAYER_BLEND_VIVIDLIGHT UPAINTER_LAYER_BLEND_PINLIGHT UPAINTER_LAYER_BLEND_LINEARLIGHT UPAINTER_LAYER_BLEND_HARDMIX UPAINTER_LAYER_BLEND_DIFFERENCE UPAINTER_LAYER_BLEND_EXCLUSION UPAINTER_LAYER_BLEND_SUBTRACT UPAINTER_LAYER_BLEND_ADD UPAINTER_LAYER_BLEND_DISSOLVE
#pragma multi_compile __ UPAINTER_ENABLE_REMAP_UV
			#pragma vertex vert
			#pragma fragment frag	
						
			fixed4 frag (v2f i) : SV_Target
			{
				float ratio = 1;
				float2 remapUV = i.uv;
				float2 rawUV = i.uv;

#ifdef UPAINTER_ENABLE_REMAP_UV
				float4 uvCol = tex2D(_UVRemapTex, i.uv);
				bool blend = true; // uvCol.w == 1;
				remapUV = uvCol.xy;
#endif

				fixed4 col = tex2D(_MainTex, rawUV);
				fixed4 overlay = tex2D(_OverlayTex, remapUV);

#ifdef UPAINTER_LAYER_BLEND_RESTORE
				fixed4 raw = tex2D(_RawTex, rawUV);
				col = BlendByAlpha(col, raw, overlay.a);
#endif
				if (_BlendType == 1)
				{
#ifdef UPAINTER_LAYER_BLEND_REPLACE
					return BlendReplaceEx(col, overlay) * ratio;
#else
					return UPAINTER_LAYER_BLEND(col, overlay, rawUV) * ratio;
#endif
				}
				else if (_BlendType == 2)
				{
					return max(col, overlay) * ratio;
				}
				else
				{
#ifdef UPAINTER_ENABLE_REMAP_UV
					if (blend)
#endif
					{
						float4 blend = UPAINTER_LAYER_BLEND(col, overlay, rawUV);
						blend = BlendByAlpha(col, blend, overlay.a);
						blend.a = 1;
						return blend * ratio;
					}
					return col;
				}
			}
			ENDCG
		}
	}
}
