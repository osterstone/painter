Shader "Wing/uPainter/OutlinePostEffect"
{
	Properties
	{
		[HideInInspector]
		_OutlineColor("OutlineColor", Color) = (1,1,1,1)
		[HideInInspector]
		_OutlineWidth("OutlineWidth",Range(0,1)) = 0.01
		[HideInInspector]
		_MainTex("MainTex", 2D) = "white"
	}
	SubShader
	{
		// No culling or depth
		Cull Off ZWrite Off ZTest Always

		CGINCLUDE
			sampler2D			_MainTex;
			float4				_MainTex_TexelSize;
			float4				_OutlineColor;
			float				_OutlineWidth;


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

			float2 Rotate(float2 p, float rotate) {
				float rad = radians(rotate);
				float cosr = 0, sinr = 0;
				sincos(rad, sinr, cosr);
				float x = p.x * cosr - p.y * sinr;
				float y = p.x * sinr + p.y * cosr;
				return float2(x, y);
			}

			float getAlpha(v2f i, float angle)
			{
				float2 p = _MainTex_TexelSize.xy * _OutlineWidth;
				float2 uv = i.uv + Rotate(p, angle);
				return tex2D(_MainTex, uv).a;
			}

			fixed4 frag (v2f i) : SV_Target
			{ 
				float4 col = tex2D(_MainTex, i.uv);
				if (col.a == 0)
				{
					float alpha = 0;
					alpha += getAlpha(i, 0);
					alpha += getAlpha(i, 30);
					alpha += getAlpha(i, 60);
					alpha += getAlpha(i, 90);
					alpha += getAlpha(i, 120);
					alpha += getAlpha(i, 150);
					alpha += getAlpha(i, 180);
					alpha += getAlpha(i, 210);
					alpha += getAlpha(i, 240);
					alpha += getAlpha(i, 270);
					alpha += getAlpha(i, 300);
					alpha += getAlpha(i, 330);
					//alpha /= 12;
					
					return _OutlineColor * (alpha > 0);
				}
				
				return col;
			}
			ENDCG
		}
	}
}
