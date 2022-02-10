Shader "Wing/uPainter/RenderUV"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100
		/*ZTest Off
		ZWrite Off
		Cull Off*/

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

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

            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);

				/*float2 uvRemapped = v.uv.xy * 2 - 1;
				o.vertex = float4(uvRemapped.xy, 0, 1);*/

                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

			float4 frag (v2f i) : SV_Target
            {
				return float4(i.uv.xy, 0, 1);
            }
            ENDCG
        }
    }
}
