Shader "Wing/uPainter/CompositePaint"
{
	Properties
	{
		[HideInInspector]
		_MainTex("MainTex", 2D) = "white"
		[HideInInspector]
		_RawTex("RawTex", 2D) = "white"
	}
	SubShader
	{
		// No culling or depth
		Cull Off ZWrite Off ZTest Always

		CGINCLUDE
			sampler2D			_MainTex;
			sampler2D			_RawTex;

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
				o.uv = v.uv;
				o.vertex = UnityObjectToClipPos(v.vertex);
				return o;
			}
		ENDCG

		Pass
		{
			CGPROGRAM
			#include "UnityCG.cginc"
			#include "../Lib/uPainterBlend.cginc"
			#include "../Lib/uPainterFoundation.cginc"			
			#pragma multi_compile UPAINTER_LAYER_BLEND_NORMAL UPAINTER_LAYER_BLEND_RESTORE UPAINTER_LAYER_BLEND_REPLACE UPAINTER_LAYER_BLEND_DARKEN UPAINTER_LAYER_BLEND_MUTIPY UPAINTER_LAYER_BLEND_COLORBURN UPAINTER_LAYER_BLEND_LINEARDARK UPAINTER_LAYER_BLEND_LIGHTEN UPAINTER_LAYER_BLEND_COLORSCREEN UPAINTER_LAYER_BLEND_COLORDODGE UPAINTER_LAYER_BLEND_LINEARDODGE UPAINTER_LAYER_BLEND_OVERLAY UPAINTER_LAYER_BLEND_HARDLIGHT UPAINTER_LAYER_BLEND_SOFTLIGHT UPAINTER_LAYER_BLEND_VIVIDLIGHT UPAINTER_LAYER_BLEND_PINLIGHT UPAINTER_LAYER_BLEND_LINEARLIGHT UPAINTER_LAYER_BLEND_HARDMIX UPAINTER_LAYER_BLEND_DIFFERENCE UPAINTER_LAYER_BLEND_EXCLUSION UPAINTER_LAYER_BLEND_SUBTRACT UPAINTER_LAYER_BLEND_ADD UPAINTER_LAYER_BLEND_DISSOLVE

			#pragma vertex vert
			#pragma fragment frag	

			fixed4 frag(v2f i) : SV_Target
			{
				fixed4 col = tex2D(_RawTex, i.uv);
				fixed4 overlay = tex2D(_MainTex, i.uv);

				float4 blend = UPAINTER_LAYER_BLEND(col, overlay, i.uv);
				return BlendByAlpha(col, blend, overlay.a);
			}
			ENDCG
		}
	}
}
