Shader "Wing/uPainter/GraphicPaint"
{
	Properties
	{
		[HideInInspector]
		_Noise("Noise", Range(0.0, 1.0)) = 0.0
		[HideInInspector]
		_NoiseSize("NoiseSize", Range(0.01,10)) = 1
		[HideInInspector]
		_MainTex("MainTex", 2D) = "white"
		[HideInInspector]
		_ControlColor("ControlColor", VECTOR) = (0,0,0,0)
	}
	SubShader
	{
		// No culling or depth
		Cull Off ZWrite Off ZTest Always

		CGINCLUDE
			sampler2D			_MainTex;
			float4				_ControlColor;
			float				_Noise;
			float				_NoiseSize;
			uniform float4		_PaintUVs[3];

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

			#pragma vertex vert
			#pragma fragment frag
			
			fixed4 frag (v2f i) : SV_Target
			{ 
				return  _ControlColor;
			}

			ENDCG
		}
	}
}
