Shader "Hidden/OpticalFlow"
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

			float4 _CameraMotionVectorsTexture_ST;
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				//o.uv = v.uv;
				//o.uv = TRANSFORM_TEX(v.uv, _CameraDepthNormalsTexture);
				o.uv = TRANSFORM_TEX(v.uv, _CameraMotionVectorsTexture);
				return o;
			}
			
			sampler2D _MainTex;
			//sampler2D_float _CameraDepthTexture;
			sampler2D _CameraDepthTexture;
			sampler2D _CameraDepthNormalsTexture;
			sampler2D _CameraMotionVectorsTexture;
            //float4 _CameraDepthTexture_ST;

            float3 Hue(float H)
			{
			    float R = abs(H * 6 - 3) - 1;
			    float G = 2 - abs(H * 6 - 2);
			    float B = 2 - abs(H * 6 - 4);
			    return saturate(float3(R,G,B));
			}

			float3 HSVtoRGB(float3 HSV)
			{
			    return float3(((Hue(HSV.x) - 1) * HSV.y + 1) * HSV.z);
			}

			float3 MotionVectorsToOpticalFlow(float2 motion)
			{
				// gamma color conv
				// lut for  https://www.microsoft.com/en-us/research/wp-content/uploads/2007/10/ofdatabase_iccv_07.pdf
				//  		https://people.csail.mit.edu/celiu/SIFTflow/
				//     and code https://github.com/suhangpro/epicflow/blob/master/utils/flow-code-matlab/computeColor.m

				// currently:
				//			"Optical Flow in a Smart Sensor Based on Hybrid Analog-Digital Architecture" by P. Guzman et al
				//			http://www.mdpi.com/1424-8220/10/4/2975
				// analogous to http://docs.opencv.org/trunk/d7/d8b/tutorial_py_lucas_kanade.html, but might need to swap or rotate axis


				float angle = atan2(-motion.y, -motion.x);
				float hue = angle / (UNITY_PI * 2.0) + 0.5;
				float value = saturate(length(motion)*10);
    			return HSVtoRGB(float3(hue, 1, value));
			}

			fixed4 frag (v2f i) : SV_Target
			{
				//float depth = tex2D (_CameraDepthTexture, 1-i.uv);\
				// read depth
				//float4 depthnormal = tex2D (_CameraDepthNormalsTexture, i.uv);
				//return depthnormal;

				float2 mv = tex2D(_CameraMotionVectorsTexture, i.uv).rg;
				return float4(MotionVectorsToOpticalFlow(mv), 1);
				mv = (mv + 1.0) / 2.0;
				return fixed4(mv.r, mv.g, 0, 1);
			}
			ENDCG
		}
	}
}
