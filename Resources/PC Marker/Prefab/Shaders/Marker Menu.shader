Shader "VRLabs/Marker/Marker Menu"
{
	Properties
	{
		_Cutoff( "Mask Clip Value", Float ) = 0.9
		_SpriteSheet("Sprite Sheet", 2D) = "white" {}
		_BackgroundColor("Background Color", Color) = (0.2641509,0.2641509,0.2641509,0)
		_SliderForegroundColor("Slider Foreground Color", Color) = (0.6415094,0.6415094,0.6415094,1)
		_SliderBackgroundColor("Slider Background Color", Color) = (0.3018868,0.3018868,0.3018868,1)
		_LoadingBarColor("Loading Bar Color", Color) = (1,0,0,0.509804)
		[HDR]_SelectedColor("Selected Color", Color) = (0.1223032,1,0,1)
		_Loading("Loading", Range( 0 , 1)) = 0
		_SliderValue("Slider Value", Range( 0 , 1)) = 0.3711113
		[Toggle]_PenSelected("Pen Selected", Float) = 0
		[Toggle]_EraserSelected("Eraser Selected", Float) = 0
		[Toggle]_SizeSelected("Size Selected", Float) = 0
		[Toggle]_ColorSelected("Color Selected", Float) = 0
		[Toggle]_ClearSelected("Clear Selected", Float) = 0
		[Toggle]_SpaceSelected("Space Selected", Float) = 0
		[Toggle]_Space1Selected("Space 1 Selected", Float) = 0
		[Toggle]_Space2Selected("Space 2 Selected", Float) = 0
		[Toggle]_Space3Selected("Space 3 Selected", Float) = 0
		[Toggle]_Space4Selected("Space 4 Selected", Float) = 0
		[Toggle]_Space5Selected("Space 5 Selected", Float) = 0
		[Toggle]_Space6Selected("Space 6 Selected", Float) = 0
		[Toggle]_Space7Selected("Space 7 Selected", Float) = 0
		[Toggle]_Space8Selected("Space 8 Selected", Float) = 0
		[HideInInspector] _texcoord2( "", 2D ) = "white" {}
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Transparent"  "Queue" = "Geometry+0" "IsEmissive" = "true"  }
		Cull Off
		CGPROGRAM
		#pragma target 3.0
		#pragma surface surf Unlit keepalpha addshadow fullforwardshadows 
		struct Input
		{
			float4 vertexColor : COLOR;
			float2 uv_texcoord;
			float2 uv2_texcoord2;
		};

		uniform sampler2D _SpriteSheet;
		uniform float4 _SpriteSheet_ST;
		uniform float4 _SelectedColor;
		uniform float _PenSelected;
		uniform float _EraserSelected;
		uniform float _SizeSelected;
		uniform float _ColorSelected;
		uniform float _SpaceSelected;
		uniform float _ClearSelected;
		uniform float4 _SliderForegroundColor;
		uniform float4 _SliderBackgroundColor;
		uniform float _SliderValue;
		uniform float4 _BackgroundColor;
		uniform float4 _LoadingBarColor;
		uniform float _Loading;
		uniform float _Space1Selected;
		uniform float _Space2Selected;
		uniform float _Space3Selected;
		uniform float _Space4Selected;
		uniform float _Space5Selected;
		uniform float _Space6Selected;
		uniform float _Space7Selected;
		uniform float _Space8Selected;
		uniform float _Cutoff = 0.5;

		inline half4 LightingUnlit( SurfaceOutput s, half3 lightDir, half atten )
		{
			return half4 ( 0, 0, 0, s.Alpha );
		}

		float isColorClose(float4 color1, float4 color2)
		{
			return (length( abs( ( color1 - color2 ) ) ) <= 0.005 ) ? 1.0 : 0.0;
		}
		
		void surf( Input i , inout SurfaceOutput o )
		{
			float2 uv_SpriteSheet = i.uv_texcoord * _SpriteSheet_ST.xy + _SpriteSheet_ST.zw;
			float4 Sprite = tex2D( _SpriteSheet, uv_SpriteSheet );
			float4 SpriteMasked = ( Sprite * Sprite.a );
			float4 SpriteUv2 = tex2D( _SpriteSheet, i.uv2_texcoord2 );
			
			float menuColors[] = {1, 0.8962694, 0.7379107, 0.4677838, 0.3139888};
			bool menuEnabled[] = { _PenSelected, _EraserSelected, _SizeSelected, _SpaceSelected, _ClearSelected };
			float4 color = float4(0,0,0,0);
			for (int j = 0; j < 5; j++)
			{
				float4 selectedColor = lerp( Sprite , ( Sprite * _SelectedColor ) , menuEnabled[j]);
				float isSelected = isColorClose(i.vertexColor, float4(0,menuColors[j],0,1));
				color += ( isSelected * selectedColor );
			}
			
			// Color Button
			float isColorButton = isColorClose(i.vertexColor, float4(0,0.6172068,0,1));
			float4 colorButtonColor = lerp(( SpriteMasked + SpriteUv2 ), ( SpriteMasked + ( SpriteUv2 * _SelectedColor ) ) , _ColorSelected);
			color += (isColorButton * colorButtonColor );
			
			// Slider
			float4 sliderCompletionColor = i.uv_texcoord.x > _SliderValue  ? _SliderBackgroundColor : _SliderForegroundColor;
			float isSlider = isColorClose(i.vertexColor, float4(0,0,1,1));
			color += ( ( sliderCompletionColor * isSlider ));

			float isSliderSprite = isColorClose(i.vertexColor, float4(0,0,0.658375,1));
			color += ( isSliderSprite * Sprite );

			// Background
			float isBackground = isColorClose(i.vertexColor,float4(0,1,1,1));
			color += ( isBackground * _BackgroundColor );

			// Loading Bar
			float isLoadingBar = isColorClose(i.vertexColor, float4( 1,0.35,1,1 )) ;
			float4 loadingBarBackground = float4(0.04193995,0.04193995,0.04193995,0);
			float4 loadingBarColor = (i.uv_texcoord.x < _Loading) ? _LoadingBarColor : loadingBarBackground;
			color += ( isLoadingBar * loadingBarColor );
			
			float spaceColors[] = { 1, 0.8148469, 0.708376, 0.6307572, 0.5271155, 0.4507858, 0.3915725, 0.3049874 };
			bool spaceEnabled[] = { _Space1Selected, _Space2Selected, _Space3Selected, _Space4Selected, _Space5Selected, _Space6Selected, _Space7Selected, _Space8Selected };
			float4 Sprite2 = tex2D( _SpriteSheet, i.uv2_texcoord2 );

			for (int j = 0; j < 8; j++) 
			{
				float isSelected = isColorClose(i.vertexColor, float4(spaceColors[j],0,0,1));
				float4 SpriteColor = lerp( ( SpriteMasked + Sprite2 ) , ( SpriteMasked + ( Sprite2 * _SelectedColor ) ) , spaceEnabled[j]);
				color += ( isSelected * SpriteColor );
			}
			
			o.Emission = color.rgb;
			o.Alpha = 1;
			clip( color.a - _Cutoff );
		}

		ENDCG
	}
	Fallback "Diffuse"
}