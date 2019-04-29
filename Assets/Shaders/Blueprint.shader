Shader "Custom/Blueprint"
{
	Properties
	{
		_WireThickness ("Wire Thickness", RANGE(0, 800)) = 1
		_WireSmoothness ("Wire Smoothness", RANGE(0, 20)) = 10
		_WireColor ("Wire Color", Color) = (0.5, 0.3, 0.3, 1.0)
		_BaseColor ("Base Color", Color) = (0.5, 0.3, 0.3, 0.0)
		_MaxTriSize ("Max Tri Size", RANGE(0, 200)) = 10
	}

	SubShader
	{
		Tags {
            "IgnoreProjector"="True"
            "Queue"="Transparent"
            "RenderType"="Transparent"
        }

		Pass
		{
			Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
			Cull Off
			
			CGPROGRAM
			#pragma vertex vert
			#pragma geometry geom
			#pragma fragment frag

			#include "UnityCG.cginc"
			#include "edge.cginc"

			ENDCG
		}
	}
}