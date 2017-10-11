Shader "Terrain/SurfaceBased" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_KeyTex ("Key Texture", 2D) = "white" {}
		_PavedTex ("Paved Tex", 2D) = "white" {}
		_PavedNormal ("Paved Normal", 2D) = "white" {}
		_GrassTex ("Grass Tex", 2D) = "white" {}
		_GrassNormal ("Grass Normal", 2D) = "white" {}
		_DirtTex ("Dirt Tex", 2D) = "white" {}
		_DirtNormal ("Dirt Normal", 2D) = "white" {}
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
		sampler2D _PavedTex;
		sampler2D _PavedNormal;
		sampler2D _GrassTex;
		sampler2D _GrassNormal;
		sampler2D _DirtTex;
		sampler2D _DirtNormal;

		struct Input {
			fixed2 uv_KeyTex;
			fixed2 uv_PavedTex;
//			fixed2 uv_PavedNormal;
			fixed2 uv_GrassTex;
//			fixed2 uv_GrassNormal;
			fixed2 uv_DirtTex;
//			fixed2 uv_DirtNormal;
		};

		half _Glossiness;
		half _Metallic;
		fixed4 _Color;

		// Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
		// See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
		// #pragma instancing_options assumeuniformscaling
		UNITY_INSTANCING_CBUFFER_START(Props)
			// put more per-instance properties here
		UNITY_INSTANCING_CBUFFER_END

		void surf (Input IN, inout SurfaceOutputStandard o) {
			// for now, blend between 3 textures using the r, g, b values from a lookup ("key") texture
			fixed3 key = tex2D ( _KeyTex, IN.uv_KeyTex );
			fixed4 c1 = tex2D ( _PavedTex, IN.uv_PavedTex ) * key.r;
			fixed4 c2 = tex2D ( _DirtTex, IN.uv_DirtTex ) * key.g;
			fixed4 c3 = tex2D ( _GrassTex, IN.uv_GrassTex ) * key.b;
			o.Albedo = fixed3 ( c1.r + c2.r + c3.r, c1.g + c2.g + c3.g, c1.b + c2.b + c3.b ) * _Color;
			fixed3 n1 = UnpackNormal ( tex2D ( _PavedNormal, IN.uv_PavedTex ) ) * key.r;
			fixed3 n2 = UnpackNormal ( tex2D ( _DirtNormal, IN.uv_DirtTex ) ) * key.g;
			fixed3 n3 = UnpackNormal ( tex2D ( _GrassNormal, IN.uv_GrassTex ) ) * key.b;
			o.Normal = fixed3 ( n1.r + n2.r + n3.r, n1.g + n2.g + n3.b, n1.b + n2.b + n3.b );
//			o.Smoothness = min ( c1.a, min ( c2.a, c3.a ) );
			o.Smoothness = fixed ( c1.a * + c2.a + c3.a );

//			o.Albedo = lerp ( minColor, maxColor, h ) * _Color;
//			o.Albedo = c.rgb;
			// Metallic and smoothness come from slider variables
			o.Metallic = 0;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
