Shader "Unlit/Distance Field Shader"
{
	Properties
	{
        _MainTex ("Texture", 2D) = "white" {}
        _DarknessColor ("Darkness Color", Color) = (0, 0, 0, 1)
        _DebugAmount ("Debug Amount", Range(0, 1)) = 0
	}
    SubShader
    {
        // Draw ourselves after all opaque geometry
        Tags { "Queue" = "Transparent" }

        // Grab the screen behind the object into _BackgroundTexture
        GrabPass
        {
            "_BackgroundTexture"
        }

        // Render the object with the texture generated above, and invert the colors
        Pass
        {

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
			
			static const float FOG_PIXEL_SIZE = 16.0;
			static const float FOG_EDGE_SHARPNESS = 32.0;
			static const float SMOOTHNESS = 1.0 / 32.0;

            struct v2f
            {
                float4 grabPos : TEXCOORD0;
                float2 uv : TEXCOORD1;
                float4 pos : SV_POSITION;
            };
			
			float4 _MainTex_ST; 
			float4 _MainTex_TexelSize; 

            v2f vert(appdata_base v) {
                v2f o;
                // use UnityObjectToClipPos from UnityCG.cginc to calculate 
                // the clip-space of the vertex
                o.pos = UnityObjectToClipPos(v.vertex);
                // use ComputeGrabScreenPos function from UnityCG.cginc
                // to get the correct texture coordinate
                o.grabPos = ComputeGrabScreenPos(o.pos);
				o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
                return o;
            }
			
            sampler2D _BackgroundTexture;
            sampler2D _MainTex;
			float4 _DarknessColor;
			float _DebugAmount;

			float fuzz(float value)
			{
				return clamp(FOG_EDGE_SHARPNESS * (value - 0.5), 0.0, 1.0);
			}

			float distanceFieldCutoff(float distance){
				return smoothstep(0.5 - SMOOTHNESS, 0.5 + SMOOTHNESS, distance);
			}

            half4 frag(v2f i) : SV_Target
            {
				float2 pointUV = (floor(i.uv * _MainTex_TexelSize.zw * FOG_PIXEL_SIZE) + 0.5) * _MainTex_TexelSize.xy / FOG_PIXEL_SIZE;
				float2 pointUV2 = (floor(i.uv * _MainTex_TexelSize.zw) + 0.5) * _MainTex_TexelSize.xy;
				half4 color = tex2Dlod(_MainTex, float4(pointUV, 0, 0));
				half4 color2 = tex2Dlod(_MainTex, float4(pointUV2, 0, 0));
				half4 bgcolor = tex2Dproj(_BackgroundTexture, i.grabPos);


				float amount = 1 - distanceFieldCutoff(color.r);
				amount = max(amount, color.b);
				amount = min(amount, 1 - distanceFieldCutoff(color.g));

				half3 result = lerp(_DarknessColor.rgb, bgcolor, amount);

                return lerp(half4(result, 1.0), color2.bbba, _DebugAmount);
                //return half4(result, 1.0);
            }
            ENDCG
        }

    }
}
