// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "VRLabs/Cam/Marker Menu"
{
	Properties
	{
		_Cutoff( "Mask Clip Value", Float ) = 0.5
		_SpriteSheet("Sprite Sheet", 2D) = "white" {}
		_BackgroundColor("Background Color", Color) = (0.2641509,0.2641509,0.2641509,0)
		_SliderForegroundColor("Slider Foreground Color", Color) = (0.6415094,0.6415094,0.6415094,0)
		_SliderBackgroundColor("Slider Background Color", Color) = (0.3018868,0.3018868,0.3018868,0)
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
		Tags{ "RenderType" = "TransparentCutout"  "Queue" = "Geometry+0" "IsEmissive" = "true"  }
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

		void surf( Input i , inout SurfaceOutput o )
		{
			float4 color11 = IsGammaSpace() ? float4(0,1,0,1) : float4(0,1,0,1);
			float ifLocalVar55_g121 = 0;
			if( length( abs( ( i.vertexColor - color11 ) ) ) <= 0.005 )
				ifLocalVar55_g121 = 1.0;
			else
				ifLocalVar55_g121 = 0.0;
			float lerpResult59_g121 = lerp( 0.0 , ifLocalVar55_g121 , 1.0);
			float2 uv_SpriteSheet = i.uv_texcoord * _SpriteSheet_ST.xy + _SpriteSheet_ST.zw;
			float4 SpriteSheet17 = tex2D( _SpriteSheet, uv_SpriteSheet );
			float4 SelectedColor117 = _SelectedColor;
			float4 lerpResult115 = lerp( SpriteSheet17 , ( SpriteSheet17 * SelectedColor117 ) , _PenSelected);
			float4 PenButton119 = ( lerpResult59_g121 * lerpResult115 );
			float4 color12 = IsGammaSpace() ? float4(0,0.9529412,0,1) : float4(0,0.8962694,0,1);
			float ifLocalVar55_g126 = 0;
			if( length( abs( ( i.vertexColor - color12 ) ) ) <= 0.005 )
				ifLocalVar55_g126 = 1.0;
			else
				ifLocalVar55_g126 = 0.0;
			float lerpResult59_g126 = lerp( 0.0 , ifLocalVar55_g126 , 1.0);
			float4 lerpResult127 = lerp( SpriteSheet17 , ( SpriteSheet17 * SelectedColor117 ) , _EraserSelected);
			float4 PenButton2123 = ( lerpResult59_g126 * lerpResult127 );
			float4 color13 = IsGammaSpace() ? float4(0,0.8745099,0,1) : float4(0,0.7379107,0,1);
			float ifLocalVar55_g117 = 0;
			if( length( abs( ( i.vertexColor - color13 ) ) ) <= 0.005 )
				ifLocalVar55_g117 = 1.0;
			else
				ifLocalVar55_g117 = 0.0;
			float lerpResult59_g117 = lerp( 0.0 , ifLocalVar55_g117 , 1.0);
			float4 lerpResult137 = lerp( SpriteSheet17 , ( SpriteSheet17 * SelectedColor117 ) , _SizeSelected);
			float4 PenButton3140 = ( lerpResult59_g117 * lerpResult137 );
			float4 color14 = IsGammaSpace() ? float4(0,0.8078432,0,1) : float4(0,0.6172068,0,1);
			float ifLocalVar55_g115 = 0;
			if( length( abs( ( i.vertexColor - color14 ) ) ) <= 0.005 )
				ifLocalVar55_g115 = 1.0;
			else
				ifLocalVar55_g115 = 0.0;
			float lerpResult59_g115 = lerp( 0.0 , ifLocalVar55_g115 , 1.0);
			float4 temp_output_192_0 = ( SpriteSheet17 * SpriteSheet17.a );
			float4 tex2DNode178 = tex2D( _SpriteSheet, i.uv2_texcoord2 );
			float4 lerpResult149 = lerp( ( temp_output_192_0 + tex2DNode178 ) , ( temp_output_192_0 + ( tex2DNode178 * SelectedColor117 ) ) , _ColorSelected);
			float4 PenButton4152 = ( lerpResult59_g115 * lerpResult149 );
			float4 color15 = IsGammaSpace() ? float4(0,0.7137255,0,1) : float4(0,0.4677838,0,1);
			float ifLocalVar55_g123 = 0;
			if( length( abs( ( i.vertexColor - color15 ) ) ) <= 0.005 )
				ifLocalVar55_g123 = 1.0;
			else
				ifLocalVar55_g123 = 0.0;
			float lerpResult59_g123 = lerp( 0.0 , ifLocalVar55_g123 , 1.0);
			float4 lerpResult159 = lerp( SpriteSheet17 , ( SpriteSheet17 * SelectedColor117 ) , _SpaceSelected);
			float4 PenButton5162 = ( lerpResult59_g123 * lerpResult159 );
			float4 color16 = IsGammaSpace() ? float4(0,0.5960785,0,1) : float4(0,0.3139888,0,1);
			float ifLocalVar55_g128 = 0;
			if( length( abs( ( i.vertexColor - color16 ) ) ) <= 0.005 )
				ifLocalVar55_g128 = 1.0;
			else
				ifLocalVar55_g128 = 0.0;
			float lerpResult59_g128 = lerp( 0.0 , ifLocalVar55_g128 , 1.0);
			float4 lerpResult169 = lerp( SpriteSheet17 , ( SpriteSheet17 * SelectedColor117 ) , _ClearSelected);
			float4 PenButton6172 = ( lerpResult59_g128 * lerpResult169 );
			float2 temp_cast_0 = (( 1.0 - _SliderValue )).xx;
			float2 uv_TexCoord97 = i.uv_texcoord + temp_cast_0;
			float4 lerpResult99 = lerp( _SliderForegroundColor , _SliderBackgroundColor , saturate( (0.0 + (uv_TexCoord97.x - 0.999999) * (1.0 - 0.0) / (1.0 - 0.999999)) ));
			float4 color80 = IsGammaSpace() ? float4(0,0,1,1) : float4(0,0,1,1);
			float ifLocalVar55_g17 = 0;
			if( length( abs( ( i.vertexColor - color80 ) ) ) <= 0.005 )
				ifLocalVar55_g17 = 1.0;
			else
				ifLocalVar55_g17 = 0.0;
			float lerpResult59_g17 = lerp( 0.0 , ifLocalVar55_g17 , 1.0);
			float SliderMask102 = lerpResult59_g17;
			float4 color70 = IsGammaSpace() ? float4(0,0,0.8313726,1) : float4(0,0,0.658375,1);
			float ifLocalVar55_g127 = 0;
			if( length( abs( ( i.vertexColor - color70 ) ) ) <= 0.005 )
				ifLocalVar55_g127 = 1.0;
			else
				ifLocalVar55_g127 = 0.0;
			float lerpResult59_g127 = lerp( 0.0 , ifLocalVar55_g127 , 1.0);
			float4 Slider108 = ( ( lerpResult99 * SliderMask102 ) + ( lerpResult59_g127 * SpriteSheet17 ) );
			float4 color6 = IsGammaSpace() ? float4(0,1,1,1) : float4(0,1,1,1);
			float ifLocalVar55_g129 = 0;
			if( length( abs( ( i.vertexColor - color6 ) ) ) <= 0.005 )
				ifLocalVar55_g129 = 1.0;
			else
				ifLocalVar55_g129 = 0.0;
			float lerpResult59_g129 = lerp( 0.0 , ifLocalVar55_g129 , 1.0);
			float temp_output_75_0 = lerpResult59_g129;
			float4 Background195 = ( temp_output_75_0 * _BackgroundColor );
			float ifLocalVar55_g130 = 0;
			if( length( abs( ( i.vertexColor - float4( 1,0.35,1,1 ) ) ) ) <= 0.005 )
				ifLocalVar55_g130 = 1.0;
			else
				ifLocalVar55_g130 = 0.0;
			float lerpResult59_g130 = lerp( 0.0 , ifLocalVar55_g130 , 1.0);
			float4 color346 = IsGammaSpace() ? float4(0.2264151,0.2264151,0.2264151,0) : float4(0.04193995,0.04193995,0.04193995,0);
			float2 temp_cast_1 = (( 1.0 - _Loading )).xx;
			float2 uv_TexCoord347 = i.uv_texcoord + temp_cast_1;
			float ifLocalVar355 = 0;
			if( uv_TexCoord347.x >= 1.0 )
				ifLocalVar355 = 1.0;
			else
				ifLocalVar355 = 0.0;
			float4 lerpResult345 = lerp( _LoadingBarColor , color346 , saturate( ifLocalVar355 ));
			float4 Load360 = ( lerpResult59_g130 * lerpResult345 );
			float4 color36 = IsGammaSpace() ? float4(1,0,0,1) : float4(1,0,0,1);
			float ifLocalVar55_g124 = 0;
			if( length( abs( ( i.vertexColor - color36 ) ) ) <= 0.005 )
				ifLocalVar55_g124 = 1.0;
			else
				ifLocalVar55_g124 = 0.0;
			float lerpResult59_g124 = lerp( 0.0 , ifLocalVar55_g124 , 1.0);
			float4 temp_output_211_0 = ( SpriteSheet17 * SpriteSheet17.a );
			float4 tex2DNode207 = tex2D( _SpriteSheet, i.uv2_texcoord2 );
			float4 lerpResult217 = lerp( ( temp_output_211_0 + tex2DNode207 ) , ( temp_output_211_0 + ( tex2DNode207 * SelectedColor117 ) ) , _Space1Selected);
			float4 SpaceButton1219 = ( lerpResult59_g124 * lerpResult217 );
			float4 color37 = IsGammaSpace() ? float4(0.9137256,0,0,1) : float4(0.8148469,0,0,1);
			float ifLocalVar55_g116 = 0;
			if( length( abs( ( i.vertexColor - color37 ) ) ) <= 0.005 )
				ifLocalVar55_g116 = 1.0;
			else
				ifLocalVar55_g116 = 0.0;
			float lerpResult59_g116 = lerp( 0.0 , ifLocalVar55_g116 , 1.0);
			float4 temp_output_229_0 = ( SpriteSheet17 * SpriteSheet17.a );
			float4 tex2DNode225 = tex2D( _SpriteSheet, i.uv2_texcoord2 );
			float4 lerpResult235 = lerp( ( temp_output_229_0 + tex2DNode225 ) , ( temp_output_229_0 + ( tex2DNode225 * SelectedColor117 ) ) , _Space2Selected);
			float4 SpaceButton2237 = ( lerpResult59_g116 * lerpResult235 );
			float4 color38 = IsGammaSpace() ? float4(0.8588236,0,0,1) : float4(0.708376,0,0,1);
			float ifLocalVar55_g122 = 0;
			if( length( abs( ( i.vertexColor - color38 ) ) ) <= 0.005 )
				ifLocalVar55_g122 = 1.0;
			else
				ifLocalVar55_g122 = 0.0;
			float lerpResult59_g122 = lerp( 0.0 , ifLocalVar55_g122 , 1.0);
			float4 temp_output_245_0 = ( SpriteSheet17 * SpriteSheet17.a );
			float4 tex2DNode241 = tex2D( _SpriteSheet, i.uv2_texcoord2 );
			float4 lerpResult251 = lerp( ( temp_output_245_0 + tex2DNode241 ) , ( temp_output_245_0 + ( tex2DNode241 * SelectedColor117 ) ) , _Space3Selected);
			float4 SpaceButton3253 = ( lerpResult59_g122 * lerpResult251 );
			float4 color40 = IsGammaSpace() ? float4(0.8156863,0,0,1) : float4(0.6307572,0,0,1);
			float ifLocalVar55_g118 = 0;
			if( length( abs( ( i.vertexColor - color40 ) ) ) <= 0.005 )
				ifLocalVar55_g118 = 1.0;
			else
				ifLocalVar55_g118 = 0.0;
			float lerpResult59_g118 = lerp( 0.0 , ifLocalVar55_g118 , 1.0);
			float4 temp_output_261_0 = ( SpriteSheet17 * SpriteSheet17.a );
			float4 tex2DNode257 = tex2D( _SpriteSheet, i.uv2_texcoord2 );
			float4 lerpResult267 = lerp( ( temp_output_261_0 + tex2DNode257 ) , ( temp_output_261_0 + ( tex2DNode257 * SelectedColor117 ) ) , _Space4Selected);
			float4 SpaceButton4269 = ( lerpResult59_g118 * lerpResult267 );
			float4 color39 = IsGammaSpace() ? float4(0.7529413,0,0,1) : float4(0.5271155,0,0,1);
			float ifLocalVar55_g120 = 0;
			if( length( abs( ( i.vertexColor - color39 ) ) ) <= 0.005 )
				ifLocalVar55_g120 = 1.0;
			else
				ifLocalVar55_g120 = 0.0;
			float lerpResult59_g120 = lerp( 0.0 , ifLocalVar55_g120 , 1.0);
			float4 temp_output_277_0 = ( SpriteSheet17 * SpriteSheet17.a );
			float4 tex2DNode273 = tex2D( _SpriteSheet, i.uv2_texcoord2 );
			float4 lerpResult283 = lerp( ( temp_output_277_0 + tex2DNode273 ) , ( temp_output_277_0 + ( tex2DNode273 * SelectedColor117 ) ) , _Space5Selected);
			float4 SpaceButton5285 = ( lerpResult59_g120 * lerpResult283 );
			float4 color41 = IsGammaSpace() ? float4(0.7019608,0,0,1) : float4(0.4507858,0,0,1);
			float ifLocalVar55_g125 = 0;
			if( length( abs( ( i.vertexColor - color41 ) ) ) <= 0.005 )
				ifLocalVar55_g125 = 1.0;
			else
				ifLocalVar55_g125 = 0.0;
			float lerpResult59_g125 = lerp( 0.0 , ifLocalVar55_g125 , 1.0);
			float4 temp_output_293_0 = ( SpriteSheet17 * SpriteSheet17.a );
			float4 tex2DNode289 = tex2D( _SpriteSheet, i.uv2_texcoord2 );
			float4 lerpResult299 = lerp( ( temp_output_293_0 + tex2DNode289 ) , ( temp_output_293_0 + ( tex2DNode289 * SelectedColor117 ) ) , _Space6Selected);
			float4 SpaceButton6301 = ( lerpResult59_g125 * lerpResult299 );
			float4 color42 = IsGammaSpace() ? float4(0.6588235,0,0,1) : float4(0.3915725,0,0,1);
			float ifLocalVar55_g114 = 0;
			if( length( abs( ( i.vertexColor - color42 ) ) ) <= 0.005 )
				ifLocalVar55_g114 = 1.0;
			else
				ifLocalVar55_g114 = 0.0;
			float lerpResult59_g114 = lerp( 0.0 , ifLocalVar55_g114 , 1.0);
			float4 temp_output_309_0 = ( SpriteSheet17 * SpriteSheet17.a );
			float4 tex2DNode305 = tex2D( _SpriteSheet, i.uv2_texcoord2 );
			float4 lerpResult315 = lerp( ( temp_output_309_0 + tex2DNode305 ) , ( temp_output_309_0 + ( tex2DNode305 * SelectedColor117 ) ) , _Space7Selected);
			float4 SpaceButton7317 = ( lerpResult59_g114 * lerpResult315 );
			float4 color43 = IsGammaSpace() ? float4(0.5882353,0,0,1) : float4(0.3049874,0,0,1);
			float ifLocalVar55_g119 = 0;
			if( length( abs( ( i.vertexColor - color43 ) ) ) <= 0.005 )
				ifLocalVar55_g119 = 1.0;
			else
				ifLocalVar55_g119 = 0.0;
			float lerpResult59_g119 = lerp( 0.0 , ifLocalVar55_g119 , 1.0);
			float4 temp_output_325_0 = ( SpriteSheet17 * SpriteSheet17.a );
			float4 tex2DNode321 = tex2D( _SpriteSheet, i.uv2_texcoord2 );
			float4 lerpResult331 = lerp( ( temp_output_325_0 + tex2DNode321 ) , ( temp_output_325_0 + ( tex2DNode321 * SelectedColor117 ) ) , _Space8Selected);
			float4 SpaceButton8333 = ( lerpResult59_g119 * lerpResult331 );
			float4 temp_output_69_0 = ( ( PenButton119 + PenButton2123 + PenButton3140 + PenButton4152 + PenButton5162 + PenButton6172 ) + Slider108 + Background195 + Load360 + ( SpaceButton1219 + SpaceButton2237 + SpaceButton3253 + SpaceButton4269 + SpaceButton5285 + SpaceButton6301 + SpaceButton7317 + SpaceButton8333 ) );
			o.Emission = temp_output_69_0.rgb;
			o.Alpha = 1;
			float BackgroundAlpha197 = temp_output_75_0;
			clip( saturate( ( temp_output_69_0.a + SliderMask102 + BackgroundAlpha197 ) ) - _Cutoff );
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=18935
307;247;2047;1124;1484.681;-1536.847;1;True;False
Node;AmplifyShaderEditor.CommentaryNode;121;-2123.291,-1307.014;Inherit;False;575.8082;503.0624;Comment;4;117;114;17;2;Registers;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;193;-3707.136,-1317.159;Inherit;False;1488.985;3892.361;Comment;6;153;116;132;163;142;143;Main Menu;1,1,1,1;0;0
Node;AmplifyShaderEditor.SamplerNode;2;-2073.291,-1256.609;Inherit;True;Property;_SpriteSheet;Sprite Sheet;1;0;Create;True;0;0;0;False;0;False;-1;None;4038eea094c413e4195c2459b5a0d45b;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.CommentaryNode;201;-1082.341,-1238.604;Inherit;False;1918.096;1045.964;Comment;19;108;203;72;73;71;70;102;82;80;100;99;101;95;110;96;104;97;107;105;Slider;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;222;-3534.12,3680.081;Inherit;False;1320.432;705.8389;Comment;15;237;236;235;234;233;231;230;229;228;227;226;225;224;223;37;Color Button;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;204;-3502.001,2847.451;Inherit;False;1320.432;705.8389;Comment;15;219;218;217;216;215;214;213;211;210;209;208;207;206;205;36;Color Button;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;318;-1969.166,5903.227;Inherit;False;1320.432;705.8389;Comment;15;333;332;331;330;329;327;326;325;324;323;322;321;320;319;43;Color Button;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;270;-1954.747,3308.049;Inherit;False;1320.432;705.8389;Comment;15;285;284;283;282;281;279;278;277;276;275;274;273;272;271;39;Color Button;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;238;-3557.072,4512.078;Inherit;False;1320.432;705.8389;Comment;15;253;252;251;250;249;247;246;245;244;243;242;241;240;239;38;Color Button;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;361;-1197.676,1782.474;Inherit;False;1774.484;729;Comment;13;356;347;355;346;343;345;344;360;357;365;350;366;367;Loading Bar;1,1,1,1;0;0
Node;AmplifyShaderEditor.ColorNode;114;-2013.411,-1020.505;Inherit;False;Property;_SelectedColor;Selected Color;6;1;[HDR];Create;True;0;0;0;False;0;False;0.1223032,1,0,1;0,1,0.7190266,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.CommentaryNode;254;-3569.526,5344.544;Inherit;False;1320.432;705.8389;Comment;14;269;268;267;266;265;263;262;261;260;259;258;257;256;255;Color Button;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;302;-1961.956,5085.024;Inherit;False;1320.432;705.8389;Comment;15;317;316;315;314;313;311;310;309;308;307;306;305;304;303;42;Color Button;1,1,1,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;105;-1032.341,-896.7032;Inherit;False;Property;_SliderValue;Slider Value;8;0;Create;True;0;0;0;False;0;False;0.3711113;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;286;-1943.934,4183.922;Inherit;False;1320.432;705.8389;Comment;15;301;300;299;298;297;295;294;293;292;291;290;289;288;287;41;Color Button;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;143;-3639.793,1822.327;Inherit;False;1320.432;705.8389;Comment;15;177;178;152;151;14;150;149;192;187;186;148;146;145;191;144;Color Button;1,1,1,1;0;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;17;-1772.609,-1257.014;Inherit;False;SpriteSheet;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;287;-1826.076,4409.125;Inherit;False;17;SpriteSheet;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;117;-1748.72,-1021.17;Inherit;False;SelectedColor;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;144;-3521.935,2047.53;Inherit;False;17;SpriteSheet;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;288;-1927.728,4647.116;Inherit;False;1;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TextureCoordinatesNode;177;-3623.587,2285.521;Inherit;False;1;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TextureCoordinatesNode;256;-3553.32,5807.738;Inherit;False;1;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TextureCoordinatesNode;272;-1938.541,3771.243;Inherit;False;1;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;223;-3416.262,3905.284;Inherit;False;17;SpriteSheet;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;205;-3485.795,3310.645;Inherit;False;1;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TextureCoordinatesNode;304;-1945.75,5548.219;Inherit;False;1;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TextureCoordinatesNode;224;-3517.914,4143.274;Inherit;False;1;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;350;-1131.676,2076.474;Inherit;False;Property;_Loading;Loading;7;0;Create;True;0;0;0;False;0;False;0;0.764;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;240;-3540.866,4975.272;Inherit;False;1;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;319;-1851.308,6128.43;Inherit;False;17;SpriteSheet;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;206;-3384.143,3072.654;Inherit;False;17;SpriteSheet;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;271;-1836.89,3533.251;Inherit;False;17;SpriteSheet;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;303;-1844.099,5310.228;Inherit;False;17;SpriteSheet;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;239;-3439.214,4737.281;Inherit;False;17;SpriteSheet;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;320;-1952.96,6366.421;Inherit;False;1;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.OneMinusNode;107;-751.8335,-892.85;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;255;-3451.668,5569.747;Inherit;False;17;SpriteSheet;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.CommentaryNode;116;-3655.48,-1201.254;Inherit;False;1154.924;493.7314;Comment;9;119;118;7;3;115;113;11;111;18;Pen Button;1,1,1,1;0;0
Node;AmplifyShaderEditor.BreakToComponentsNode;274;-1638.294,3595.926;Inherit;False;COLOR;1;0;COLOR;0,0,0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.SamplerNode;289;-1715.376,4618.481;Inherit;True;Property;_TextureSample5;Texture Sample 5;1;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Instance;2;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;273;-1726.19,3742.608;Inherit;True;Property;_TextureSample4;Texture Sample 4;1;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Instance;2;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;305;-1733.399,5519.584;Inherit;True;Property;_TextureSample6;Texture Sample 6;1;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Instance;2;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.CommentaryNode;142;-3651.64,-64.8255;Inherit;False;1083.432;530.1389;Comment;9;13;133;134;135;137;138;139;140;136;Size Button;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;132;-3655.113,-649.6738;Inherit;False;1145.833;533.3392;Comment;9;123;129;131;130;128;127;21;20;12;Eraser Button;1,1,1,1;0;0
Node;AmplifyShaderEditor.BreakToComponentsNode;191;-3323.339,2110.204;Inherit;False;COLOR;1;0;COLOR;0,0,0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.SamplerNode;241;-3328.514,4946.638;Inherit;True;Property;_TextureSample2;Texture Sample 2;1;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Instance;2;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.CommentaryNode;163;-3645.87,1195.778;Inherit;False;1172.909;518.2964;Comment;9;172;171;164;169;170;168;166;16;165;Clear Button;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;153;-3655.925,576.874;Inherit;False;1083.432;530.1389;Comment;9;162;161;160;159;158;156;155;154;15;Space Button;1,1,1,1;0;0
Node;AmplifyShaderEditor.ColorNode;80;-1022.619,-385.4397;Inherit;False;Constant;_Slider;Slider;2;0;Create;True;0;0;0;False;0;False;0,0,1,1;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.BreakToComponentsNode;226;-3217.666,3967.958;Inherit;False;COLOR;1;0;COLOR;0,0,0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.GetLocalVarNode;145;-3321.594,2448.796;Inherit;False;117;SelectedColor;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;243;-3238.873,5138.546;Inherit;False;117;SelectedColor;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;225;-3305.562,4114.639;Inherit;True;Property;_TextureSample1;Texture Sample 1;1;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Instance;2;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.BreakToComponentsNode;242;-3240.618,4799.955;Inherit;False;COLOR;1;0;COLOR;0,0,0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.BreakToComponentsNode;208;-3185.547,3135.328;Inherit;False;COLOR;1;0;COLOR;0,0,0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.GetLocalVarNode;307;-1643.758,5711.493;Inherit;False;117;SelectedColor;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;259;-3251.328,5971.012;Inherit;False;117;SelectedColor;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;178;-3411.235,2256.886;Inherit;True;Property;_UV2;UV2;1;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Instance;2;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.BreakToComponentsNode;258;-3253.073,5632.421;Inherit;False;COLOR;1;0;COLOR;0,0,0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.GetLocalVarNode;275;-1636.548,3934.517;Inherit;False;117;SelectedColor;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;207;-3273.443,3282.01;Inherit;True;Property;_TextureSample0;Texture Sample 0;1;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Instance;2;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;323;-1650.967,6529.695;Inherit;False;117;SelectedColor;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;257;-3340.969,5779.104;Inherit;True;Property;_TextureSample3;Texture Sample 3;1;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Instance;2;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.BreakToComponentsNode;306;-1645.503,5372.901;Inherit;False;COLOR;1;0;COLOR;0,0,0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.GetLocalVarNode;291;-1625.735,4810.39;Inherit;False;117;SelectedColor;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;97;-561.6581,-824.6471;Inherit;True;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0.83,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;227;-3215.921,4306.548;Inherit;False;117;SelectedColor;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.BreakToComponentsNode;322;-1652.712,6191.104;Inherit;False;COLOR;1;0;COLOR;0,0,0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.SamplerNode;321;-1740.608,6337.787;Inherit;True;Property;_TextureSample7;Texture Sample 7;1;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Instance;2;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.BreakToComponentsNode;290;-1627.48,4471.799;Inherit;False;COLOR;1;0;COLOR;0,0,0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.OneMinusNode;366;-1111.878,2182.451;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;209;-3183.802,3473.919;Inherit;False;117;SelectedColor;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;357;-871.6758,2395.475;Inherit;False;Constant;_0;0;22;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;134;-3601.64,312.3432;Inherit;False;117;SelectedColor;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;245;-3109.618,4742.955;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;293;-1496.48,4414.799;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;261;-3122.073,5575.421;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;154;-3594.067,833.0765;Inherit;False;17;SpriteSheet;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;131;-3566.055,-385.4713;Inherit;False;17;SpriteSheet;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;229;-3086.666,3910.958;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;18;-3579.129,-964.4583;Inherit;False;17;SpriteSheet;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;260;-3020.019,5827.683;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;118;-3590.987,-843.4919;Inherit;False;117;SelectedColor;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;211;-3054.547,3078.328;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.TFHCRemapNode;104;-349.6289,-809.1522;Inherit;True;5;0;FLOAT;0;False;1;FLOAT;0.999999;False;2;FLOAT;1;False;3;FLOAT;0;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;133;-3589.782,191.3769;Inherit;False;17;SpriteSheet;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;210;-2952.493,3330.59;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;325;-1521.712,6134.104;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;244;-3007.564,4995.218;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;164;-3584.012,1451.981;Inherit;False;17;SpriteSheet;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;148;-3090.285,2305.466;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;82;-749.6191,-409.4397;Inherit;False;Vertex Color Picker;-1;;17;67fddad07bd5ca74290a1d9d00adbc35;0;2;61;FLOAT;1;False;2;COLOR;1,0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;277;-1507.294,3538.925;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;309;-1514.503,5315.901;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;130;-3577.913,-264.5049;Inherit;False;117;SelectedColor;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;347;-953.6759,2197.474;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0.45,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;276;-1405.239,3791.188;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;192;-3192.339,2053.204;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;228;-2984.612,4163.219;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;324;-1419.658,6386.367;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;292;-1394.426,4667.062;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;155;-3605.925,954.0428;Inherit;False;117;SelectedColor;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;165;-3595.87,1572.947;Inherit;False;117;SelectedColor;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;356;-876.6758,2322.475;Inherit;False;Constant;_1;1;22;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;308;-1412.448,5568.164;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;168;-3349.362,1505.918;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;230;-2810.666,4022.958;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;249;-2846.366,5001.518;Inherit;False;Property;_Space3Selected;Space 3 Selected;17;1;[Toggle];Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;136;-3376.132,349.3133;Inherit;False;Property;_SizeSelected;Size Selected;11;1;[Toggle];Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;129;-3352.405,-227.5347;Inherit;False;Property;_EraserSelected;Eraser Selected;10;1;[Toggle];Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;279;-1236.294,3532.925;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;113;-3344.48,-910.5218;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;158;-3359.417,887.013;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;278;-1231.294,3650.926;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;13;-3501.744,-0.7602015;Inherit;False;Constant;_Button3;Button 3;2;0;Create;True;0;0;0;False;0;False;0,0.8745099,0,1;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;11;-3584.771,-1146.367;Inherit;False;Constant;_Button1;Button 1;2;0;Create;True;0;0;0;False;0;False;0,1,0,1;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;128;-3331.405,-331.5348;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;16;-3588.937,1240.094;Inherit;False;Constant;_Button6;Button 6;2;0;Create;True;0;0;0;False;0;False;0,0.5960785,0,1;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.CommentaryNode;202;-2126.879,-683.0515;Inherit;False;958.2195;374.5294;Comment;6;197;195;76;75;78;6;Background;1,1,1,1;0;0
Node;AmplifyShaderEditor.ColorNode;37;-3164.327,3740.858;Inherit;False;Constant;_SpaceButton2;Space Button 2;2;0;Create;True;0;0;0;False;0;False;0.9137256,0,0,1;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;329;-1257.237,6392.667;Inherit;False;Property;_Space8Selected;Space 8 Selected;22;1;[Toggle];Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;111;-3365.48,-806.5218;Inherit;False;Property;_PenSelected;Pen Selected;9;1;[Toggle];Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;187;-2921.339,2047.204;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;146;-2950.087,2367.766;Inherit;False;Property;_ColorSelected;Color Selected;12;1;[Toggle];Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;310;-1238.503,5427.901;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;102;-502.8481,-400.6406;Inherit;False;SliderMask;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;14;-3156.009,1876.734;Inherit;False;Constant;_Button4;Button 4;2;0;Create;True;0;0;0;False;0;False;0,0.8078432,0,1;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;295;-1225.48,4408.799;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;70;-236.1212,-538.2613;Inherit;False;Constant;_VRLabsIcon;VRLabs Icon;2;0;Create;True;0;0;0;False;0;False;0,0,0.8313726,1;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;265;-2858.82,5833.983;Inherit;False;Property;_Space4Selected;Space 4 Selected;18;1;[Toggle];Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;135;-3355.132,245.3134;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;231;-2815.666,3904.958;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;263;-2851.072,5569.421;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;214;-2791.295,3336.89;Inherit;False;Property;_Space1Selected;Space 1 Selected;15;1;[Toggle];Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;166;-3370.362,1609.917;Inherit;False;Property;_ClearSelected;Clear Selected;13;1;[Toggle];Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;41;-1555.285,4241.753;Inherit;False;Constant;_SpaceButton6;Space Button 6;2;0;Create;True;0;0;0;False;0;False;0.7019608,0,0,1;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;213;-2778.547,3190.328;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;40;-3200.638,5391.757;Inherit;False;Constant;_SpaceButton4;Space Button 4;2;0;Create;True;0;0;0;False;0;False;0.8156863,0,0,1;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;294;-1220.48,4526.799;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;96;-314.4492,-1007.9;Inherit;False;Property;_SliderForegroundColor;Slider Foreground Color;3;0;Create;True;0;0;0;False;0;False;0.6415094,0.6415094,0.6415094,0;0.5294118,0.5294118,0.5294118,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;327;-1250.712,6128.104;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;246;-2833.618,4854.955;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SaturateNode;110;-71.79638,-807.1866;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;247;-2838.618,4736.955;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;215;-2783.547,3072.328;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;36;-3306.476,2889.184;Inherit;False;Constant;_SpaceButton1;Space Button 1;2;0;Create;True;0;0;0;False;0;False;1,0,0,1;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;186;-2916.339,2165.204;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;313;-1251.25,5574.464;Inherit;False;Property;_Space7Selected;Space 7 Selected;21;1;[Toggle];Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;12;-3510.125,-572.2867;Inherit;False;Constant;_Button2;Button 2;2;0;Create;True;0;0;0;False;0;False;0,0.9529412,0,1;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;311;-1243.503,5309.901;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;39;-1556.548,3351.946;Inherit;False;Constant;_SpaceButton5;Space Button 5;2;0;Create;True;0;0;0;False;0;False;0.7529413,0,0,1;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;233;-2823.414,4169.519;Inherit;False;Property;_Space2Selected;Space 2 Selected;16;1;[Toggle];Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;42;-1572.891,5145.424;Inherit;False;Constant;_SpaceButton7;Space Button 7;2;0;Create;True;0;0;0;False;0;False;0.6588235,0,0,1;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;38;-3165.893,4565.587;Inherit;False;Constant;_SpaceButton3;Space Button 3;2;0;Create;True;0;0;0;False;0;False;0.8588236,0,0,1;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ConditionalIfNode;355;-690.6758,2238.474;Inherit;False;False;5;0;FLOAT;0;False;1;FLOAT;1;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;262;-2846.072,5687.421;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;156;-3380.417,991.0122;Inherit;False;Property;_SpaceSelected;Space Selected;14;1;[Toggle];Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;297;-1233.228,4673.361;Inherit;False;Property;_Space6Selected;Space 6 Selected;20;1;[Toggle];Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;15;-3562.569,637.0641;Inherit;False;Constant;_Button5;Button 5;2;0;Create;True;0;0;0;False;0;False;0,0.7137255,0,1;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;43;-1584.667,5955.653;Inherit;False;Constant;_SpaceButton8;Space Button 8;2;0;Create;True;0;0;0;False;0;False;0.5882353,0,0,1;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;95;-318.0103,-1188.604;Inherit;False;Property;_SliderBackgroundColor;Slider Background Color;4;0;Create;True;0;0;0;False;0;False;0.3018868,0.3018868,0.3018868,0;0.3584903,0.3584903,0.3584903,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;326;-1245.712,6246.104;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;281;-1244.041,3797.488;Inherit;False;Property;_Space5Selected;Space 5 Selected;19;1;[Toggle];Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;346;-771.6759,2003.474;Inherit;False;Constant;_Color0;Color 0;21;0;Create;True;0;0;0;False;0;False;0.2264151,0.2264151,0.2264151,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;169;-3134.162,1401.018;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;250;-2836.968,4559.078;Inherit;False;Vertex Color Picker;-1;;122;67fddad07bd5ca74290a1d9d00adbc35;0;2;61;FLOAT;1;False;2;COLOR;1,0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;251;-2652.365,4785.316;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;101;144.5466,-711.754;Inherit;False;102;SliderMask;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;159;-3144.217,782.1129;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;160;-3291.82,626.8738;Inherit;False;Vertex Color Picker;-1;;123;67fddad07bd5ca74290a1d9d00adbc35;0;2;61;FLOAT;1;False;2;COLOR;1,0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;216;-2781.897,2894.451;Inherit;False;Vertex Color Picker;-1;;124;67fddad07bd5ca74290a1d9d00adbc35;0;2;61;FLOAT;1;False;2;COLOR;1,0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;71;-16.31581,-559.4285;Inherit;False;Vertex Color Picker;-1;;127;67fddad07bd5ca74290a1d9d00adbc35;0;2;61;FLOAT;1;False;2;COLOR;1,0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;315;-1057.25,5358.263;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;149;-2735.086,2095.566;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;298;-1223.83,4230.922;Inherit;False;Vertex Color Picker;-1;;125;67fddad07bd5ca74290a1d9d00adbc35;0;2;61;FLOAT;1;False;2;COLOR;1,0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;20;-3263.807,-591.6738;Inherit;False;Vertex Color Picker;-1;;126;67fddad07bd5ca74290a1d9d00adbc35;0;2;61;FLOAT;1;False;2;COLOR;1,0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;73;-7.62562,-473.4947;Inherit;False;17;SpriteSheet;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;170;-3281.765,1245.778;Inherit;False;Vertex Color Picker;-1;;128;67fddad07bd5ca74290a1d9d00adbc35;0;2;61;FLOAT;1;False;2;COLOR;1,0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;3;-3258.338,-1151.254;Inherit;False;Vertex Color Picker;-1;;121;67fddad07bd5ca74290a1d9d00adbc35;0;2;61;FLOAT;1;False;2;COLOR;1,0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;365;-497.2476,2222.557;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;282;-1234.643,3355.049;Inherit;False;Vertex Color Picker;-1;;120;67fddad07bd5ca74290a1d9d00adbc35;0;2;61;FLOAT;1;False;2;COLOR;1,0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;283;-1050.04,3581.287;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;137;-3139.932,140.4135;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;217;-2597.294,3120.689;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;235;-2629.413,3953.319;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;314;-1241.852,5132.024;Inherit;False;Vertex Color Picker;-1;;114;67fddad07bd5ca74290a1d9d00adbc35;0;2;61;FLOAT;1;False;2;COLOR;1,0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;127;-3116.206,-436.4348;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;330;-1249.062,5950.227;Inherit;False;Vertex Color Picker;-1;;119;67fddad07bd5ca74290a1d9d00adbc35;0;2;61;FLOAT;1;False;2;COLOR;1,0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;331;-1064.459,6176.465;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;115;-3134.48,-998.5218;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;299;-1039.227,4457.16;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;150;-2919.689,1869.327;Inherit;False;Vertex Color Picker;-1;;115;67fddad07bd5ca74290a1d9d00adbc35;0;2;61;FLOAT;1;False;2;COLOR;1,0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;99;92.18583,-968.8713;Inherit;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;267;-2664.819,5617.782;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;234;-2814.016,3727.081;Inherit;False;Vertex Color Picker;-1;;116;67fddad07bd5ca74290a1d9d00adbc35;0;2;61;FLOAT;1;False;2;COLOR;1,0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;6;-2076.878,-615.4429;Inherit;False;Constant;_Background;Background;2;0;Create;True;0;0;0;False;0;False;0,1,1,1;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;343;-763.6759,1832.474;Inherit;False;Property;_LoadingBarColor;Loading Bar Color;5;0;Create;True;0;0;0;False;0;False;1,0,0,0.509804;1,0,0,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.FunctionNode;138;-3287.535,-14.82551;Inherit;False;Vertex Color Picker;-1;;117;67fddad07bd5ca74290a1d9d00adbc35;0;2;61;FLOAT;1;False;2;COLOR;1,0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;266;-2849.422,5391.544;Inherit;False;Vertex Color Picker;-1;;118;67fddad07bd5ca74290a1d9d00adbc35;0;2;61;FLOAT;1;False;2;COLOR;1,0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;139;-2955.041,31.7093;Inherit;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;236;-2537.523,3752.616;Inherit;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;332;-972.5687,5975.762;Inherit;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;100;361.629,-828.8566;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;7;-2905.556,-1078.418;Inherit;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;75;-1840.432,-633.0518;Inherit;False;Vertex Color Picker;-1;;129;67fddad07bd5ca74290a1d9d00adbc35;0;2;61;FLOAT;1;False;2;COLOR;1,0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;284;-958.1505,3380.584;Inherit;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;171;-2944.007,1305.472;Inherit;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;21;-2931.315,-545.1389;Inherit;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;300;-947.3372,4256.457;Inherit;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;218;-2505.404,2919.986;Inherit;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;161;-2959.326,673.4088;Inherit;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;72;223.3159,-523.4986;Inherit;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;78;-1854.201,-507.8545;Inherit;False;Property;_BackgroundColor;Background Color;2;0;Create;True;0;0;0;False;0;False;0.2641509,0.2641509,0.2641509,0;0.3773582,0.3773582,0.3773582,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;252;-2560.475,4584.613;Inherit;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;367;-309.5726,1889.249;Inherit;False;Vertex Color Picker;-1;;130;67fddad07bd5ca74290a1d9d00adbc35;0;2;61;FLOAT;1;False;2;COLOR;1,0.35,1,1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;316;-965.3597,5157.56;Inherit;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;151;-2643.196,1894.862;Inherit;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;345;-334.1754,2061.174;Inherit;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;268;-2572.929,5417.079;Inherit;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;152;-2501.363,1892.442;Inherit;False;PenButton4;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;162;-2796.494,667.9896;Inherit;False;PenButton5;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;76;-1595.201,-552.8542;Inherit;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;119;-2696.108,-1027.409;Inherit;False;PenButton;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;301;-805.5042,4254.036;Inherit;False;SpaceButton6;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;123;-2768.482,-550.5583;Inherit;False;PenButton2;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;269;-2431.096,5414.658;Inherit;False;SpaceButton4;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;333;-830.7358,5973.341;Inherit;False;SpaceButton8;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;203;514.6572,-648.1145;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;237;-2395.69,3750.195;Inherit;False;SpaceButton2;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;344;-32.17542,1986.174;Inherit;True;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;172;-2704.854,1285.578;Inherit;False;PenButton6;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;219;-2363.571,2917.565;Inherit;False;SpaceButton1;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;285;-815.3176,3378.162;Inherit;False;SpaceButton5;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;140;-2792.208,26.28999;Inherit;False;PenButton3;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;317;-823.5267,5155.139;Inherit;False;SpaceButton7;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;253;-2418.642,4582.192;Inherit;False;SpaceButton3;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;220;-578.481,1026.258;Inherit;False;219;SpaceButton1;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;334;-570.0543,1107.792;Inherit;False;237;SpaceButton2;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;120;-612.278,487.5804;Inherit;False;119;PenButton;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;338;-592,1433;Inherit;False;301;SpaceButton6;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;335;-576.0543,1187.792;Inherit;False;253;SpaceButton3;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;174;-614.4411,795.6821;Inherit;False;162;PenButton5;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;340;-588.0543,1594.792;Inherit;False;333;SpaceButton8;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;195;-1349.873,-543.9817;Inherit;False;Background;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;141;-615.6696,644.7502;Inherit;False;140;PenButton3;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;108;646.9548,-651.548;Inherit;False;Slider;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;336;-594.0543,1274.792;Inherit;False;269;SpaceButton4;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;173;-616.4411,718.6821;Inherit;False;152;PenButton4;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;339;-597.0543,1510.792;Inherit;False;317;SpaceButton7;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;337;-597.0543,1348.792;Inherit;False;285;SpaceButton5;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;122;-616.1854,566.6724;Inherit;False;123;PenButton2;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;360;268.8083,1902.337;Inherit;False;Load;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;175;-613.4411,869.6821;Inherit;False;172;PenButton6;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;363;-275.0387,1046.863;Inherit;False;360;Load;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;196;-289.861,980.8474;Inherit;False;195;Background;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;68;-339.9893,1218.16;Inherit;False;8;8;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;5;COLOR;0,0,0,0;False;6;COLOR;0,0,0,0;False;7;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;19;-318.0474,647.8187;Inherit;False;6;6;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;5;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;109;-283.3787,909.0123;Inherit;False;108;Slider;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;69;-10.54129,924.0626;Inherit;False;5;5;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;197;-1444.873,-630.9817;Inherit;False;BackgroundAlpha;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.BreakToComponentsNode;35;264.4053,889.0346;Inherit;False;COLOR;1;0;COLOR;0,0,0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.GetLocalVarNode;198;194.139,1110.847;Inherit;False;197;BackgroundAlpha;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;194;231.139,1033.847;Inherit;False;102;SliderMask;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode;199;99.4043,714.2512;Inherit;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;74;482.2756,1012.861;Inherit;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode;200;118.4043,711.2512;Inherit;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SaturateNode;89;642.77,1031.548;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;1318.409,617.5735;Float;False;True;-1;2;ASEMaterialInspector;0;0;Unlit;VRLabs/Cam/Marker Menu;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Off;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Custom;0.5;True;True;0;True;TransparentCutout;;Geometry;All;18;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;True;Relative;0;;0;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;False;15;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;17;0;2;0
WireConnection;117;0;114;0
WireConnection;107;0;105;0
WireConnection;274;0;271;0
WireConnection;289;1;288;0
WireConnection;273;1;272;0
WireConnection;305;1;304;0
WireConnection;191;0;144;0
WireConnection;241;1;240;0
WireConnection;226;0;223;0
WireConnection;225;1;224;0
WireConnection;242;0;239;0
WireConnection;208;0;206;0
WireConnection;178;1;177;0
WireConnection;258;0;255;0
WireConnection;207;1;205;0
WireConnection;257;1;256;0
WireConnection;306;0;303;0
WireConnection;97;1;107;0
WireConnection;322;0;319;0
WireConnection;321;1;320;0
WireConnection;290;0;287;0
WireConnection;366;0;350;0
WireConnection;245;0;239;0
WireConnection;245;1;242;3
WireConnection;293;0;287;0
WireConnection;293;1;290;3
WireConnection;261;0;255;0
WireConnection;261;1;258;3
WireConnection;229;0;223;0
WireConnection;229;1;226;3
WireConnection;260;0;257;0
WireConnection;260;1;259;0
WireConnection;211;0;206;0
WireConnection;211;1;208;3
WireConnection;104;0;97;1
WireConnection;210;0;207;0
WireConnection;210;1;209;0
WireConnection;325;0;319;0
WireConnection;325;1;322;3
WireConnection;244;0;241;0
WireConnection;244;1;243;0
WireConnection;148;0;178;0
WireConnection;148;1;145;0
WireConnection;82;2;80;0
WireConnection;277;0;271;0
WireConnection;277;1;274;3
WireConnection;309;0;303;0
WireConnection;309;1;306;3
WireConnection;347;1;366;0
WireConnection;276;0;273;0
WireConnection;276;1;275;0
WireConnection;192;0;144;0
WireConnection;192;1;191;3
WireConnection;228;0;225;0
WireConnection;228;1;227;0
WireConnection;324;0;321;0
WireConnection;324;1;323;0
WireConnection;292;0;289;0
WireConnection;292;1;291;0
WireConnection;308;0;305;0
WireConnection;308;1;307;0
WireConnection;168;0;164;0
WireConnection;168;1;165;0
WireConnection;230;0;229;0
WireConnection;230;1;228;0
WireConnection;279;0;277;0
WireConnection;279;1;273;0
WireConnection;113;0;18;0
WireConnection;113;1;118;0
WireConnection;158;0;154;0
WireConnection;158;1;155;0
WireConnection;278;0;277;0
WireConnection;278;1;276;0
WireConnection;128;0;131;0
WireConnection;128;1;130;0
WireConnection;187;0;192;0
WireConnection;187;1;178;0
WireConnection;310;0;309;0
WireConnection;310;1;308;0
WireConnection;102;0;82;0
WireConnection;295;0;293;0
WireConnection;295;1;289;0
WireConnection;135;0;133;0
WireConnection;135;1;134;0
WireConnection;231;0;229;0
WireConnection;231;1;225;0
WireConnection;263;0;261;0
WireConnection;263;1;257;0
WireConnection;213;0;211;0
WireConnection;213;1;210;0
WireConnection;294;0;293;0
WireConnection;294;1;292;0
WireConnection;327;0;325;0
WireConnection;327;1;321;0
WireConnection;246;0;245;0
WireConnection;246;1;244;0
WireConnection;110;0;104;0
WireConnection;247;0;245;0
WireConnection;247;1;241;0
WireConnection;215;0;211;0
WireConnection;215;1;207;0
WireConnection;186;0;192;0
WireConnection;186;1;148;0
WireConnection;311;0;309;0
WireConnection;311;1;305;0
WireConnection;355;0;347;1
WireConnection;355;2;356;0
WireConnection;355;3;356;0
WireConnection;355;4;357;0
WireConnection;262;0;261;0
WireConnection;262;1;260;0
WireConnection;326;0;325;0
WireConnection;326;1;324;0
WireConnection;169;0;164;0
WireConnection;169;1;168;0
WireConnection;169;2;166;0
WireConnection;250;2;38;0
WireConnection;251;0;247;0
WireConnection;251;1;246;0
WireConnection;251;2;249;0
WireConnection;159;0;154;0
WireConnection;159;1;158;0
WireConnection;159;2;156;0
WireConnection;160;2;15;0
WireConnection;216;2;36;0
WireConnection;71;2;70;0
WireConnection;315;0;311;0
WireConnection;315;1;310;0
WireConnection;315;2;313;0
WireConnection;149;0;187;0
WireConnection;149;1;186;0
WireConnection;149;2;146;0
WireConnection;298;2;41;0
WireConnection;20;2;12;0
WireConnection;170;2;16;0
WireConnection;3;2;11;0
WireConnection;365;0;355;0
WireConnection;282;2;39;0
WireConnection;283;0;279;0
WireConnection;283;1;278;0
WireConnection;283;2;281;0
WireConnection;137;0;133;0
WireConnection;137;1;135;0
WireConnection;137;2;136;0
WireConnection;217;0;215;0
WireConnection;217;1;213;0
WireConnection;217;2;214;0
WireConnection;235;0;231;0
WireConnection;235;1;230;0
WireConnection;235;2;233;0
WireConnection;314;2;42;0
WireConnection;127;0;131;0
WireConnection;127;1;128;0
WireConnection;127;2;129;0
WireConnection;330;2;43;0
WireConnection;331;0;327;0
WireConnection;331;1;326;0
WireConnection;331;2;329;0
WireConnection;115;0;18;0
WireConnection;115;1;113;0
WireConnection;115;2;111;0
WireConnection;299;0;295;0
WireConnection;299;1;294;0
WireConnection;299;2;297;0
WireConnection;150;2;14;0
WireConnection;99;0;96;0
WireConnection;99;1;95;0
WireConnection;99;2;110;0
WireConnection;267;0;263;0
WireConnection;267;1;262;0
WireConnection;267;2;265;0
WireConnection;234;2;37;0
WireConnection;138;2;13;0
WireConnection;266;2;40;0
WireConnection;139;0;138;0
WireConnection;139;1;137;0
WireConnection;236;0;234;0
WireConnection;236;1;235;0
WireConnection;332;0;330;0
WireConnection;332;1;331;0
WireConnection;100;0;99;0
WireConnection;100;1;101;0
WireConnection;7;0;3;0
WireConnection;7;1;115;0
WireConnection;75;2;6;0
WireConnection;284;0;282;0
WireConnection;284;1;283;0
WireConnection;171;0;170;0
WireConnection;171;1;169;0
WireConnection;21;0;20;0
WireConnection;21;1;127;0
WireConnection;300;0;298;0
WireConnection;300;1;299;0
WireConnection;218;0;216;0
WireConnection;218;1;217;0
WireConnection;161;0;160;0
WireConnection;161;1;159;0
WireConnection;72;0;71;0
WireConnection;72;1;73;0
WireConnection;252;0;250;0
WireConnection;252;1;251;0
WireConnection;316;0;314;0
WireConnection;316;1;315;0
WireConnection;151;0;150;0
WireConnection;151;1;149;0
WireConnection;345;0;343;0
WireConnection;345;1;346;0
WireConnection;345;2;365;0
WireConnection;268;0;266;0
WireConnection;268;1;267;0
WireConnection;152;0;151;0
WireConnection;162;0;161;0
WireConnection;76;0;75;0
WireConnection;76;1;78;0
WireConnection;119;0;7;0
WireConnection;301;0;300;0
WireConnection;123;0;21;0
WireConnection;269;0;268;0
WireConnection;333;0;332;0
WireConnection;203;0;100;0
WireConnection;203;1;72;0
WireConnection;237;0;236;0
WireConnection;344;0;367;0
WireConnection;344;1;345;0
WireConnection;172;0;171;0
WireConnection;219;0;218;0
WireConnection;285;0;284;0
WireConnection;140;0;139;0
WireConnection;317;0;316;0
WireConnection;253;0;252;0
WireConnection;195;0;76;0
WireConnection;108;0;203;0
WireConnection;360;0;344;0
WireConnection;68;0;220;0
WireConnection;68;1;334;0
WireConnection;68;2;335;0
WireConnection;68;3;336;0
WireConnection;68;4;337;0
WireConnection;68;5;338;0
WireConnection;68;6;339;0
WireConnection;68;7;340;0
WireConnection;19;0;120;0
WireConnection;19;1;122;0
WireConnection;19;2;141;0
WireConnection;19;3;173;0
WireConnection;19;4;174;0
WireConnection;19;5;175;0
WireConnection;69;0;19;0
WireConnection;69;1;109;0
WireConnection;69;2;196;0
WireConnection;69;3;363;0
WireConnection;69;4;68;0
WireConnection;197;0;75;0
WireConnection;35;0;69;0
WireConnection;199;0;69;0
WireConnection;74;0;35;3
WireConnection;74;1;194;0
WireConnection;74;2;198;0
WireConnection;200;0;199;0
WireConnection;89;0;74;0
WireConnection;0;2;200;0
WireConnection;0;10;89;0
ASEEND*/
//CHKSM=1DE1A7CA5E9C823F649987C1E768EC150513B108