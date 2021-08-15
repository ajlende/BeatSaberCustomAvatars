Shader "BeatSaber/GLTFUtility/Standard Dither (Specular)" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_SpecGlossMap ("Specular Map", 2D) = "white" {}
		_SpecColor ("Specular Color", Color) = (1,1,1,1)
		_GlossyReflections ("Glossiness", Range(0,1)) = 1
		[Normal] _BumpMap ("Normal", 2D) = "bump" {}
		_BumpScale("NormalScale", Float) = 1.0
		_OcclusionMap ("Occlusion", 2D) = "white" {}
		_EmissionMap ("Emission", 2D) = "black" {}
		_EmissionColor ("Emission Color", Color) = (0,0,0,0)
		_GlowMap ("Glow (A)", 2D) = "black" {}
		_GlowIntensity ("Glow Intensity", Range(0,1)) = 0
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200

		CGPROGRAM
		// Physically based StandardSpecular lighting model, keep alpha because it represents glow
		#pragma surface surf StandardSpecular keepalpha
		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		#include "Shared/Dither Functions.cginc"

		sampler2D _MainTex;
		sampler2D _SpecGlossMap;
		sampler2D _BumpMap;
		sampler2D _OcclusionMap;
		sampler2D _EmissionMap;
		sampler2D _GlowMap;

		struct Input {
			float2 uv_MainTex;
			float2 uv_BumpMap;
			float2 uv_SpecGlossMap;
			float2 uv_OcclusionMap;
			float2 uv_EmissionMap;
			float2 uv_GlowMap;
			float4 color : COLOR;
		};

		half _GlossyReflections;
		half _BumpScale;
		half _GlowIntensity;
		fixed4 _Color;
		fixed4 _EmissionColor;

		// Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
		// See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
		// #pragma instancing_options assumeuniformscaling
		UNITY_INSTANCING_BUFFER_START(Props)
		// put more per-instance properties here
		UNITY_INSTANCING_BUFFER_END(Props)

		void surf (Input IN, inout SurfaceOutputStandardSpecular o) {
			fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
			o.Albedo = c.rgb * IN.color;
			clip(isDithered(IN.uv_MainTex, c.a));
			fixed4 s = tex2D (_SpecGlossMap, IN.uv_SpecGlossMap);
			o.Specular = s.rgb * _SpecColor;
			o.Smoothness = s.a * _GlossyReflections;
			o.Normal = UnpackScaleNormal (tex2D (_BumpMap, IN.uv_BumpMap), _BumpScale);
			o.Occlusion = tex2D (_OcclusionMap, IN.uv_OcclusionMap).r;
			o.Emission = tex2D (_EmissionMap, IN.uv_EmissionMap) * _EmissionColor;
			o.Alpha = tex2D (_GlowMap, IN.uv_GlowMap).a * _GlowIntensity;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
