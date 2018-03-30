Shader "Example/Detail" {
    Properties {
   	 _Color ("Color", Color) = (1,1,1,1)
   	  _SpecularColor ("Specular Color", Color) = (1,1,1,1)
      _MainTex ("Texture", 2D) = "white" {}
      _BumpMap ("Bumpmap", 2D) = "bump" {}
      BumpGain ("Bump gain",Float) = 1
      SpecularGain ("Specular gain",Float) = 1
      ReflectionGain ("Reflection gain",Float) = 1
      _Detail ("Detail", 2D) = "gray" {}
      _Cube ("Cubemap", CUBE) = "" {}
    }
    SubShader {
      Tags { "RenderType" = "Opaque" "Queue" = "Geometry+20"}

      ZWrite On
      CGPROGRAM
    //  #pragma surface surf Lambert
      #pragma surface surf SimpleSpecular

        float SpecularGain;

    half4 LightingSimpleSpecular (SurfaceOutput s, half3 lightDir, half3 viewDir, half atten) {
        half3 h = normalize (lightDir + viewDir);

        half diff = max (0, dot (s.Normal, lightDir));

        float nh = max (0, dot (s.Normal, h))* SpecularGain;
        float spec = pow (nh, 48.0) ;

        half4 c;
        c.rgb = (s.Albedo * _LightColor0.rgb * diff + _LightColor0.rgb * spec) * atten;
        c.a = s.Alpha;
        return c;

        }
      struct Input {
          float2 uv_MainTex;
          float2 uv_BumpMap;
          float2 uv_Detail;
          float3 worldRefl;
          INTERNAL_DATA
      };
      sampler2D _MainTex;
      sampler2D _BumpMap;
      sampler2D _Detail;
      samplerCUBE _Cube;
      float BumpGain;
    
      float ReflectionGain;
      fixed4 _Color;
      fixed4 _SpecularColor;

      void surf (Input IN, inout SurfaceOutput o) {
          o.Albedo = tex2D (_MainTex, IN.uv_MainTex).rgb * _Color;
          o.Albedo *= tex2D (_Detail, IN.uv_Detail).rgb  * _Color;  
           o.Emission = texCUBE (_Cube, WorldReflectionVector (IN, o.Normal)).rgb * ReflectionGain;                
           o.Normal = UnpackNormal (tex2D (_BumpMap, IN.uv_BumpMap)) * BumpGain;
          

      }
      ENDCG
    } 
    Fallback "Diffuse"
  }