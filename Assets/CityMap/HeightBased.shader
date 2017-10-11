Shader "Terrain/HeightBased" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_LowTex ("Low Texture", 2D) = "white" {}
		_LowNormal ("Low Normal", 2D) = "white" {}
		_HighTex ("High Texture", 2D) = "white" {}
		_HighNormal ("High Normal", 2D) = "white" {}
		_MinHeight ("Min Height", Float) = 0
		_MaxHeight ("Max Height", Float) = 1000
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard fullforwardshadows

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _LowTex;
		sampler2D _HighTex;

		struct Input {
			float2 uv_LowTex;
			float2 uv_HighTex;
			float3 worldPos;
		};

		half _Glossiness;
		half _Metallic;
		fixed4 _Color;
		half _MinHeight;
		half _MaxHeight;

		// Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
		// See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
		// #pragma instancing_options assumeuniformscaling
		UNITY_INSTANCING_CBUFFER_START(Props)
			// put more per-instance properties here
		UNITY_INSTANCING_CBUFFER_END

		void surf (Input IN, inout SurfaceOutputStandard o) {
			// Albedo comes from a texture tinted by color
			fixed4 minColor = tex2D (_LowTex, IN.uv_LowTex);
			fixed4 maxColor = tex2D (_HighTex, IN.uv_HighTex);
			float h = clamp ( IN.worldPos.y, _MinHeight, _MaxHeight );
			h = ( h - _MinHeight ) / ( _MaxHeight - _MinHeight );

			o.Albedo = lerp ( minColor, maxColor, h ) * _Color;
//			o.Albedo = c.rgb;
			// Metallic and smoothness come from slider variables
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
