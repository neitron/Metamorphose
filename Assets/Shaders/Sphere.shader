// Upgrade NOTE: upgraded instancing buffer 'Props' to new syntax.

Shader "CS/GpuInstancing" 
{

	Properties 
	{
		_Color ("Color", Color) = (1,1,1,1)
		[HDR]_ColorBack ("Color Back", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0.0, 1.0)) = 0.0
		
		_HullOffset ("Hull Offset", Range(0, 0.5)) = 0.0
	}

	SubShader 
	{
		Tags 
		{ 
			"RenderType" = "Opaque" 
		}

		LOD 200
		Cull Off

		CGPROGRAM

			// Physically based Standard lighting model, and enable shadows on all light types
			#pragma surface surf Standard vertex:vert fullforwardshadows
			#pragma instancing_options assumeuniformscaling

			// Use shader model 3.0 target, to get nicer looking lighting
			#pragma target 3.0


			struct appdata
			{
				float4 vertex : POSITION;
				float3 normal : NORMAL;
				float4 tangent : TANGENT;
				float4 texcoord : TEXCOORD0;
				float4 texcoord1 : TEXCOORD1;
				float4 texcoord2 : TEXCOORD2;
				uint vid : SV_VertexID;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};


			sampler2D _MainTex;
			fixed4 _ColorBack;

			struct Input 
			{
				fixed facing:VFACE;
				float2 uv_MainTex;
			};

			UNITY_INSTANCING_BUFFER_START(Props)
				UNITY_DEFINE_INSTANCED_PROP(fixed4, _Color)
				UNITY_DEFINE_INSTANCED_PROP(half, _Glossiness)
				UNITY_DEFINE_INSTANCED_PROP(half, _Metallic)
				UNITY_DEFINE_INSTANCED_PROP(float4x4, _VertexPositions)
				UNITY_DEFINE_INSTANCED_PROP(float4x4, _VertexNormals)
				UNITY_DEFINE_INSTANCED_PROP(float4x4, _VertexUV)
			UNITY_INSTANCING_BUFFER_END(Props)


			float _HullOffset;
			float _MetamorphoseLevel;
			static const float PI = 3.14159265359f;

			void vert(inout appdata v)
			{
				float4x4 poses = UNITY_ACCESS_INSTANCED_PROP(Props, _VertexPositions);
				v.vertex.xyz = float3 ( poses[0][v.vid], poses[1][v.vid], poses[2][v.vid] );

				float4x4 norms = UNITY_ACCESS_INSTANCED_PROP(Props, _VertexNormals);
				v.normal.xyz = float3 ( norms[0][v.vid], norms[1][v.vid], norms[2][v.vid] );

				v.vertex.xyz = 
					v.vertex.xyz + 
					_HullOffset * v.normal.xyz + 
					sin(_MetamorphoseLevel * PI) * float3 ( norms[0][3], norms[1][3], norms[2][3] ) + 
					_MetamorphoseLevel * saturate(sin(_Time.w + v.vertex.y) * 0.5) * float3 ( norms[0][3], norms[1][3], norms[2][3] );
			
				float4x4 uvs = UNITY_ACCESS_INSTANCED_PROP(Props, _VertexUV);
				v.texcoord.xy =  float2 ( uvs[0][v.vid], uvs[1][v.vid] );
			}


			void surf (Input IN, inout SurfaceOutputStandard o)
			{
				fixed4 colorFront = UNITY_ACCESS_INSTANCED_PROP(Props, _Color);
				fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * lerp(_ColorBack, colorFront, IN.facing);
				o.Albedo = c.rgb;
				o.Emission = ( 1 - IN.facing ) * lerp(_ColorBack, colorFront, IN.facing);
				o.Metallic = UNITY_ACCESS_INSTANCED_PROP(Props, _Metallic);
				o.Smoothness = UNITY_ACCESS_INSTANCED_PROP(Props, _Glossiness);
				o.Alpha = c.a;
			}
		
		ENDCG
	}
	FallBack "Diffuse"
}
