Shader "Hidden/LinearDepth"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	}
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

			float4 _CameraDepthNormalsTexture_ST;
			float4 _CameraDepthTexture_ST;
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				//o.uv = v.uv;
				//o.uv = TRANSFORM_TEX(v.uv, _CameraDepthNormalsTexture);
				o.uv = TRANSFORM_TEX(v.uv, _CameraDepthTexture);
				return o;
			}
			
			sampler2D _MainTex;
			//sampler2D_float _CameraDepthTexture;
			sampler2D _CameraDepthTexture;
			sampler2D _CameraDepthNormalsTexture;
            //float4 _CameraDepthTexture_ST;

            float3 Hue(float H)
			{
			    float R = abs(H * 6 - 3) - 1;
			    float G = 2 - abs(H * 6 - 2);
			    float B = 2 - abs(H * 6 - 4);
			    return saturate(float3(R,G,B));
			}

			float4 HSVtoRGB(in float3 HSV)
			{
			    return float4(((Hue(HSV.x) - 1) * HSV.y + 1) * HSV.z,1);
			}

			fixed4 frag (v2f i) : SV_Target
			{
				//float depth = tex2D (_CameraDepthTexture, 1-i.uv);\
				// read depth
				//float4 depthnormal = tex2D (_CameraDepthNormalsTexture, i.uv);
				//return depthnormal;

				float d = UNITY_SAMPLE_DEPTH(tex2D(_CameraDepthTexture, i.uv));
                //d = 1.0 / Linear01Depth(d);
				return fixed4(d, d, d, 1);
			}
			ENDCG
		}
	}
}
