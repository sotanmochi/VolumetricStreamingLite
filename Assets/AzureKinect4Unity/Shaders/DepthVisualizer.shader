// Copyright (c) Soichiro Sugimoto.
// Licensed under the MIT License. See LICENSE in the project root for license information.

Shader "Custom/DepthVisualizer"
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
                float depth = depthTex.r * 65535.0 / 1000.0 * _DepthScale;

                float h = 300.0/360.0 * (depth - _MinInMeters) / ((_MaxInMeters - _MinInMeters) * _DepthScale);
                float s = 1.0;
                float v = 1.0;

                if (depth < _MinInMeters || depth > _MaxInMeters)
                {
                    v = 0.0;
                }

                float r = 0.0;
                float g = 0.0;
                float b = 0.0;
                if (s > 0.0)
                {
                    h *= 6.0;
                    int i = (int) h;
                    float f = h - (float) i;
                    float aa = v * (1 - s);
                    float bb = v * (1 - s * f);
                    float cc = v * (1 - s * (1 - f));
                    switch (i)
                    {
                        default:
                        case 0:
                            r = v;
                            g = cc;
                            b = aa;
                            break;
                        case 1:
                            r = bb;
                            g = v;
                            b = aa;
                            break;
                        case 2:
                            r = aa;
                            g = v;
                            b = cc;
                            break;
                        case 3:
                            r = aa;
                            g = bb;
                            b = v;
                            break;
                        case 4:
                            r = cc;
                            g = aa;
                            b = v;
                            break;
                        case 5:
                            r = v;
                            g = aa;
                            b = bb;
                            break;
                    }
                }

                return float4(r, g, b, 1);
            }

            ENDCG
        }
    }
}
