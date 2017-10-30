Shader "Terrain/SurfaceBased" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_KeyTex ("Key Texture", 2D) = "white" {}
		_LookupTex ("Lookup Texture", 2D) = "white" {}
		_LookupNormal ("Lookup Normal", 2D) = "white" {}
		_LookupTiling ("Lookup Tiling", Vector) = (1, 1, 1, 1)
//		_PavedUV ("Paved UV", Vector) = (0, 0, 0, 0)
//		_GrassUV ("Grass UV", Vector) = (0, 0, 0, 0)
//		_CropUV ("Crop UV", Vector) = (0, 0, 0, 0)
//		_DirtUV ("Dirt UV", Vector) = (0, 0, 0, 0)
//		_PavedTex ("Paved Tex", 2D) = "white" {}
//		_PavedNormal ("Paved Normal", 2D) = "white" {}
//		_GrassTex ("Grass Tex", 2D) = "white" {}
//		_GrassNormal ("Grass Normal", 2D) = "white" {}
//		_DirtTex ("Dirt Tex", 2D) = "white" {}
//		_DirtNormal ("Dirt Normal", 2D) = "white" {}
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard fullforwardshadows

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _KeyTex;
		sampler2D _LookupTex;
		sampler2D _LookupNormal;
//		sampler2D _PavedTex;
//		sampler2D _PavedNormal;
//		sampler2D _GrassTex;
//		sampler2D _GrassNormal;
//		sampler2D _DirtTex;
//		sampler2D _DirtNormal;

		struct Input {
			fixed2 uv_KeyTex;
			fixed2 uv_LookupTex;
//			fixed2 uv_PavedTex;
//			fixed2 uv_GrassTex;
//			fixed2 uv_DirtTex;
		};

		half _Glossiness;
		half _Metallic;
		fixed4 _Color;
		fixed4 _LookupTiling;
//		fixed4 _PavedUV;
//		fixed4 _GrassUV;
//		fixed4 _CropUV;
//		fixed4 _DirtUV;

		// Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
		// See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
		// #pragma instancing_options assumeuniformscaling
		UNITY_INSTANCING_CBUFFER_START(Props)
			// put more per-instance properties here
		UNITY_INSTANCING_CBUFFER_END

		void surf (Input IN, inout SurfaceOutputStandard o) {
			// for now, blend between 3 textures using the r, g, b values from a lookup ("key") texture
			fixed4 key = tex2D ( _KeyTex, IN.uv_KeyTex );
			fixed2 uv;
			fixed tiling;
			uv.x = 1 - key.b + 0.5 * key.r + 0.5 * key.a;
			uv.y = 1 - key.b + 0.5 * key.r + 0.5 * key.g;
			tiling = key.r * _LookupTiling.y + key.g * _LookupTiling.x + key.b * _LookupTiling.z + key.a * _LookupTiling.w;

//			fixed2 repCount = fixed2 ( trunc ( IN.uv_KeyTex.x * tiling ) / tiling, trunc ( IN.uv_KeyTex.y * tiling ) / tiling );
//			fixed2 uv2 = ( IN.uv_KeyTex - repCount ) * 0.5 + uv;
			fixed2 uv2 = frac (IN.uv_KeyTex * tiling) * 0.5 + uv;
//			fixed2 uv2 = IN.uv_KeyTex * 0.5 + uv;

//			fixed val = IN.uv_KeyTex.y;
//			o.Albedo = fixed3 ( val, val, val );

			o.Albedo = tex2D ( _LookupTex, uv2 );
			o.Normal = UnpackNormal ( tex2D ( _LookupNormal, uv2 ) );
			o.Smoothness = 0.1;
			o.Metallic = 0;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
