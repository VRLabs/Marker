Shader "VRLabs/Marker/Eraser"
{
	Properties
	{
		_Opacity("Opacity", Range( 0 , 1)) = 0
		_EraserSize("Eraser Size", Range( 0 , 5)) = 0
		[HDR]_EraserColor("Eraser Color", Color) = (1,1,1,0)
		[Toggle]_Toggle("Toggle", Float) = 0
	}

	SubShader
	{
		Tags{ "RenderType" = "Transparent"  "Queue" = "Transparent+0" "IgnoreProjector" = "True" "IsEmissive" = "true"  }
		Cull Back
		CGPROGRAM
		#pragma target 3.0
		#pragma surface surf Unlit alpha:fade keepalpha fullforwardshadows vertex:vertexDataFunc 

		struct Input
		{
			half filler;
		};

		uniform float _EraserSize;
		uniform float4 _EraserColor;
		uniform float _Opacity;
		uniform float _Toggle;

		void vertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			v.vertex.xyz += float3(_EraserSize * v.vertex.x, _EraserSize * ( -0.195 + v.vertex.y ), v.vertex.z * _EraserSize );
			v.vertex.w = 1;
		}

		inline half4 LightingUnlit( SurfaceOutput s, half3 lightDir, half atten )
		{
			return half4 ( 0, 0, 0, s.Alpha );
		}

		void surf( Input i , inout SurfaceOutput o )
		{
			o.Emission = _EraserColor.rgb;
			o.Alpha = ( _Opacity * _Toggle );
		}

		ENDCG
	}
	Fallback "Diffuse"
}