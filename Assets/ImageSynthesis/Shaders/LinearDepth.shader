Shader "Hidden/LinearDepth"
{
	SubShader
	{
		// No culling or depth
		Cull Off ZWrite Off ZTest Always

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

			float4 _CameraDepthTexture_ST;
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _CameraDepthTexture);
				return o;
			}

			sampler2D _CameraDepthTexture;
			fixed4 frag (v2f i) : SV_Target
			{
				float d = UNITY_SAMPLE_DEPTH(tex2D(_CameraDepthTexture, i.uv));
				float linearZ = Linear01Depth(d);
				
				float lowBits = frac(linearZ * 256);
				float highBits = linearZ - lowBits / 256;
				return float4(lowBits, highBits, d, 1);
			}
			ENDCG
		}
	}
}
