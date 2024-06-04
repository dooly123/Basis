Shader "Vive/DartBoardShader" {
	Properties{
		_Color0("Color_0", Color) = (0.48, 0.44, 0.32, 1)
		_Color1("Color_1", Color) = (0.07, 0.07, 0.08, 1)
		_Color2("Color_2", Color) = (0.39, 0.09, 0.09, 1)
		_Color3("Color_3", Color) = (0.09, 0.27, 0.09, 1)
		_ColorHit("Color_Hit", Color) = (0.8, 0.78, 0, 1)
		
		[HideInInspector]_FrameWidth("FrameWidth", Int) = 1
		[HideInInspector] _MeshForward("MeshForward", Vector) = (0, 0, 0, 0)
		[HideInInspector] _MeshRight("MeshRight", Vector) = (0, 0, 0, 0)
		[HideInInspector] _MeshCenter("MeshCenter", Vector) = (0, 0, 0, 0)
		[HideInInspector] _BendCount("BendCount", Int) = 1
		[HideInInspector] _PieCount("PieCount", Int) = 1
		[HideInInspector] _BendIndex("BendIndex", Int) = -2
		[HideInInspector] _PieIndex("PieIndex", Int) = 1
		[HideInInspector] _Scale("Scale", Float) = 1
		[HideInInspector] _MainTex("MainTex (RGB)", 2D) = "white" {}
	}
		SubShader{
			Tags { "RenderType" = "Opaque" "Queue" = "Transparent" }
			LOD 200

			CGPROGRAM
			#pragma surface surf NoLighting alpha

			fixed4 LightingNoLighting(SurfaceOutput s, fixed3 lightDir, fixed atten)
			{
				return fixed4(s.Albedo, s.Alpha);
			}

			sampler2D _MainTex;

			struct Input {
				float2 uv_MainTex;
				float3 worldPos;
			};

			fixed3 _Color0, _Color1, _Color2, _Color3, _ColorHit;
			float4 _MeshCenter, _MeshRight, _MeshForward;
			uint _BendIndex, _PieIndex, _BendCount, _PieCount, _FrameWidth;
			float _Scale;

			int signedAngle(float3 v1, float3 v2, float3 v_forward)
			{
				float dotP = dot(v1, v2);
				float unsignedAngle = acos(dotP) * (180 / 3.14159);

				float sign = dot(v_forward, cross(v1, v2));
				float signedAngle = unsignedAngle * (sign >= 0 ? 1 : -1) + 180;

				return signedAngle;
			}

			void surf(Input IN, inout SurfaceOutput o)
			{
				fixed tex_a = tex2D(_MainTex, IN.uv_MainTex).a;

				fixed4 c;
				c.rgb = _Color0 / 2;
				c.a = 1;

				float maxDist = 0.42 * _Scale;
				float dist;

				if (_BendIndex == -2) dist = distance(fixed2(0, 0), IN.uv_MainTex - fixed2(0.5, 0.5));
				else dist = distance(IN.worldPos, _MeshCenter);

				if (dist > maxDist) clip(-1);
				else if (_BendIndex != -2)
				{
					float3 v1 = _MeshRight;
					float3 v2 = normalize(IN.worldPos - _MeshCenter);
					int ang = signedAngle(v1, v2, _MeshForward);
					float pieDegree = 360 / _PieCount;
					uint pieIndex = ang / pieDegree;

					float bendSectionLength = maxDist / _BendCount;
					uint bendIndex = dist / bendSectionLength;

					if (bendIndex <= _BendCount - 1 && bendIndex >= _BendCount - _FrameWidth)
						c.rgb = _Color1;
					else if (bendIndex == 0)
						c.rgb = (_BendIndex == bendIndex) ? _ColorHit : _Color2;
					else if (bendIndex == 1)
						c.rgb = (_BendIndex == bendIndex) ? _ColorHit : _Color3;
					else if (bendIndex % 4 == 2)
					{
						if (_BendIndex >= bendIndex && _BendIndex <= bendIndex + 2 && _PieIndex == pieIndex)
							c.rgb = _ColorHit;
						else c.rgb = (pieIndex % 2 == 0) ? _Color1 : _Color0;
					}
					else if (bendIndex % 4 == 3)
					{
						if (_BendIndex >= bendIndex - 1 && _BendIndex <= bendIndex + 1 && _PieIndex == pieIndex)
							c.rgb = _ColorHit;
						else c.rgb = (pieIndex % 2 == 0) ? _Color1 : _Color0;
					}
					else if (bendIndex % 4 == 0)
					{
						if (_BendIndex >= bendIndex - 2 && _BendIndex <= bendIndex && _PieIndex == pieIndex)
							c.rgb = _ColorHit;
						else c.rgb = (pieIndex % 2 == 0) ? _Color1 : _Color0;
					}
					else if (bendIndex % 4 == 1)
					{
						if (_BendIndex == bendIndex && _PieIndex == pieIndex)
							c.rgb = _ColorHit;
						else c.rgb = (pieIndex % 2 == 0) ? _Color2 : _Color3;
					}
				}

				o.Albedo = c.rgb;
				o.Alpha = c.a;
			}
			ENDCG
	}
		FallBack "Diffuse"
}