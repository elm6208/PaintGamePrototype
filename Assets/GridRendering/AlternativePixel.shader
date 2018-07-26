Shader "Custom/Blended Noise Texture Shader Alternate Pixel" {
	Properties{
		_Color("Color", Color) = (1,1,1,1)
		_MainTex("Albedo (RGB)", 2D) = "white" {}
	_MainTex2("Albedo (RGB)", 2D) = "white" {}

	_Glossiness("Smoothness", Range(0,1)) = 0.5
		_Metallic("Metallic", Range(0,1)) = 0.0

		_NoiseFactor1("Noise Factor1", Range(0.1,100)) = 1
		_NoiseFactor2("Noise Factor2", Range(0.1,100)) = 1

	}

		SubShader{
		Tags{ "RenderType" = "Opaque" }
		LOD 200

		CGPROGRAM
#include "noise.cginc"
#pragma surface surf Standard vertex:vert fullforwardshadows
#pragma target 3.0
		struct Input {
		float2 uv_MainTex;
		float2 uv2_MainTex2;

		float3 vertexColor; // Vertex color stored here by vert() method
		float3 worldPos;
	};

	struct v2f {
		float4 pos : SV_POSITION;
		fixed4 color : COLOR;
	};

	void vert(inout appdata_full v, out Input o)
	{
		UNITY_INITIALIZE_OUTPUT(Input,o);
		o.vertexColor = v.color; // Save the Vertex Color in the Input for the surf() method
	}

	sampler2D _MainTex;
	sampler2D _MainTex2;
	uniform float2 _MainTex_TexelSize;
	uniform float _NoiseFactor1;
	uniform float _NoiseFactor2;
	uniform float _Length;

	half _Glossiness;
	half _Metallic;
	fixed4 _Color;

	void surf(Input IN, inout SurfaceOutputStandard o)
	{
		float3 worldNoise = simplexNoise((IN.worldPos *_NoiseFactor1));
		float3 worldNoise2 = simplexNoise((IN.worldPos * _NoiseFactor2));

		float2 units = _MainTex_TexelSize;

		//uv2 = 0-1
		float2 coord = IN.uv_MainTex;

		float2 discreteCoordinate = coord;
		discreteCoordinate /= units;
		discreteCoordinate = floor(discreteCoordinate);
		
		float2 masterDiscreteCoordinate = discreteCoordinate;

		//originally an even pixel coordinate
		fixed isEvenX = (masterDiscreteCoordinate.x) % 2 < 0.1;
		fixed isEvenY = (masterDiscreteCoordinate.y) % 2 < 0.1;
		fixed isRight = coord.x + units/2> masterDiscreteCoordinate.x;
		fixed isUp = coord.y + units/2 > masterDiscreteCoordinate.y;

		//force it to be even
		if (!isEvenX)
		{
			if (isRight)
			{
				masterDiscreteCoordinate.x += 1;
			}
			else {
				masterDiscreteCoordinate.x -= 1;
			}
		}
		if (!isEvenY)
		{
			if (isUp)
			{
				masterDiscreteCoordinate.y += 1;
			}
			else {
				masterDiscreteCoordinate.y -= 1;
			}
		}
		masterDiscreteCoordinate *= units;
		discreteCoordinate *= units;

		float2 coord2 = discreteCoordinate;

		float4 tex = tex2D(_MainTex, coord - units/2);
		float4 co = tex2D(_MainTex, masterDiscreteCoordinate + units/2);
		float4 c2 = float4(0,0,0,1);
	
		if (!isEvenX)
		{
			c2.r = 1;
		}
		else {
			c2.r = 0;
		}

		if (!isEvenY)
		{
			c2.g = 1;
		}
		else {
			c2.g = 0;
		}

		float2 distanceToMaster = (coord - units/2 ) - (masterDiscreteCoordinate + units/2);
		float noiseFactor = max(length(worldNoise), length(worldNoise2));
		if (noiseFactor < 0.7)
		{
			noiseFactor = 0.7;
		}
		float blend = length(distanceToMaster) <  noiseFactor * 0.8 * length(units) ? 0 : 1;
		

		o.Albedo = lerp(co, c2, blend);

		o.Smoothness = _Glossiness;

		o.Metallic = _Metallic;

		o.Alpha = 1;
	}
	ENDCG
	}
		FallBack "Diffuse"
}