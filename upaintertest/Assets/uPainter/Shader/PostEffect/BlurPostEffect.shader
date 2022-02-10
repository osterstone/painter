Shader "Wing/uPainter/BlurPostEffect"
{
	Properties
	{
		[HideInInspector]
		_BlurColor("BlurColor", Color) = (1,1,1,1)
		[HideInInspector]
		_CheckRange("CheckRange",Range(0,1)) = 0.1
		[HideInInspector]
		_CheckAccuracy("CheckAccuracy",Range(0.1,0.99)) = 0.9
		[HideInInspector]
		_MainTex("MainTex", 2D) = "white"
		[HideInInspector]
		_Overlay("Overlay", 2D) = "white"
		[HideInInspector]
		_LineWidth("LineWidth",Float) = 0.39
	}
	SubShader
	{
		// No culling or depth
		Cull Off ZWrite Off ZTest Always

		CGINCLUDE
			sampler2D			_MainTex;
			float4				_MainTex_TexelSize;

			sampler2D			_Overlay;
			float4				_BlurColor;
			float				_BlurWidth;

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};
			
		ENDCG

		Pass
		{
			CGPROGRAM
#include "UnityCG.cginc"
			#pragma vertex vert
			#pragma fragment frag

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
				float2 p = _MainTex_TexelSize.xy * _BlurWidth;
				float2 uv = i.uv + Rotate(p, angle);
				return tex2D(_Overlay, uv).a;
			}

			fixed4 frag (v2f i) : SV_Target
			{ 
				float4 col = tex2D(_Overlay, i.uv);
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
					
					int stroke = alpha > 0;
					return _BlurColor * stroke;
				}
				
				return col;
			}
			ENDCG
	}

		Pass
		{
			CGPROGRAM
#include "UnityCG.cginc"
#pragma vertex vert
#pragma fragment frag

			uniform float4		_Offsets;
			uniform int			_SharpMode;
			uniform float		_Rotate;
			
			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;

				float4 uv01 : TEXCOORD1;
				float4 uv23 : TEXCOORD2;
				float4 uv45 : TEXCOORD3;
			};

			v2f vert(appdata v)
			{
				v2f o;
				o.uv = v.uv;
				o.vertex = UnityObjectToClipPos(v.vertex);

				_Offsets *= _MainTex_TexelSize.xyxy;

				o.uv01 = v.uv.xyxy + _Offsets.xyxy * float4(1, 1, -1, -1);
				o.uv23 = v.uv.xyxy + _Offsets.xyxy * float4(1, 1, -1, -1) * 2.0;
				o.uv45 = v.uv.xyxy + _Offsets.xyxy * float4(1, 1, -1, -1) * 3.0;

				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				fixed4 color = fixed4(0,0,0,0);

				color += 0.4 * tex2D(_MainTex, i.uv);
				color += 0.15 * tex2D(_MainTex, i.uv01.xy);
				color += 0.15 * tex2D(_MainTex, i.uv01.zw);
				color += 0.10 * tex2D(_MainTex, i.uv23.xy);
				color += 0.10 * tex2D(_MainTex, i.uv23.zw);
				color += 0.05 * tex2D(_MainTex, i.uv45.xy);
				color += 0.05 * tex2D(_MainTex, i.uv45.zw);

				if (_SharpMode == 0)
				{
					return color;
				}
				else
				{
					float4 col = tex2D(_Overlay, i.uv);
					return color * (1 - col.a) + col * col.a;
				}
			}
			ENDCG
		}
	}
}
