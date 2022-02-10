Shader "Wing/uPainter/SolidPaint"
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

				/*float2 uvRemapped = v.uv.xy * 2 - 1;
				o.vertex = float4(uvRemapped.x, uvRemapped.y, 0, 1);*/

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

				float drawPos(float2 uv, float2 pos)
				{
					return distance(uv, pos) < 0.05;
				}

				float modv(float x, float y)
				{
					return x - y * floor(x / y);
				}	

				fixed4 frag(v2f i) : SV_Target
				{
					float range = UPAINTER_IN_PAINT_RANGE(i.uv, _PaintUVs, 0, _Controls);
					float4 col = SampleTexture(_MainTex, i.uv);
					float2 uv = UPAINTER_CALC_BRUSH_UV(i.uv, _PaintUVs, 0);
					if (_EnableOverlay == 1)
					{
						float ratio = pow(range, _Softness);

						float4 col = _ControlColor;
						col.a *= ratio;

						if (_Noise > 0)
						{
							bool timeEffect = _BrushStatus <= 1;
							float bias = timeEffect * sin(_Time.x);
							float noiseAlpha = snoise((uv + bias) / _NoiseSize * 10);
							col.a = col.a * (1 - _Noise) + noiseAlpha * col.a * _Noise;
						}

						return col;
					}
					else
					{
						return max(col, float4(range, uv.x, uv.y, range));
					}
				}
			ENDCG
		}
	}
}
