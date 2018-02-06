// Upgrade NOTE: upgraded instancing buffer 'Props' to new syntax.

Shader "Terrain/TilingAtlas" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_Albedo ("Albedo", 2D) = "white" {}
		_Normal ("Normal", 2D) = "white" {}
		_Metal ("MSO (R,A,G)", 2D) = "white" {}

	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard fullforwardshadows

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _Albedo;
		sampler2D _Normal;
		sampler2D _Metal;

		struct Input {
			fixed2 uv_Albedo;
			fixed2 uv2_Bla;//_Albedo
			fixed4 color;
		};

		half _Glossiness;
		half _Metallic;
		fixed4 _Color;

		// Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
		// See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
		// #pragma instancing_options assumeuniformscaling
//		UNITY_INSTANCING_CBUFFER_START(Props)
			// put more per-instance properties here
//		UNITY_INSTANCING_CBUFFER_END(Props)

		void surf (Input IN, inout SurfaceOutputStandard o) {
			// for now, blend between 3 textures using the r, g, b values from a lookup ("key") texture
			fixed2 texUV = IN.uv_Albedo;
//			fixed2 tiling = fixed2 (1, 1);
			fixed2 tiling = IN.uv2_Bla;//_Albedo;

//			fixed2 uv2 = frac (IN.uv_KeyTex * tiling) * 0.5 + uv;
//			fixed2 tiledUV = frac ( fixed2 ( texUV.x * tiling.x, texUV.y * tiling.y ) ) * 0.5 + texUV;
//			fixed2 tiledUV = fixed2 ( texUV.x * tiling.x, texUV.y * tiling.y );

//			fixed tiling = _Tiling.x;
//			fixed size = 0.5;
//			fixed2 quadrant = (0, 0.5);
//			fixed2 uv2 = frac (uv * tiling) * size + quadrant;

//			o.Albedo = IN.color.rgb;
//			o.Normal = fixed3 (0,0,1);
//			o.Emission = 0;
//			o.Metallic = 0;
//			o.Smoothness = 0.1;
//			o.Occlusion = 1;
//			o.Alpha = 1;

			o.Albedo = tex2D ( _Albedo, texUV );
			o.Normal = UnpackNormal ( tex2D ( _Normal, texUV ) );
			o.Metallic = tex2D ( _Metal, texUV ).r;
			o.Smoothness = tex2D ( _Metal, texUV ).a;
			o.Occlusion = tex2D ( _Metal, texUV ).g;
		}

		ENDCG
	}
	FallBack "Diffuse"
}
