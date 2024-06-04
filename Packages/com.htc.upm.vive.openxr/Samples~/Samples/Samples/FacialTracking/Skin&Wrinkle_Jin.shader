// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Custom/Skin&Wrinkle_Jin"
{
	Properties
	{
		_Normal("Normal", 2D) = "bump" {}
		_Wrinkle("Wrinkle", 2D) = "bump" {}
		_Wrinkle_Sad("Wrinkle_Sad", 2D) = "bump" {}
		_Wrinkle_Smile("Wrinkle_Smile", 2D) = "bump" {}
		_Wide_M("Wide_M", 2D) = "white" {}
		_SQZ_M("SQZ_M", 2D) = "white" {}
		_lip_Sad_M("lip_Sad_M", 2D) = "white" {}
		_lip_Smile_M("lip_Smile_M", 2D) = "white" {}
		_WrinkleScale_Squeeze_Left("WrinkleScale_Squeeze_Left", Range( 0 , 1)) = 1
		_WrinkleScale_Squeeze_Right("WrinkleScale_Squeeze_Right", Range( 0 , 1)) = 0
		_WrinkleScale_Wide_Up_Left("WrinkleScale_Wide_Up_Left", Range( 0 , 1)) = 0
		_WrinkleScale_Wide_Up_Right("WrinkleScale_Wide_Up_Right", Range( 0 , 1)) = 0
		_WrinkleScale_Sad_Left("WrinkleScale_Sad_Left", Range( 0 , 1)) = 0
		_WrinkleScale_Sad_Right("WrinkleScale_Sad_Right", Range( 0 , 1)) = 0
		_WrinkleScale_Smile_Left("WrinkleScale_Smile_Left", Range( 0 , 1)) = 0
		_WrinkleScale_Smile_Right("WrinkleScale_Smile_Right", Range( 0 , 1)) = 0
		_wrinkle_AllScale("wrinkle_AllScale", Range( 1 , 50)) = 0
		_Albedo("Albedo", 2D) = "white" {}
		_Metalic("Metalic", 2D) = "white" {}
		_MetalicPower("MetalicPower", Range( 0 , 1)) = 0
		_Smoothness("Smoothness", Range( 0 , 1)) = 1
		_SSS_Depth("SSS_Depth", Range( 0 , 10)) = 1
		_SSS_Power("SSS_Power", Range( 0 , 1)) = 1
		_Mask("Mask", 2D) = "white" {}
		_AO_power("AO_power", Range( 0 , 1)) = 1
		_DetailSkin_Normal("DetailSkin_Normal", 2D) = "white" {}
		_DetailSkin_Metalic("DetailSkin_Metalic", 2D) = "white" {}
		_DetailSkin_Tiling("DetailSkin_Tiling", Vector) = (1,1,0,0)
		_DetailSkin_alpha("DetailSkin_alpha", Range( 0 , 1)) = 1
		_DetailSkin_Rotate("DetailSkin_Rotate", Range( 0 , 360)) = 0
		_DetailSkin_MetalicAlpha("DetailSkin_MetalicAlpha", Range( 0 , 1)) = 0
		_Translucency_Power("Translucency_Power", Range( 0 , 1)) = 0.6
		[Header(Translucency)]
		_Translucency("Strength", Range( 0 , 50)) = 1
		_TransNormalDistortion("Normal Distortion", Range( 0 , 1)) = 0.1
		_TransScattering("Scaterring Falloff", Range( 1 , 50)) = 2
		_TransDirect("Direct", Range( 0 , 1)) = 1
		_TransAmbient("Ambient", Range( 0 , 1)) = 0.2
		_TransShadow("Shadow", Range( 0 , 1)) = 0.9
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" }
		Cull Back
		CGINCLUDE
		#include "UnityCG.cginc"
		#include "UnityPBSLighting.cginc"
		#include "Lighting.cginc"
		#pragma target 3.0
		#if defined(SHADER_API_D3D11) || defined(SHADER_API_XBOXONE) || defined(UNITY_COMPILER_HLSLCC) || defined(SHADER_API_PSSL) || (defined(SHADER_TARGET_SURFACE_ANALYSIS) && !defined(SHADER_TARGET_SURFACE_ANALYSIS_MOJOSHADER))//ASE Sampler Macros
		#define SAMPLE_TEXTURE2D(tex,samplerTex,coord) tex.Sample(samplerTex,coord)
		#else//ASE Sampling Macros
		#define SAMPLE_TEXTURE2D(tex,samplerTex,coord) tex2D(tex,coord)
		#endif//ASE Sampling Macros

		#ifdef UNITY_PASS_SHADOWCASTER
			#undef INTERNAL_DATA
			#undef WorldReflectionVector
			#undef WorldNormalVector
			#define INTERNAL_DATA half3 internalSurfaceTtoW0; half3 internalSurfaceTtoW1; half3 internalSurfaceTtoW2;
			#define WorldReflectionVector(data,normal) reflect (data.worldRefl, half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal)))
			#define WorldNormalVector(data,normal) half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal))
		#endif
		struct Input
		{
			float2 uv_texcoord;
			float3 worldNormal;
			INTERNAL_DATA
			float3 worldPos;
		};

		struct SurfaceOutputStandardCustom
		{
			half3 Albedo;
			half3 Normal;
			half3 Emission;
			half Metallic;
			half Smoothness;
			half Occlusion;
			half Alpha;
			half3 Translucency;
		};

		UNITY_DECLARE_TEX2D_NOSAMPLER(_Normal);
		SamplerState sampler_Normal;
		uniform float _WrinkleScale_Squeeze_Left;
		UNITY_DECLARE_TEX2D_NOSAMPLER(_SQZ_M);
		uniform float4 _SQZ_M_ST;
		SamplerState sampler_SQZ_M;
		uniform float _WrinkleScale_Squeeze_Right;
		uniform float _WrinkleScale_Wide_Up_Left;
		UNITY_DECLARE_TEX2D_NOSAMPLER(_Wide_M);
		uniform float4 _Wide_M_ST;
		SamplerState sampler_Wide_M;
		uniform float _WrinkleScale_Wide_Up_Right;
		uniform float _wrinkle_AllScale;
		UNITY_DECLARE_TEX2D_NOSAMPLER(_Wrinkle);
		SamplerState sampler_Wrinkle;
		uniform float _WrinkleScale_Sad_Left;
		UNITY_DECLARE_TEX2D_NOSAMPLER(_lip_Sad_M);
		uniform float4 _lip_Sad_M_ST;
		SamplerState sampler_lip_Sad_M;
		uniform float _WrinkleScale_Sad_Right;
		UNITY_DECLARE_TEX2D_NOSAMPLER(_Wrinkle_Sad);
		SamplerState sampler_Wrinkle_Sad;
		uniform float _WrinkleScale_Smile_Left;
		UNITY_DECLARE_TEX2D_NOSAMPLER(_lip_Smile_M);
		uniform float4 _lip_Smile_M_ST;
		SamplerState sampler_lip_Smile_M;
		uniform float _WrinkleScale_Smile_Right;
		UNITY_DECLARE_TEX2D_NOSAMPLER(_Wrinkle_Smile);
		SamplerState sampler_Wrinkle_Smile;
		UNITY_DECLARE_TEX2D_NOSAMPLER(_DetailSkin_Normal);
		uniform float2 _DetailSkin_Tiling;
		uniform float _DetailSkin_Rotate;
		SamplerState sampler_DetailSkin_Normal;
		uniform float _DetailSkin_alpha;
		UNITY_DECLARE_TEX2D_NOSAMPLER(_Mask);
		SamplerState sampler_Mask;
		UNITY_DECLARE_TEX2D_NOSAMPLER(_Albedo);
		SamplerState sampler_Albedo;
		uniform float _Translucency_Power;
		uniform float _SSS_Depth;
		uniform float _SSS_Power;
		uniform float _MetalicPower;
		UNITY_DECLARE_TEX2D_NOSAMPLER(_Metalic);
		SamplerState sampler_Metalic;
		uniform float _DetailSkin_MetalicAlpha;
		UNITY_DECLARE_TEX2D_NOSAMPLER(_DetailSkin_Metalic);
		SamplerState sampler_DetailSkin_Metalic;
		uniform float _Smoothness;
		uniform float _AO_power;
		uniform half _Translucency;
		uniform half _TransNormalDistortion;
		uniform half _TransScattering;
		uniform half _TransDirect;
		uniform half _TransAmbient;
		uniform half _TransShadow;


		struct Gradient
		{
			int type;
			int colorsLength;
			int alphasLength;
			float4 colors[8];
			float2 alphas[8];
		};


		Gradient NewGradient(int type, int colorsLength, int alphasLength, 
		float4 colors0, float4 colors1, float4 colors2, float4 colors3, float4 colors4, float4 colors5, float4 colors6, float4 colors7,
		float2 alphas0, float2 alphas1, float2 alphas2, float2 alphas3, float2 alphas4, float2 alphas5, float2 alphas6, float2 alphas7)
		{
			Gradient g;
			g.type = type;
			g.colorsLength = colorsLength;
			g.alphasLength = alphasLength;
			g.colors[ 0 ] = colors0;
			g.colors[ 1 ] = colors1;
			g.colors[ 2 ] = colors2;
			g.colors[ 3 ] = colors3;
			g.colors[ 4 ] = colors4;
			g.colors[ 5 ] = colors5;
			g.colors[ 6 ] = colors6;
			g.colors[ 7 ] = colors7;
			g.alphas[ 0 ] = alphas0;
			g.alphas[ 1 ] = alphas1;
			g.alphas[ 2 ] = alphas2;
			g.alphas[ 3 ] = alphas3;
			g.alphas[ 4 ] = alphas4;
			g.alphas[ 5 ] = alphas5;
			g.alphas[ 6 ] = alphas6;
			g.alphas[ 7 ] = alphas7;
			return g;
		}


		float4 SampleGradient( Gradient gradient, float time )
		{
			float3 color = gradient.colors[0].rgb;
			UNITY_UNROLL
			for (int c = 1; c < 8; c++)
			{
			float colorPos = saturate((time - gradient.colors[c-1].w) / (gradient.colors[c].w - gradient.colors[c-1].w)) * step(c, (float)gradient.colorsLength-1);
			color = lerp(color, gradient.colors[c].rgb, lerp(colorPos, step(0.01, colorPos), gradient.type));
			}
			#ifndef UNITY_COLORSPACE_GAMMA
			color = half3(GammaToLinearSpaceExact(color.r), GammaToLinearSpaceExact(color.g), GammaToLinearSpaceExact(color.b));
			#endif
			float alpha = gradient.alphas[0].x;
			UNITY_UNROLL
			for (int a = 1; a < 8; a++)
			{
			float alphaPos = saturate((time - gradient.alphas[a-1].y) / (gradient.alphas[a].y - gradient.alphas[a-1].y)) * step(a, (float)gradient.alphasLength-1);
			alpha = lerp(alpha, gradient.alphas[a].x, lerp(alphaPos, step(0.01, alphaPos), gradient.type));
			}
			return float4(color, alpha);
		}


		inline half4 LightingStandardCustom(SurfaceOutputStandardCustom s, half3 viewDir, UnityGI gi )
		{
			#if !DIRECTIONAL
			float3 lightAtten = gi.light.color;
			#else
			float3 lightAtten = lerp( _LightColor0.rgb, gi.light.color, _TransShadow );
			#endif
			half3 lightDir = gi.light.dir + s.Normal * _TransNormalDistortion;
			half transVdotL = pow( saturate( dot( viewDir, -lightDir ) ), _TransScattering );
			half3 translucency = lightAtten * (transVdotL * _TransDirect + gi.indirect.diffuse * _TransAmbient) * s.Translucency;
			half4 c = half4( s.Albedo * translucency * _Translucency, 0 );

			SurfaceOutputStandard r;
			r.Albedo = s.Albedo;
			r.Normal = s.Normal;
			r.Emission = s.Emission;
			r.Metallic = s.Metallic;
			r.Smoothness = s.Smoothness;
			r.Occlusion = s.Occlusion;
			r.Alpha = s.Alpha;
			return LightingStandard (r, viewDir, gi) + c;
		}

		inline void LightingStandardCustom_GI(SurfaceOutputStandardCustom s, UnityGIInput data, inout UnityGI gi )
		{
			#if defined(UNITY_PASS_DEFERRED) && UNITY_ENABLE_REFLECTION_BUFFERS
				gi = UnityGlobalIllumination(data, s.Occlusion, s.Normal);
			#else
				UNITY_GLOSSY_ENV_FROM_SURFACE( g, s, data );
				gi = UnityGlobalIllumination( data, s.Occlusion, s.Normal, g );
			#endif
		}

		void surf( Input i , inout SurfaceOutputStandardCustom o )
		{
			float2 uv_SQZ_M = i.uv_texcoord * _SQZ_M_ST.xy + _SQZ_M_ST.zw;
			float4 tex2DNode120 = SAMPLE_TEXTURE2D( _SQZ_M, sampler_SQZ_M, uv_SQZ_M );
			Gradient gradient100 = NewGradient( 0, 3, 2, float4( 0, 0, 0, 0.4950027 ), float4( 1, 1, 1, 0.5000076 ), float4( 1, 1, 1, 1 ), 0, 0, 0, 0, 0, float2( 1, 0 ), float2( 1, 1 ), 0, 0, 0, 0, 0, 0 );
			float4 L_m104 = SampleGradient( gradient100, i.uv_texcoord.x );
			Gradient gradient99 = NewGradient( 0, 3, 2, float4( 1, 1, 1, 0.5000076 ), float4( 0, 0, 0, 0.5049973 ), float4( 0, 0, 0, 1 ), 0, 0, 0, 0, 0, float2( 1, 0 ), float2( 1, 1 ), 0, 0, 0, 0, 0, 0 );
			float4 R_m103 = SampleGradient( gradient99, i.uv_texcoord.x );
			float2 uv_Wide_M = i.uv_texcoord * _Wide_M_ST.xy + _Wide_M_ST.zw;
			float4 tex2DNode123 = SAMPLE_TEXTURE2D( _Wide_M, sampler_Wide_M, uv_Wide_M );
			float4 all_Eye_m136 = ( ( ( _WrinkleScale_Squeeze_Left * tex2DNode120 * L_m104 ) + ( _WrinkleScale_Squeeze_Right * R_m103 * tex2DNode120 ) + ( _WrinkleScale_Wide_Up_Left * tex2DNode123 * L_m104 ) + ( _WrinkleScale_Wide_Up_Right * tex2DNode123 * R_m103 ) ) * _wrinkle_AllScale );
			float2 uv_lip_Sad_M = i.uv_texcoord * _lip_Sad_M_ST.xy + _lip_Sad_M_ST.zw;
			float4 tex2DNode114 = SAMPLE_TEXTURE2D( _lip_Sad_M, sampler_lip_Sad_M, uv_lip_Sad_M );
			float4 all_Sad_m137 = ( ( ( _WrinkleScale_Sad_Left * tex2DNode114 * L_m104 ) + ( _WrinkleScale_Sad_Right * tex2DNode114 * R_m103 ) ) * _wrinkle_AllScale );
			float2 uv_lip_Smile_M = i.uv_texcoord * _lip_Smile_M_ST.xy + _lip_Smile_M_ST.zw;
			float4 tex2DNode112 = SAMPLE_TEXTURE2D( _lip_Smile_M, sampler_lip_Smile_M, uv_lip_Smile_M );
			float4 all_Smile_m138 = ( ( ( _WrinkleScale_Smile_Left * tex2DNode112 * L_m104 ) + ( _WrinkleScale_Smile_Right * tex2DNode112 * R_m103 ) ) * _wrinkle_AllScale );
			float4 Normal_out150 = ( float4( UnpackNormal( SAMPLE_TEXTURE2D( _Normal, sampler_Normal, i.uv_texcoord ) ) , 0.0 ) + ( all_Eye_m136 * float4( UnpackNormal( SAMPLE_TEXTURE2D( _Wrinkle, sampler_Wrinkle, i.uv_texcoord ) ) , 0.0 ) ) + ( all_Sad_m137 * float4( UnpackNormal( SAMPLE_TEXTURE2D( _Wrinkle_Sad, sampler_Wrinkle_Sad, i.uv_texcoord ) ) , 0.0 ) ) + ( all_Smile_m138 * float4( UnpackNormal( SAMPLE_TEXTURE2D( _Wrinkle_Smile, sampler_Wrinkle_Smile, i.uv_texcoord ) ) , 0.0 ) ) );
			float2 uv_TexCoord18 = i.uv_texcoord * _DetailSkin_Tiling;
			float cos19 = cos( radians( _DetailSkin_Rotate ) );
			float sin19 = sin( radians( _DetailSkin_Rotate ) );
			float2 rotator19 = mul( uv_TexCoord18 - float2( 0.5,0.5 ) , float2x2( cos19 , -sin19 , sin19 , cos19 )) + float2( 0.5,0.5 );
			float4 tex2DNode26 = SAMPLE_TEXTURE2D( _Mask, sampler_Mask, i.uv_texcoord );
			float4 lerpResult158 = lerp( Normal_out150 , SAMPLE_TEXTURE2D( _DetailSkin_Normal, sampler_DetailSkin_Normal, rotator19 ) , ( _DetailSkin_alpha * tex2DNode26.g ));
			float4 Normal86 = ( ( lerpResult158 * _DetailSkin_alpha ) + Normal_out150 );
			o.Normal = Normal86.rgb;
			float temp_output_37_0 = ( tex2DNode26.r * _Translucency_Power );
			float4 color71 = IsGammaSpace() ? float4(1,1,1,0) : float4(1,1,1,0);
			Gradient gradient72 = NewGradient( 0, 2, 2, float4( 0.972549, 0.7921569, 0.7921569, 0 ), float4( 1, 0.9572161, 0.9481132, 1 ), 0, 0, 0, 0, 0, 0, float2( 1, 0 ), float2( 1, 1 ), 0, 0, 0, 0, 0, 0 );
			float3 ase_worldNormal = WorldNormalVector( i, float3( 0, 0, 1 ) );
			float3 ase_worldPos = i.worldPos;
			#if defined(LIGHTMAP_ON) && UNITY_VERSION < 560 //aseld
			float3 ase_worldlightDir = 0;
			#else //aseld
			float3 ase_worldlightDir = Unity_SafeNormalize( UnityWorldSpaceLightDir( ase_worldPos ) );
			#endif //aseld
			float dotResult7 = dot( ase_worldNormal , ( ase_worldlightDir * _SSS_Depth ) );
			float layeredBlendVar70 = temp_output_37_0;
			float4 layeredBlend70 = ( lerp( color71,( SampleGradient( gradient72, dotResult7 ) * _SSS_Power ) , layeredBlendVar70 ) );
			float4 Albedo81 = ( SAMPLE_TEXTURE2D( _Albedo, sampler_Albedo, i.uv_texcoord ) * layeredBlend70 );
			o.Albedo = Albedo81.rgb;
			float4 tex2DNode46 = SAMPLE_TEXTURE2D( _Metalic, sampler_Metalic, i.uv_texcoord );
			float4 Metallic84 = ( _MetalicPower * tex2DNode46 );
			o.Metallic = Metallic84.r;
			float Smoothness83 = ( ( _DetailSkin_MetalicAlpha * ( SAMPLE_TEXTURE2D( _DetailSkin_Metalic, sampler_DetailSkin_Metalic, i.uv_texcoord ).a * tex2DNode26.g ) ) + ( tex2DNode46.a * _Smoothness ) );
			o.Smoothness = Smoothness83;
			float AO85 = ( tex2DNode26.b * _AO_power );
			o.Occlusion = AO85;
			float Translucency79 = temp_output_37_0;
			float3 temp_cast_9 = (Translucency79).xxx;
			o.Translucency = temp_cast_9;
			o.Alpha = 1;
		}

		ENDCG
		CGPROGRAM
		#pragma surface surf StandardCustom keepalpha fullforwardshadows exclude_path:deferred 

		ENDCG
		Pass
		{
			Name "ShadowCaster"
			Tags{ "LightMode" = "ShadowCaster" }
			ZWrite On
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0
			#pragma multi_compile_shadowcaster
			#pragma multi_compile UNITY_PASS_SHADOWCASTER
			#pragma skip_variants FOG_LINEAR FOG_EXP FOG_EXP2
			#include "HLSLSupport.cginc"
			#if ( SHADER_API_D3D11 || SHADER_API_GLCORE || SHADER_API_GLES || SHADER_API_GLES3 || SHADER_API_METAL || SHADER_API_VULKAN )
				#define CAN_SKIP_VPOS
			#endif
			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "UnityPBSLighting.cginc"
			struct v2f
			{
				V2F_SHADOW_CASTER;
				float2 customPack1 : TEXCOORD1;
				float4 tSpace0 : TEXCOORD2;
				float4 tSpace1 : TEXCOORD3;
				float4 tSpace2 : TEXCOORD4;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};
			v2f vert( appdata_full v )
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID( v );
				UNITY_INITIALIZE_OUTPUT( v2f, o );
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO( o );
				UNITY_TRANSFER_INSTANCE_ID( v, o );
				Input customInputData;
				float3 worldPos = mul( unity_ObjectToWorld, v.vertex ).xyz;
				half3 worldNormal = UnityObjectToWorldNormal( v.normal );
				half3 worldTangent = UnityObjectToWorldDir( v.tangent.xyz );
				half tangentSign = v.tangent.w * unity_WorldTransformParams.w;
				half3 worldBinormal = cross( worldNormal, worldTangent ) * tangentSign;
				o.tSpace0 = float4( worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x );
				o.tSpace1 = float4( worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y );
				o.tSpace2 = float4( worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z );
				o.customPack1.xy = customInputData.uv_texcoord;
				o.customPack1.xy = v.texcoord;
				TRANSFER_SHADOW_CASTER_NORMALOFFSET( o )
				return o;
			}
			half4 frag( v2f IN
			#if !defined( CAN_SKIP_VPOS )
			, UNITY_VPOS_TYPE vpos : VPOS
			#endif
			) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID( IN );
				Input surfIN;
				UNITY_INITIALIZE_OUTPUT( Input, surfIN );
				surfIN.uv_texcoord = IN.customPack1.xy;
				float3 worldPos = float3( IN.tSpace0.w, IN.tSpace1.w, IN.tSpace2.w );
				half3 worldViewDir = normalize( UnityWorldSpaceViewDir( worldPos ) );
				surfIN.worldPos = worldPos;
				surfIN.worldNormal = float3( IN.tSpace0.z, IN.tSpace1.z, IN.tSpace2.z );
				surfIN.internalSurfaceTtoW0 = IN.tSpace0.xyz;
				surfIN.internalSurfaceTtoW1 = IN.tSpace1.xyz;
				surfIN.internalSurfaceTtoW2 = IN.tSpace2.xyz;
				SurfaceOutputStandardCustom o;
				UNITY_INITIALIZE_OUTPUT( SurfaceOutputStandardCustom, o )
				surf( surfIN, o );
				#if defined( CAN_SKIP_VPOS )
				float2 vpos = IN.pos;
				#endif
				SHADOW_CASTER_FRAGMENT( IN )
			}
			ENDCG
		}
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=18400
-1440;188;1440;836;5394.574;-782.314;1;True;False
Node;AmplifyShaderEditor.CommentaryNode;94;-5762.245,872.9059;Inherit;False;613.3813;369.743;R_UV;2;102;99;;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;95;-5766.595,503.0958;Inherit;False;617.7324;334.9375;L_UV;2;101;100;;1,1,1,1;0;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;98;-6000.696,807.8517;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GradientNode;99;-5741.39,964.5538;Inherit;False;0;3;2;1,1,1,0.5000076;0,0,0,0.5049973;0,0,0,1;1,0;1,1;0;1;OBJECT;0
Node;AmplifyShaderEditor.GradientNode;100;-5739.448,634.3599;Inherit;False;0;3;2;0,0,0,0.4950027;1,1,1,0.5000076;1,1,1,1;1,0;1,1;0;1;OBJECT;0
Node;AmplifyShaderEditor.GradientSampleNode;101;-5442.127,630.3428;Inherit;True;2;0;OBJECT;;False;1;FLOAT;0;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GradientSampleNode;102;-5450.901,982.2878;Inherit;True;2;0;OBJECT;;False;1;FLOAT;0;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.CommentaryNode;97;-4702.964,478.5438;Inherit;False;578.3761;292.5326;SQZ_L;3;126;119;116;;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;96;-4704.698,798.3118;Inherit;False;592.7122;250.4255;SQZ_R;3;125;118;117;;1,1,1,1;0;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;103;-5007.304,900.5298;Inherit;False;R_m;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;104;-5048.692,530.6858;Inherit;False;L_m;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;111;-5653.006,1464.302;Inherit;False;104;L_m;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;107;-4584.189,1421.759;Inherit;False;Property;_WrinkleScale_Smile_Left;WrinkleScale_Smile_Left;14;0;Create;True;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;112;-4920.581,1553.702;Inherit;True;Property;_lip_Smile_M;lip_Smile_M;7;0;Create;True;0;0;False;0;False;-1;b7e5809a3a1915e499c18d83d21ac805;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;106;-4506.252,1506.941;Inherit;False;104;L_m;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;119;-4595.809,536.3148;Inherit;False;Property;_WrinkleScale_Squeeze_Left;WrinkleScale_Squeeze_Left;8;0;Create;True;0;0;False;0;False;1;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;108;-4459.899,1733.231;Inherit;False;103;R_m;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;109;-5654.68,1641.468;Inherit;False;103;R_m;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;116;-4558.337,659.2399;Inherit;False;104;L_m;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;122;-4604.923,1225.47;Inherit;False;Property;_WrinkleScale_Wide_Up_Right;WrinkleScale_Wide_Up_Right;11;0;Create;True;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;124;-4606.867,1066.8;Inherit;False;Property;_WrinkleScale_Wide_Up_Left;WrinkleScale_Wide_Up_Left;10;0;Create;True;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;123;-4939.582,1173.537;Inherit;True;Property;_Wide_M;Wide_M;4;0;Create;True;0;0;False;0;False;-1;c8262032472c7f442a7834e3fc15064c;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;117;-4631.354,872.5428;Inherit;False;Property;_WrinkleScale_Squeeze_Right;WrinkleScale_Squeeze_Right;9;0;Create;True;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;105;-4559.904,1644.286;Inherit;False;Property;_WrinkleScale_Smile_Right;WrinkleScale_Smile_Right;15;0;Create;True;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;120;-5067.851,654.3718;Inherit;True;Property;_SQZ_M;SQZ_M;5;0;Create;True;0;0;False;0;False;-1;4fe5e527bd9d40444ae4d3f7efcd7d5e;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;118;-4581.327,963.2089;Inherit;False;103;R_m;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;115;-4510.704,1303.902;Inherit;False;103;R_m;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;113;-5762.145,1554.898;Inherit;False;Property;_WrinkleScale_Sad_Right;WrinkleScale_Sad_Right;13;0;Create;True;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;110;-5760.923,1394.412;Inherit;False;Property;_WrinkleScale_Sad_Left;WrinkleScale_Sad_Left;12;0;Create;True;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;114;-6072.71,1473.314;Inherit;True;Property;_lip_Sad_M;lip_Sad_M;6;0;Create;True;0;0;False;0;False;-1;4fbcb3d92a06ab944af2f2a28f5cacb6;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;121;-4514.828,1145.745;Inherit;False;104;L_m;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;126;-4273.433,607.8378;Inherit;False;3;3;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;125;-4273.177,869.8079;Inherit;False;3;3;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;129;-4229.981,1088.737;Inherit;False;3;3;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;131;-4212.857,1507.256;Inherit;False;3;3;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;132;-4205.058,1647.657;Inherit;False;3;3;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;128;-5367.087,1590.42;Inherit;False;3;3;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;127;-5373.886,1434.849;Inherit;False;3;3;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;130;-4223.379,1212.372;Inherit;False;3;3;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;135;-4035.015,1520.885;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;133;-3924.421,634.5908;Inherit;False;4;4;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;155;-5052.234,1888.661;Inherit;False;Property;_wrinkle_AllScale;wrinkle_AllScale;16;0;Create;True;0;0;False;0;False;0;1;1;50;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;134;-5219.82,1446.772;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;152;-3855.492,1516.114;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;154;-5036.926,1444.667;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;153;-3776.39,643.4372;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;136;-3606.879,630.5257;Inherit;False;all_Eye_m;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;138;-3679.374,1511.708;Inherit;False;all_Smile_m;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;157;-3744.246,1786.114;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RegisterLocalVarNode;137;-4825.111,1443.62;Inherit;False;all_Sad_m;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.CommentaryNode;92;-2075.377,645.8531;Inherit;False;2289.385;655.7867;Normal;13;19;17;16;18;14;15;21;22;23;86;151;156;158;;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;91;-2058.169,-683.2374;Inherit;False;2837.814;1019.518;Albedo;15;72;55;71;11;8;70;81;9;73;10;56;4;7;6;12;;1,1,1,1;0;0
Node;AmplifyShaderEditor.GetLocalVarNode;139;-3369.641,1091.123;Inherit;False;137;all_Sad_m;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;144;-3475.249,870.3969;Inherit;True;Property;_Wrinkle;Wrinkle;1;0;Create;True;0;0;False;0;False;-1;700440cb6385d9640aa622b559c48b0e;None;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;143;-3377.436,794.8948;Inherit;False;136;all_Eye_m;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;142;-3457.164,1444.427;Inherit;True;Property;_Wrinkle_Smile;Wrinkle_Smile;3;0;Create;True;0;0;False;0;False;-1;f2121725b7b607e4db9b34a643c52fd7;None;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;140;-3468.679,1165.854;Inherit;True;Property;_Wrinkle_Sad;Wrinkle_Sad;2;0;Create;True;0;0;False;0;False;-1;547b636f0106a86438c3c7d9bb184861;None;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;141;-3376.13,1367.524;Inherit;False;138;all_Smile_m;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;149;-3061.233,1476.57;Inherit;True;Property;_Normal;Normal;0;0;Create;True;0;0;False;0;False;-1;5f6a88171526ba44b8edd0316d32be0e;None;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.Vector2Node;15;-2001.746,780.6942;Inherit;False;Property;_DetailSkin_Tiling;DetailSkin_Tiling;27;0;Create;True;0;0;False;0;False;1,1;30,30;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;147;-3115.382,1368.964;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT3;0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;146;-3121.658,1087.34;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT3;0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;14;-1980.15,1123.764;Inherit;False;Property;_DetailSkin_Rotate;DetailSkin_Rotate;29;0;Create;True;0;0;False;0;False;0;0;0;360;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;145;-3128.717,826.8129;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT3;0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;12;-2024.223,-205.4592;Inherit;False;Property;_SSS_Depth;SSS_Depth;21;0;Create;True;0;0;False;0;False;1;1;0;10;0;1;FLOAT;0
Node;AmplifyShaderEditor.WorldSpaceLightDirHlpNode;6;-1996.104,-375.6187;Inherit;False;True;1;0;FLOAT;0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.TextureCoordinatesNode;78;-1914.427,375.004;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;11;-1718.054,-374.1815;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.CommentaryNode;93;-1652.657,1332.536;Inherit;False;1866.496;767.1895;Metallic & Smoothness;14;83;49;84;50;48;47;52;51;46;44;45;43;75;74;;1,1,1,1;0;0
Node;AmplifyShaderEditor.WorldNormalVector;8;-1778.006,-537.9479;Inherit;False;False;1;0;FLOAT3;0,0,1;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.TextureCoordinatesNode;18;-1764.468,791.4263;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;148;-2713.122,1102.728;Inherit;False;4;4;0;FLOAT3;0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.Vector2Node;17;-1719.703,955.5605;Inherit;False;Constant;_Vector0;Vector 0;11;0;Create;True;0;0;False;0;False;0.5,0.5;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.RadiansOpNode;16;-1687.149,1134.616;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DotProductOpNode;7;-1533.092,-482.9666;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GradientNode;72;-1611.289,-622.6675;Inherit;False;0;2;2;0.972549,0.7921569,0.7921569,0;1,0.9572161,0.9481132,1;1,0;1,1;0;1;OBJECT;0
Node;AmplifyShaderEditor.SamplerNode;26;-1651.963,347.5779;Inherit;True;Property;_Mask;Mask;23;0;Create;True;0;0;False;0;False;-1;7bf471a629e3ed640b3ace8997379f80;4de633b93a5c4fd4ba6a9263663ff7e1;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RotatorNode;19;-1493.154,893.4912;Inherit;False;3;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;2;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;74;-1586.892,1475.731;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RegisterLocalVarNode;150;-2433.694,1105.526;Inherit;False;Normal_out;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;21;-1270.01,1054.852;Inherit;False;Property;_DetailSkin_alpha;DetailSkin_alpha;28;0;Create;True;0;0;False;0;False;1;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;151;-946.4784,1169.967;Inherit;False;150;Normal_out;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;38;-1161.322,446.4533;Inherit;False;Property;_Translucency_Power;Translucency_Power;31;0;Create;True;0;0;False;0;False;0.6;0.6;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.GradientSampleNode;4;-1301.101,-594.6799;Inherit;True;2;0;OBJECT;;False;1;FLOAT;0;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;56;-1359.7,-345.746;Inherit;False;Property;_SSS_Power;SSS_Power;22;0;Create;True;0;0;False;0;False;1;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;23;-951.6035,963.5776;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;22;-1157.514,746.5341;Inherit;True;Property;_DetailSkin_Normal;DetailSkin_Normal;25;0;Create;True;0;0;False;0;False;-1;cdc6e1684b458a644850dc97191b0b98;cdc6e1684b458a644850dc97191b0b98;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TextureCoordinatesNode;75;-1568.842,1823.917;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;43;-1290.712,1413.351;Inherit;True;Property;_DetailSkin_Metalic;DetailSkin_Metalic;26;0;Create;True;0;0;False;0;False;-1;6cd9fd8642eda8f439b0402013b24d08;6cd9fd8642eda8f439b0402013b24d08;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;37;-792.6582,377.6286;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;46;-1310.521,1795.697;Inherit;True;Property;_Metalic;Metalic;18;0;Create;True;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;71;-791.9308,-197.0864;Inherit;False;Constant;_Color0;Color 0;20;0;Create;True;0;0;False;0;False;1,1,1,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TextureCoordinatesNode;73;-289.7696,157.0661;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;44;-847.4484,1410.771;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;55;-900.9518,-430.5521;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;51;-875.8831,1987.181;Inherit;False;Property;_Smoothness;Smoothness;20;0;Create;True;0;0;False;0;False;1;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;158;-685.9257,780.9528;Inherit;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;45;-778.6556,1602.979;Inherit;False;Property;_DetailSkin_MetalicAlpha;DetailSkin_MetalicAlpha;30;0;Create;True;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.LayeredBlendNode;70;-291.9867,-208.8582;Inherit;True;6;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;52;-622.3829,1910.48;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;47;-471.1548,1523.58;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;54;-1145.253,556.6238;Inherit;False;Property;_AO_power;AO_power;24;0;Create;True;0;0;False;0;False;1;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;159;-431.7331,901.3828;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;48;-574.8556,1437.58;Inherit;False;Property;_MetalicPower;MetalicPower;19;0;Create;True;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;9;-59.90963,144.5609;Inherit;True;Property;_Albedo;Albedo;17;0;Create;True;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;53;-794.247,534.9562;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;156;-236.0334,1015.347;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;49;-234.1212,1746.979;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;10;354.7015,184.0346;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;50;-226.1833,1455.078;Inherit;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;83;-56.14864,1758.119;Inherit;False;Smoothness;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;86;-18.79991,1016.326;Inherit;False;Normal;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;85;-536.3601,562.4451;Inherit;False;AO;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;79;-537.497,429.1004;Inherit;False;Translucency;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;81;546.948,185.7379;Inherit;False;Albedo;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;84;-37.60497,1472.218;Inherit;False;Metallic;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;87;317.3397,872.149;Inherit;False;86;Normal;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;90;293.7708,1009.577;Inherit;False;83;Smoothness;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;89;318.7708,1077.577;Inherit;False;85;AO;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;88;317.7708,940.5767;Inherit;False;84;Metallic;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;80;288.7276,1146.958;Inherit;False;79;Translucency;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;82;314.498,801.9138;Inherit;False;81;Albedo;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;642.5273,826.9124;Float;False;True;-1;2;ASEMaterialInspector;0;0;Standard;JinShader_Skin4;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;ForwardOnly;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;32;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;True;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;101;0;100;0
WireConnection;101;1;98;0
WireConnection;102;0;99;0
WireConnection;102;1;98;0
WireConnection;103;0;102;0
WireConnection;104;0;101;0
WireConnection;126;0;119;0
WireConnection;126;1;120;0
WireConnection;126;2;116;0
WireConnection;125;0;117;0
WireConnection;125;1;118;0
WireConnection;125;2;120;0
WireConnection;129;0;124;0
WireConnection;129;1;123;0
WireConnection;129;2;121;0
WireConnection;131;0;107;0
WireConnection;131;1;112;0
WireConnection;131;2;106;0
WireConnection;132;0;105;0
WireConnection;132;1;112;0
WireConnection;132;2;108;0
WireConnection;128;0;113;0
WireConnection;128;1;114;0
WireConnection;128;2;109;0
WireConnection;127;0;110;0
WireConnection;127;1;114;0
WireConnection;127;2;111;0
WireConnection;130;0;122;0
WireConnection;130;1;123;0
WireConnection;130;2;115;0
WireConnection;135;0;131;0
WireConnection;135;1;132;0
WireConnection;133;0;126;0
WireConnection;133;1;125;0
WireConnection;133;2;129;0
WireConnection;133;3;130;0
WireConnection;134;0;127;0
WireConnection;134;1;128;0
WireConnection;152;0;135;0
WireConnection;152;1;155;0
WireConnection;154;0;134;0
WireConnection;154;1;155;0
WireConnection;153;0;133;0
WireConnection;153;1;155;0
WireConnection;136;0;153;0
WireConnection;138;0;152;0
WireConnection;137;0;154;0
WireConnection;144;1;157;0
WireConnection;142;1;157;0
WireConnection;140;1;157;0
WireConnection;149;1;157;0
WireConnection;147;0;141;0
WireConnection;147;1;142;0
WireConnection;146;0;139;0
WireConnection;146;1;140;0
WireConnection;145;0;143;0
WireConnection;145;1;144;0
WireConnection;11;0;6;0
WireConnection;11;1;12;0
WireConnection;18;0;15;0
WireConnection;148;0;149;0
WireConnection;148;1;145;0
WireConnection;148;2;146;0
WireConnection;148;3;147;0
WireConnection;16;0;14;0
WireConnection;7;0;8;0
WireConnection;7;1;11;0
WireConnection;26;1;78;0
WireConnection;19;0;18;0
WireConnection;19;1;17;0
WireConnection;19;2;16;0
WireConnection;150;0;148;0
WireConnection;4;0;72;0
WireConnection;4;1;7;0
WireConnection;23;0;21;0
WireConnection;23;1;26;2
WireConnection;22;1;19;0
WireConnection;43;1;74;0
WireConnection;37;0;26;1
WireConnection;37;1;38;0
WireConnection;46;1;75;0
WireConnection;44;0;43;4
WireConnection;44;1;26;2
WireConnection;55;0;4;0
WireConnection;55;1;56;0
WireConnection;158;0;151;0
WireConnection;158;1;22;0
WireConnection;158;2;23;0
WireConnection;70;0;37;0
WireConnection;70;1;71;0
WireConnection;70;2;55;0
WireConnection;52;0;46;4
WireConnection;52;1;51;0
WireConnection;47;0;45;0
WireConnection;47;1;44;0
WireConnection;159;0;158;0
WireConnection;159;1;21;0
WireConnection;9;1;73;0
WireConnection;53;0;26;3
WireConnection;53;1;54;0
WireConnection;156;0;159;0
WireConnection;156;1;151;0
WireConnection;49;0;47;0
WireConnection;49;1;52;0
WireConnection;10;0;9;0
WireConnection;10;1;70;0
WireConnection;50;0;48;0
WireConnection;50;1;46;0
WireConnection;83;0;49;0
WireConnection;86;0;156;0
WireConnection;85;0;53;0
WireConnection;79;0;37;0
WireConnection;81;0;10;0
WireConnection;84;0;50;0
WireConnection;0;0;82;0
WireConnection;0;1;87;0
WireConnection;0;3;88;0
WireConnection;0;4;90;0
WireConnection;0;5;89;0
WireConnection;0;7;80;0
ASEEND*/
//CHKSM=88987B34B8087CB6E197D31CBF383279D1C2130A