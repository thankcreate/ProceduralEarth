// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "AfSun"
{
	Properties
	{
		_Min("Min", Float) = 0
		_Max("Max", Float) = 0
		_HeightColorTex("HeightColorTex", 2D) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" "IsEmissive" = "true"  }
		Cull Back
		CGPROGRAM
		#include "UnityShaderVariables.cginc"
		#pragma target 3.0
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows 
		struct Input
		{
			float3 worldPos;
		};

		uniform sampler2D _HeightColorTex;
		uniform float _Min;
		uniform float _Max;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float mulTime16 = _Time.y * 0.05;
			float3 ase_vertex3Pos = mul( unity_WorldToObject, float4( i.worldPos , 1 ) );
			float4 appendResult19 = (float4(ase_vertex3Pos.x , ase_vertex3Pos.y , ase_vertex3Pos.z , 0.0));
			float clampResult14 = clamp( ( ( length( appendResult19 ) - _Min ) / ( _Max - _Min ) ) , 0.01 , 1.0 );
			float2 temp_cast_0 = (( mulTime16 + clampResult14 )).xx;
			float4 tex2DNode12 = tex2D( _HeightColorTex, temp_cast_0 );
			o.Albedo = tex2DNode12.rgb;
			o.Emission = tex2DNode12.rgb;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=16700
379.3333;72.66667;943;437;1585.151;247.1211;2.035736;True;False
Node;AmplifyShaderEditor.PosVertexDataNode;1;-1352.607,213.7655;Float;False;0;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DynamicAppendNode;19;-1049.493,97.80701;Float;False;FLOAT4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RangedFloatNode;11;-903.7766,397.8995;Float;False;Property;_Max;Max;1;0;Create;False;0;0;False;0;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;5;-928.0797,506.3272;Float;False;Property;_Min;Min;0;0;Create;True;0;0;False;0;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.LengthOpNode;3;-792.749,215.2642;Float;False;1;0;FLOAT4;0,0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;10;-651.7214,463.0999;Float;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;7;-622.144,276.039;Float;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;9;-427.7256,313.5908;Float;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;14;-281.2726,283.1259;Float;False;3;0;FLOAT;0;False;1;FLOAT;0.01;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;16;-543.348,-46.80801;Float;False;1;0;FLOAT;0.05;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;20;-315.9238,-5.388821;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;12;-47.21108,32.72916;Float;True;Property;_HeightColorTex;HeightColorTex;2;0;Create;True;0;0;False;0;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;336.9836,188.3418;Float;False;True;2;Float;ASEMaterialInspector;0;0;Standard;AfSun;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;19;0;1;1
WireConnection;19;1;1;2
WireConnection;19;2;1;3
WireConnection;3;0;19;0
WireConnection;10;0;11;0
WireConnection;10;1;5;0
WireConnection;7;0;3;0
WireConnection;7;1;5;0
WireConnection;9;0;7;0
WireConnection;9;1;10;0
WireConnection;14;0;9;0
WireConnection;20;0;16;0
WireConnection;20;1;14;0
WireConnection;12;1;20;0
WireConnection;0;0;12;0
WireConnection;0;2;12;0
ASEEND*/
//CHKSM=B7F02832F332837B1EB31E457E3C107B14571928