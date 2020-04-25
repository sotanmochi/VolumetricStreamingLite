// Copyright (c) Soichiro Sugimoto.
// Licensed under the MIT License. See LICENSE in the project root for license information.

Shader "Custom/DepthPointCloud"
{
    Properties
    {
        _MainTex ("Color Texture", 2D) = "white" {}
        _DepthTex ("Depth Texture", 2D) = "white" {}
        _DepthScale ("Depth Scale", Float) = 1.0
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

            v2f vert (appdata v)
            {
                // Original depth data format is 16-bit unsigned integer
                float4 depth = tex2Dlod(_DepthTex, float4(v.uv, 0, 0)).r * 65535.0 / 1000.0;

                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex * depth);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                return col;
            }

            ENDCG
        }
    }
}
