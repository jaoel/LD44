//Shader "Unlit/Minimap Shader"
//{
//	Properties
//	{
//        _MainTex ("Texture", 2D) = "white" {}
//        _WallColor ("Wall Color", Color) = (1, 0, 0, 1)
//        _FloorColor ("Floor Color", Color) = (0, 1, 0, 1)
//	}
//    SubShader
//    {
//        // Render the object with the texture generated above, and invert the colors
//        Pass
//        {
//
//            CGPROGRAM
//            #pragma vertex vert
//            #pragma fragment frag
//            #include "UnityCG.cginc"
//			
//			static const float FOG_PIXEL_SIZE = 1.0;
//			static const float FOG_EDGE_SHARPNESS = 32.0;
//
//            struct v2f
//            {
//                float4 grabPos : TEXCOORD0;
//                float2 uv : TEXCOORD1;
//                float4 pos : SV_POSITION;
//            };
//			
//			float4 _MainTex_ST; 
//			float4 _MainTex_TexelSize; 
//
//            v2f vert(appdata_base v) {
//                v2f o;
//                // use UnityObjectToClipPos from UnityCG.cginc to calculate 
//                // the clip-space of the vertex
//                o.pos = UnityObjectToClipPos(v.vertex);
//                // use ComputeGrabScreenPos function from UnityCG.cginc
//                // to get the correct texture coordinate
//                o.grabPos = ComputeGrabScreenPos(o.pos);
//				o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
//                return o;
//            }
//			
//            sampler2D _MainTex;
//			float4 _WallColor;
//			float4 _FloorColor;
//
//
//			// returns 1 if val is in the range (breakpoint, breakpoint + range_size), otherwise returns 0
//			float in_range(float val, float breakpoint, float rangeSize) {
//				//return ceil(max(range_size - fmod(breakpoint + fmod(val, range_size) - val, 1.0), 0.0));
//				float halfRange = rangeSize / 2.0;
//				return ceil(halfRange - abs(breakpoint + fmod(val + 5 * halfRange, rangeSize) - val - halfRange));
//			}
//
//            half4 frag(v2f i) : SV_Target
//            {
//				float2 pointUV = (floor(i.uv * _MainTex_TexelSize.zw * FOG_PIXEL_SIZE) + 0.5) * _MainTex_TexelSize.xy / FOG_PIXEL_SIZE;
//				half4 color = tex2Dlod(_MainTex, float4(pointUV, 0, 0));
//				float player = in_range(color.b, 1, 0.25);
//				float wall = in_range(color.b, 0.75, 0.25);
//				float floor = in_range(color.b, 0.5, 0.25);
//                return _WallColor * wall + _FloorColor * floor * (0.5 + 0.5 * color.r) + half4(0, 1, 0, 1) * player;
//            }
//            ENDCG
//        }
//
//    }
//}


// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

Shader "UI/Minimap"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _PlayerColor ("Player Color", Color) = (0, 1, 0, 1)
        _WallColor ("Wall Color", Color) = (1, 0, 0, 1)
        _FloorColor ("Floor Color", Color) = (0, 0, 1, 1)

        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255

        _ColorMask ("Color Mask", Float) = 15

        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            Name "Default"
        CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

			static const float PIXEL_SIZE = 1.0;

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord  : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
			float4 _MainTex_TexelSize; 
            fixed4 _Color;
            fixed4 _TextureSampleAdd;
            float4 _ClipRect;
            float4 _MainTex_ST;
			
			float4 _PlayerColor;
			float4 _WallColor;
			float4 _FloorColor;


			// returns 1 if val is in the range (breakpoint, breakpoint + range_size), otherwise returns 0
			float in_range(float val, float breakpoint, float rangeSize) {
				//return ceil(max(range_size - fmod(breakpoint + fmod(val, range_size) - val, 1.0), 0.0));
				float halfRange = rangeSize / 2.0;
				return ceil(halfRange - abs(breakpoint + fmod(val + 5 * halfRange, rangeSize) - val - halfRange));
			}

            v2f vert(appdata_t v)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                OUT.worldPosition = v.vertex;
                OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);

                OUT.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);

                OUT.color = v.color * _Color;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
				float2 pointUV = (floor(IN.texcoord * _MainTex_TexelSize.zw * PIXEL_SIZE) + 0.5) * _MainTex_TexelSize.xy / PIXEL_SIZE;
                half4 color = (tex2D(_MainTex, pointUV) + _TextureSampleAdd) * IN.color;
				float player = in_range(color.b, 1, 0.25);
				float wall = in_range(color.b, 0.75, 0.25);
				float floor = in_range(color.b, 0.5, 0.25);
                color = _WallColor * wall + _FloorColor * floor * (0.5 + 0.5 * color.r) + _PlayerColor * player;
				color.a = 1.0;

                #ifdef UNITY_UI_CLIP_RECT
                color.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                clip (color.a - 0.001);
                #endif

                return color;
            }
        ENDCG
        }
    }
}
