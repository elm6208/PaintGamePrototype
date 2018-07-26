Shader "Custom/Blended Noise Texture Shader" {
	Properties{
		
		_Color("Empty Color", Color) = (0,0,0,0)
		_MainTex("Albedo (RGB)", 2D) = "white" {}

	[Toggle]
		_ApplyNoise("Use Noise?", Float) = 1
		_NoiseFactor1("Noise Factor1", Range(0.1,100)) = 1
		_NoiseFactor2("Noise Factor2", Range(0.1,100)) = 1

	}

		SubShader{
		Tags{ "RenderType" = "Transparent" "RenderQueue"="3000" }
		LOD 200

		CGPROGRAM
#include "noise.cginc"
#pragma surface surf NoLighting vertex:vert alpha
#pragma target 4.0
		struct Input {
		float2 uv_MainTex;
 
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
	uniform float2 _MainTex_TexelSize;
	uniform float _ApplyNoise;
	uniform float _NoiseFactor1;
	uniform float _NoiseFactor2;
	uniform float _Length;

	fixed4 _Color;
	fixed4 LightingNoLighting(SurfaceOutput s, fixed3 lightDir, fixed atten)
	{
		fixed4 c;
		c.rgb = s.Albedo;
		c.a = s.Alpha;
		return c;
	}


	float noisySmoother(float3 noiseFactor, float smoother)
	{
		if (_ApplyNoise)
		{
			if (length(noiseFactor) * smoother > 0.3)
			{
				smoother = lerp(smoother, 1, length(noiseFactor.xy));
			}
			return saturate(smoother);
		}
		else {
			return smoother;
		}
	}

	void surf(Input IN, inout SurfaceOutput o)
	{
		float3 worldNoise = simplexNoise((IN.worldPos *_NoiseFactor1));
		float3 worldNoise2 = simplexNoise((IN.worldPos * _NoiseFactor2));

		float2 units = _MainTex_TexelSize;

		//uv2 = 0-1
		float2 coord = IN.uv_MainTex;

		float2 discreteCoordinate = coord;
		discreteCoordinate /= units;
		discreteCoordinate = floor(discreteCoordinate);
		discreteCoordinate *= units;
		discreteCoordinate += units / 2;
		float3 co = tex2D(_MainTex, discreteCoordinate);

			float2 neighborUV = discreteCoordinate;
			float2 gridUnits = units / 3;
			float test = 0;
			float xplus = discreteCoordinate.x + (gridUnits).x / 2;
			float xminus = discreteCoordinate.x - gridUnits.x / 2;

			float yplus = discreteCoordinate.y + (gridUnits).y / 2;
			float yminus = discreteCoordinate.y - gridUnits.y / 2;


			if (coord.x > xplus)
			{
				neighborUV.x += units.x;
			}
			else
				if (coord.x < xminus)
				{
					neighborUV.x -= units.x;
				}

			if (coord.y > yplus)
			{
				neighborUV.y += units.y;
			}
			else
				if (coord.y < yminus)
				{
					neighborUV.y -= units.y;
				}

			float2 blendDifference = neighborUV - discreteCoordinate;
			float2 coord2 = discreteCoordinate;

			float3 neighbor = tex2D(_MainTex, neighborUV);
			float neighborSame = all(co == neighbor);
			float2 needsSmoothing = 0;
			float3 c2 = neighbor;

			float2 distanceToMaster = coord - discreteCoordinate;

			float noiseFactor = max(length(worldNoise), length(worldNoise2));

			float blend;
			float xSame = 0;
			float ySame = 0;
			float4 neighbor2;

			//this is for making rounded rectangles
			float sqpower = 2;
				float distanceSq = length(pow(distanceToMaster, sqpower));
				float sqLonger = 1.8*length(pow(gridUnits, sqpower));
				float sqShorter = 0;

				blend = distanceSq < sqLonger ? 0 : 1;

				//smoother 
				float smoother = (distanceSq - sqShorter) / (sqLonger-sqShorter);
				float smootherx;
				float smoothery;

				//smoother x
				float2 d2m = distanceToMaster;
				d2m.y = 0;
				float ds = length(pow(d2m, sqpower));;
				smootherx = 1.0 - ((ds - sqLonger) / (sqShorter - sqLonger));

				//smoothery
				d2m = distanceToMaster;
				d2m.x = 0;
				ds = length(pow(d2m, sqpower));;
				smoothery = 1.0 - ((ds - sqLonger) / (sqShorter - sqLonger));

		
			float xSame2 = 0;
			float ySame2 = 0;

				if (neighborUV.x != discreteCoordinate.x)
				{
					float2 neighborUV2 = neighborUV;
					neighborUV2.y = discreteCoordinate.y;
					float4 neighbor2 = tex2D(_MainTex, neighborUV2);
					xSame = all(neighbor2 == co);

					if (!xSame)
					{
						needsSmoothing.x = 1;
					}
				}
				else {
					xSame = 0;
				}
				if (neighborUV.y != discreteCoordinate.y)
				{
					float2 neighborUV2 = neighborUV;
					neighborUV2.x = discreteCoordinate.x;
					float4 neighbor2 = tex2D(_MainTex, neighborUV2);

					ySame = all(neighbor2 == co);

					if (!ySame )
					{
						needsSmoothing.y = 1;
					}
				}
				else {
					ySame = 0;
				}
				if (xSame || ySame)
				{
					//corners that bleed in and look like artifacts. ok area for noise
					blend = 0;

				}

				float flag = 0;
				if (xSame && ySame)
				{
					//this is an inside corner
					if (!neighborSame)
					{
						flag = 1;
						needsSmoothing.xy = 1;

						//includes corner piece
						//wrong value, but close in some situations
						smoother = 1.3*(smootherx * smoothery) - 0.05;
					}
					/*
					//this is an inside corner surrounded by all the same color
					else {
						needsSmoothing.xy = 1;


					}*/
				}

				/*
				if (xSame2 && ySame2)
				{
					//inside rounded corners
					c2 = insideRounded;
					if (flag == 1)
					{
						needsSmoothing.xy = 0;
					}
				}
				*/				


				if (needsSmoothing.x && !needsSmoothing.y)
				{
					smootherx = noisySmoother(noiseFactor, smootherx);

					o.Albedo = lerp(lerp(co, c2, blend), _Color, smootherx);
				}
				else if (needsSmoothing.y && !needsSmoothing.x)
				{
					smoothery = noisySmoother(noiseFactor, smoothery);

					o.Albedo = lerp(lerp(co, c2, blend), _Color, smoothery);
				}
				else {
					smoother = noisySmoother(noiseFactor, smoother);

					if (!any(needsSmoothing))
					{
						smoother = 0;
					}
					o.Albedo = lerp(lerp(co, c2, blend), _Color, smoother);
				}

				o.Alpha = 1 ;

	}

	ENDCG
	}
		FallBack "Diffuse"
}