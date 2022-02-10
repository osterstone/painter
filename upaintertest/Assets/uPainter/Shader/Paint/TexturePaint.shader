Shader "Wing/uPainter/TexturePaint"
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
		_Brush("Brush", 2D) = "white"
		[HideInInspector]
		_BrushRotate("Rotate", FLOAT) = 0
		[HideInInspector]
		_ControlColor("ControlColor", VECTOR) = (0,0,0,0)
		[HideInInspector]
		_PaintUV("Hit UV Position", VECTOR) = (0,0,0,0)
	}
	SubShader
	{
		// No culling or depth
		Cull Off ZWrite Off ZTest Always

		CGINCLUDE

			sampler2D			_MainTex;
			sampler2D			_Brush;
			float4				_Brush_ST;
			float				_BrushRotate;
			float4				_ControlColor;
			float				_Noise;
			float				_NoiseSize;
			uniform float4		_PaintUVs[4];
			uniform float4		_Controls[4];

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
#include "../Lib/uPainterFoundation.cginc"
#include "../Lib/noise.cginc"

			#pragma multi_compile UPAINTER_POINT_MODE UPAINTER_LINE_MODE
			#pragma multi_compile UPAINTER_CAP_FLAT UPAINTER_CAP_ROUND
			#pragma multi_compile __ UPAINTER_CORNER_FLAT UPAINTER_CORNER_ROUND
			#pragma multi_compile __ UPAINTER_ENABLE_BEZIER
			#pragma multi_compile __ UPAINTER_NEQ_WIDTH_LINE

			#pragma vertex vert
			#pragma fragment frag	
			

			fixed4 frag (v2f i) : SV_Target
			{
				float range = UPAINTER_IN_PAINT_RANGE(i.uv, _PaintUVs, 0, _Controls);
				float ratio = pow(range, _Softness);

				float4 brushColor = float4(1, 1, 1, 1);
				float2 uv = UPAINTER_CALC_BRUSH_UV(i.uv, _PaintUVs, _BrushRotate);
				uv = TRANSFORM_TEX(uv, _Brush);
				brushColor = SampleTexture(_Brush, uv);

				float4 col = brushColor * _ControlColor;
				col.a = brushColor.a * _ControlColor.a * ratio;

				if (_Noise > 0)
				{
					bool timeEffect = _BrushStatus <= 1;
					float bias = timeEffect * sin(_Time.x);
					float noiseAlpha = snoise((uv + bias) / _NoiseSize * 10);
					col.a = col.a * (1 - _Noise) + noiseAlpha * col.a * _Noise;
				}
				return col;

			}

			ENDCG
		}
	}
}
