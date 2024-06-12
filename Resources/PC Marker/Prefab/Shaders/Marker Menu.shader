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
	}

	SubShader
	{
		Tags {
			"RenderType" = "Transparent"
			"Queue" = "Geometry+0"
			"VRCFallback" = "Hidden"			
		}
		LOD 100
		Pass
		{
			Cull Off
			CGPROGRAM

			#pragma fragment frag
			#pragma vertex vert
			#pragma multi_compile_fog

            #include "UnityCG.cginc"

			struct appdata
	        {
				float4 vertex : POSITION;
	            float4 vertexColor : COLOR;
	            float2 uv : TEXCOORD0;
        		float2 uv2 : TEXCOORD1;
				UNITY_VERTEX_INPUT_INSTANCE_ID
	        };

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float4 vertexColor : COLOR;
				float2 uv : TEXCOORD0;
        		float2 uv2 : TEXCOORD1;
				UNITY_FOG_COORDS(4)
				UNITY_VERTEX_OUTPUT_STEREO
			};

			UNITY_DECLARE_TEX2D(_SpriteSheet);
			float4 _SpriteSheet_ST;
			float4 _SelectedColor;
			float _PenSelected;
			float _EraserSelected;
			float _SizeSelected;
			float _ColorSelected;
			float _SpaceSelected;
			float _ClearSelected;
			float4 _SliderForegroundColor;
			float4 _SliderBackgroundColor;
			float _SliderValue;
			float4 _BackgroundColor;
			float4 _LoadingBarColor;
			float _Loading;
			float _Space1Selected;
			float _Space2Selected;
			float _Space3Selected;
			float _Space4Selected;
			float _Space5Selected;
			float _Space6Selected;
			float _Space7Selected;
			float _Space8Selected;
			float _Cutoff = 0.5;
			
			float isColorClose(float4 color1, float4 color2)
			{
				return (length( abs( ( color1 - color2 ) ) ) <= 0.005 ) ? 1.0 : 0.0;
			}
			
			v2f vert(appdata v)
			{
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_OUTPUT(v2f, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				o.uv = v.uv;
				o.uv2 = v.uv2;
				o.vertexColor = v.vertexColor;
				o.vertex = UnityObjectToClipPos(v.vertex)
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				float2 uv_SpriteSheet = i.uv * _SpriteSheet_ST.xy + _SpriteSheet_ST.zw;
				float4 Sprite = UNITY_SAMPLE_TEX2D( _SpriteSheet, uv_SpriteSheet );
				float4 SpriteMasked = ( Sprite * Sprite.a );
				float4 SpriteUv2 = UNITY_SAMPLE_TEX2D( _SpriteSheet, i.uv2 );
				
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
				float4 sliderCompletionColor = i.uv.x > _SliderValue  ? _SliderBackgroundColor : _SliderForegroundColor;
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
				float4 loadingBarColor = (i.uv.x < _Loading) ? _LoadingBarColor : loadingBarBackground;
				color += ( isLoadingBar * loadingBarColor );
				
				float spaceColors[] = { 1, 0.8148469, 0.708376, 0.6307572, 0.5271155, 0.4507858, 0.3915725, 0.3049874 };
				bool spaceEnabled[] = { _Space1Selected, _Space2Selected, _Space3Selected, _Space4Selected, _Space5Selected, _Space6Selected, _Space7Selected, _Space8Selected };
				
				for (int j = 0; j < 8; j++) 
				{
					float isSelected = isColorClose(i.vertexColor, float4(spaceColors[j],0,0,1));
					float4 SpriteColor = lerp( ( SpriteMasked + SpriteUv2 ) , ( SpriteMasked + ( SpriteUv2 * _SelectedColor ) ) , spaceEnabled[j]);
					color += ( isSelected * SpriteColor );
				}
				
				clip(color.a - _Cutoff);
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
				UNITY_APPLY_FOG(i.fogCoord, color);
				return color;
			}
			ENDCG
		}
	}
}