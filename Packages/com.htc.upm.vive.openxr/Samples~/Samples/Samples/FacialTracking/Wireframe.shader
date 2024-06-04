Shader "Custom/Wireframe"
{
    Properties
    {
        _WireColor("Edges Color", Color) = (0,0,0,1)
        _WireThickness("Edge Thickness", RANGE(0, 800)) = 100
        _WireSmoothness("Edge Smoothness", RANGE(0, 20)) = 3
        _AlbedoColor("Albedo Color", Color) = (1,1,1,1)
        _MainTex("Albedo (RGB)", 2D) = "white" {}
		
		
		//_Color ("Color", Color) = (1,1,1,1)
        //_MainTex ("Albedo (RGB)", 2D) = "white" {}
    }
	

    SubShader
    {
        Tags { "RenderType" = "Opaque" }
        LOD 200

        Pass
        {
        CGPROGRAM
            // Use shader model 3.0 target, to get nicer looking lighting
            #pragma target 3.0

            #include "UnityCG.cginc"

            #pragma vertex vert
            #pragma fragment frag
            #pragma geometry geom

            uniform sampler2D _MainTex;
            uniform float4 _MainTex_ST;

            struct Input
            {
                float2 uv_MainTex;
            };

            fixed4 _WireColor;
            fixed4 _AlbedoColor;

            uniform float _WireThickness;
            uniform float _WireSmoothness;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 texcoord0 : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2g
            {
                float4 projSpaceVertex : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float4 worldSpacePosition : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            struct g2f
            {
                float4 projSpaceVertex : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float4 worldSpacePosition : TEXCOORD1;
                float4 dist : TEXCOORD2;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            v2g vert(appdata v)
            {
                v2g o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.projSpaceVertex = UnityObjectToClipPos(v.vertex);
                o.worldSpacePosition = mul(unity_ObjectToWorld, v.vertex);
                o.uv0 = TRANSFORM_TEX(v.texcoord0, _MainTex);
                return o;
            }


            [maxvertexcount(3)]
            void geom(triangle v2g i[3], inout TriangleStream<g2f> triangleStream)
            {
                float2 p0 = i[0].projSpaceVertex.xy / i[0].projSpaceVertex.w;
                float2 p1 = i[1].projSpaceVertex.xy / i[1].projSpaceVertex.w;
                float2 p2 = i[2].projSpaceVertex.xy / i[2].projSpaceVertex.w;

                float2 edge0 = p2 - p1;
                float2 edge1 = p2 - p0;
                float2 edge2 = p1 - p0;

                float area = abs(edge1.x * edge2.y - edge1.y * edge2.x);
                float wireThickness = 800 - _WireThickness;

                g2f o;

                o.uv0 = i[0].uv0;
                o.worldSpacePosition = i[0].worldSpacePosition;
                o.projSpaceVertex = i[0].projSpaceVertex;
                o.dist.xyz = float3((area / length(edge0)), 0.0, 0.0) * o.projSpaceVertex.w * wireThickness;
                o.dist.w = 1.0 / o.projSpaceVertex.w;
                UNITY_TRANSFER_VERTEX_OUTPUT_STEREO(i[0], o);
                triangleStream.Append(o);

                o.uv0 = i[1].uv0;
                o.worldSpacePosition = i[1].worldSpacePosition;
                o.projSpaceVertex = i[1].projSpaceVertex;
                o.dist.xyz = float3(0.0, (area / length(edge1)), 0.0) * o.projSpaceVertex.w * wireThickness;
                o.dist.w = 1.0 / o.projSpaceVertex.w;
                UNITY_TRANSFER_VERTEX_OUTPUT_STEREO(i[1], o);
                triangleStream.Append(o);

                o.uv0 = i[2].uv0;
                o.worldSpacePosition = i[2].worldSpacePosition;
                o.projSpaceVertex = i[2].projSpaceVertex;
                o.dist.xyz = float3(0.0, 0.0, (area / length(edge2))) * o.projSpaceVertex.w * wireThickness;
                o.dist.w = 1.0 / o.projSpaceVertex.w;
                UNITY_TRANSFER_VERTEX_OUTPUT_STEREO(i[2], o);
                triangleStream.Append(o);
            }

            fixed4 frag(g2f i) : SV_Target
            {
                float minDistanceToEdge = min(i.dist[0], min(i.dist[1], i.dist[2])) * i.dist[3];

                float4 baseColor = _AlbedoColor * tex2D(_MainTex, i.uv0);

                // Early out if we know we are not on a line segment.
                if (minDistanceToEdge > 0.9)
                {
                    return fixed4(baseColor.rgb,0);
                }

                // Smooth our line out
                float t = exp2(_WireSmoothness * -1.0 * minDistanceToEdge * minDistanceToEdge);
                fixed4 finalColor = lerp(baseColor, _WireColor, t);
                finalColor.a = t;

                return finalColor;
            }
            ENDCG
        }
    }
FallBack "Diffuse"
}
