Shader "Unlit/Fog of War Shader"
{
	Properties
	{
        _MainTex ("Texture", 2D) = "white" {}
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

            struct v2f
            {
                float4 grabPos : TEXCOORD0;
                float2 uv : TEXCOORD1;
                float4 pos : SV_POSITION;
            };

			float4 _MainTex_ST; 

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

            half4 frag(v2f i) : SV_Target
            {
                half4 bgcolor = tex2Dproj(_BackgroundTexture, i.grabPos);
                half4 color = tex2D(_MainTex, i.uv);
				half lum = Luminance(bgcolor.rgb);

				float desaturation = color.r;
				float darkness = color.g * 0.5 + color.r * 0.5;

				half3 result = lerp(lum.xxx * darkness, bgcolor.rgb * darkness, desaturation);

                return half4(result, 1.0);
            }
            ENDCG
        }

    }
}
