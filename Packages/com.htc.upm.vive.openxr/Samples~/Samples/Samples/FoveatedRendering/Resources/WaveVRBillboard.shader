// Reference: https://en.wikibooks.org/wiki/Cg_Programming/Unity/Billboards

Shader "WaveVR/FixedFOVBillboards" {
	Properties {
		_MainTex("Texture Image", 2D) = "white" {}
		_TangentOfHalfFov("TangentOfHalfFov", Float) = 0.5
	}

	SubShader {
		Tags { "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" }
		LOD 100

		Pass
		{
			Cull Off
			Blend SrcAlpha OneMinusSrcAlpha
			ZTest Always

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			// User-specified uniforms            
			uniform sampler2D _MainTex;
			uniform float _TangentOfHalfFov;

			struct vertexInput {
				float4 vertex : POSITION;
				float4 tex : TEXCOORD0;
			};
			struct vertexOutput {
				float4 pos : SV_POSITION;
				float4 tex : TEXCOORD0;
			};

			vertexOutput vert(vertexInput input)
			{
				vertexOutput output;
				float4 orig = mul(UNITY_MATRIX_MV, float4(0.0, 0.0, 0.0, 1.0));
				float scale = length(orig.xyz) * _TangentOfHalfFov;

				output.pos = mul(UNITY_MATRIX_P,
					orig + float4(input.vertex.x, input.vertex.y, 0.0, 0.0) * float4(scale, scale, 1.0, 1.0));

				output.tex = input.tex;

				return output;
			}

			float4 frag(vertexOutput input) : COLOR
			{
				return tex2D(_MainTex, float2(input.tex.xy));
			}

			ENDCG
		}
	}
}