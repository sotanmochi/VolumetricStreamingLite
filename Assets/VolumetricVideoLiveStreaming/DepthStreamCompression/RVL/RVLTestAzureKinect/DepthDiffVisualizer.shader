// Copyright (c) Soichiro Sugimoto.
// Licensed under the MIT License. See LICENSE in the project root for license information.

Shader "Custom/DepthDiffVisualizer"
{
    Properties
    {
        _MainTex ("Color Texture", 2D) = "white" {}
        _DepthTex ("Depth Texture", 2D) = "white" {}
        _DepthScale ("Depth Scale", Float) = 1.0
        _MaxInMeters ("Maximum Distance", Float) = 4.2
        _MinInMeters ("Minimum Distance", Float) = 0.2
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            sampler2D _DepthTex;
            float _DepthScale;
            float _MaxInMeters;
            float _MinInMeters;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float4 depthTex = tex2D(_DepthTex, i.uv);

                float4 color = float4(0, 0, 0, 1);
                if (depthTex.r != 0)
                {
                    color = float4(1, 0, 0, 1);
                }

                return color;
            }

            ENDCG
        }
    }
}
