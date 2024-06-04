Shader "HTC/MRRoomSetup/UVWireFrame"
{
	//Verticies and UVs of mesh must be preprocessed in order to use this shader.

	Properties
	{
		_LineColor("Line Color", Color) = (1,1,1,1)
		_LineThickness("Line Thickness", float) = 0.5
		_FaceColor("Face Color", Color) = (1,1,1,1)
	}

		SubShader{
			Tags {"Queue" = "Transparent" "RenderType" = "Transparent"}
			LOD 100

			ZWrite Off
			Blend SrcAlpha OneMinusSrcAlpha

			Pass {
				CGPROGRAM
					#pragma vertex vert
					#pragma fragment frag

					#include "UnityCG.cginc"

					struct appdata_t {
						float4 vertex : POSITION;
						float3 normal : NORMAL;
						float2 uv : TEXCOORD0;
						UNITY_VERTEX_INPUT_INSTANCE_ID
					};

					struct v2f {
						float2 uv : TEXCOORD0;
						float4 vertex : SV_POSITION;
						UNITY_VERTEX_OUTPUT_STEREO
					};

					fixed4 _LineColor;
					float _LineThickness;
					fixed4 _FaceColor;

					v2f vert(appdata_t v)
					{
						v2f o;
						UNITY_SETUP_INSTANCE_ID(v);
						UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
						o.vertex = UnityObjectToClipPos(v.vertex);
						o.uv = v.uv;
						return o;
					}

					float distanceSq(float2 pt1, float2 pt2)
					{
						float2 vec = pt2 - pt1;
						return dot(vec, vec);
					}

					float minimum_distance(float2 v, float2 w, float2 p) {
						float l2 = distanceSq(v, w);
						float t = max(0, min(1, dot(p - v, w - v) / l2));
						float2 projection = v + t * (w - v);
						return distance(p, projection);
					}

					fixed4 frag(v2f i) : SV_Target
					{
						fixed4 outputCol;

						float lineWidthInPixels = _LineThickness;
						float lineAntiaAliasWidthInPixels = 1;

						float2 uVector = float2(ddx(i.uv.x),ddy(i.uv.x));
						float2 vVector = float2(ddx(i.uv.y),ddy(i.uv.y));

						float vLength = length(uVector);
						float uLength = length(vVector);
						float uvDiagonalLength = length(uVector + vVector);

						float maximumUDistance = lineWidthInPixels * vLength;
						float maximumVDistance = lineWidthInPixels * uLength;
						float maximumUVDiagonalDistance = lineWidthInPixels * uvDiagonalLength;

						float leftEdgeUDistance = i.uv.x;
						float rightEdgeUDistance = (1.0 - leftEdgeUDistance);

						float bottomEdgeVDistance = i.uv.y;
						float topEdgeVDistance = 1.0 - bottomEdgeVDistance;

						float minimumUDistance = min(leftEdgeUDistance,rightEdgeUDistance);
						float minimumVDistance = min(bottomEdgeVDistance,topEdgeVDistance);
						float uvDiagonalDistance = minimum_distance(float2(0.0,1.0),float2(1.0,0.0),i.uv);

						float normalizedUDistance = minimumUDistance / maximumUDistance;
						float normalizedVDistance = minimumVDistance / maximumVDistance;
						float normalizedUVDiagonalDistance = uvDiagonalDistance / maximumUVDiagonalDistance;


						float closestNormalizedDistance = min(normalizedUDistance,normalizedVDistance);
						closestNormalizedDistance = min(closestNormalizedDistance,normalizedUVDiagonalDistance);


						float lineAlpha = 1.0 - smoothstep(1.0,1.0 + (lineAntiaAliasWidthInPixels / lineWidthInPixels),closestNormalizedDistance);

						if (lineAlpha < 0.99) //Output face color instead
						{
							outputCol = _FaceColor;
						}
						else
						{
							lineAlpha *= _LineColor.a;
							outputCol = fixed4(_LineColor.rgb, lineAlpha);
						}

						return outputCol;
					}
				ENDCG
			}
	}

}
