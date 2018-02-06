Shader "Hidden/FadeEffect"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_bwBlend ("Black & White blend", Range (0, 1)) = 0
	}
	SubShader
	{
		Tags { "Queue"="Overlay" "RenderType"="Transparent"}
        LOD 2000
		// No culling or depth
		Cull Off ZWrite Off ZTest Always
		Blend SrcAlpha OneMinusSrcAlpha

		Pass
		{
			CGPROGRAM
			//#pragma vertex vert
			#pragma vertex vert_img
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

//			v2f vert (appdata v)
//			{
//				v2f o;
//				o.vertex = UnityObjectToClipPos(v.vertex);
//				o.uv = v.uv;
//				return o;
//			}
			
			sampler2D _MainTex;
			uniform float _bwBlend;

			fixed4 frag (v2f i) : COLOR// SV_Target
			{
				fixed4 col = float4(0,0,0,_bwBlend);//tex2D(_MainTex, i.uv);
				// just invert the colors
				//col = 1 - col;
				return col;
			}
			ENDCG
		}
	}
}
