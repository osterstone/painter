Shader "Wing/uPainter/BlendOverlay"
{
	Properties
	{
		_MainTex("MainTex", 2D) = "white" {}
		_OverlayTex ("OverlayTex", 2D) = "white" {}
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
			sampler2D _OverlayTex;
		ENDCG

		Pass
		{
			CGPROGRAM

#include "../Lib/uPainterBlend.cginc"

			#pragma vertex vert
			#pragma fragment frag	
						
			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col = tex2D(_MainTex, i.uv);
				fixed4 overlay = tex2D(_OverlayTex, i.uv);
				return max(col, overlay);
			}
			ENDCG
		}
	}
}
