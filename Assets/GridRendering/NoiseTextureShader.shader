// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

// Unlit shader. Simplest possible textured shader.
// - no lighting
// - no lightmap support
// - no per-material color

Shader "Custom/Blended Noisy" {
	Properties{
		_Color("Empty Color", Color) = (0,0,0,0)
		_MainTex("Albedo (RGB)", 2D) = "white" {}

	[Toggle]
	_ApplyNoise("Use Noise?", Float) = 1
		_NoiseFactor1("Noise Factor1", Range(0.1,100)) = 1
		_NoiseFactor2("Noise Factor2", Range(0.1,100)) = 1
	}

		SubShader{
		Tags{ "RenderType" = "Opaque" }

		Pass{
		CGPROGRAM


		#pragma vertex vert
		#pragma fragment frag
		#pragma target 4.0
		#pragma multi_compile_fog

		#include "UnityCG.cginc"
#include "noise.cginc"

				struct appdata_t {
				float4 vertex : POSITION;
				float2 texcoord : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f {
				float4 vertex : SV_POSITION;
				float2 texcoord : TEXCOORD0;
				float3 worldPos : TEXCOORD1;
				UNITY_FOG_COORDS(1)
					UNITY_VERTEX_OUTPUT_STEREO
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			float4 _MainTex_TexelSize;

			uniform float _ApplyNoise;
			uniform float _NoiseFactor1;
			uniform float _NoiseFactor2;
			uniform float _Length;

			uniform fixed4 _Color;

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

			v2f vert(appdata_t v)
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
				o.worldPos = mul(unity_ObjectToWorld, v.vertex)

				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}

			fixed3 frag(v2f IN) : SV_Target
			{
				float3 worldNoise = simplexNoise((IN.worldPos *_NoiseFactor1));
				float3 worldNoise2 = simplexNoise((IN.worldPos * _NoiseFactor2));

				float2 units = _MainTex_TexelSize;

				//uv2 = 0-1
				float2 coord = IN.texcoord;

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
				float smoother = (distanceSq - sqShorter) / (sqLonger - sqShorter);
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

					if (!ySame)
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
				}

				fixed3 outputColor;
				if (needsSmoothing.x && !needsSmoothing.y)
				{
					smootherx = noisySmoother(noiseFactor, smootherx);

					outputColor = lerp(lerp(co, c2, blend), _Color, smootherx);
				}
				else if (needsSmoothing.y && !needsSmoothing.x)
				{
					smoothery = noisySmoother(noiseFactor, smoothery);

					outputColor = lerp(lerp(co, c2, blend), _Color, smoothery);
				}
				else {
					smoother = noisySmoother(noiseFactor, smoother);

					if (!any(needsSmoothing))
					{
						smoother = 0;
					}
					outputColor = lerp(lerp(co, c2, blend), _Color, smoother);
				}


			UNITY_APPLY_FOG(i.fogCoord, outputColor);
			return outputColor;
			}
				ENDCG
			}
	}

}


