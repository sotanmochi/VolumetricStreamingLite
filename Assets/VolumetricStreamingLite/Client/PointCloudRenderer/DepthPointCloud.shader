// Copyright (c) 2020 Soichiro Sugimoto.
// Licensed under the MIT License. See LICENSE in the project root for license information.

Shader "Custom/DepthPointCloud"
{
    Properties
    {
        _MainTex ("Color Texture", 2D) = "white" {}
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

            Buffer<uint> _DepthBuffer;
            int _Width;
            int _Height;
            float _DepthScale;

            uint uint_to_ushort(uint raw, bool high)
            {
                uint4 c4 = uint4(raw, raw >> 8, raw >> 16, raw >> 24) & 0xff;
                uint2 c2 = high ? c4.zw : c4.xy;
                return c2.x + (c2.y << 8);
            }

            v2f vert (appdata v)
            {
                float2 uv = float2(v.uv.x, 1 - v.uv.y);

                // Buffer index
                uint idx = (uint)(uv.x * _Width) + (uint)(uv.y * _Height) * _Width;

                // Depth sample (int16 -> float)
                float depth = uint_to_ushort(_DepthBuffer[idx >> 1], idx & 1) / 1000.0 * _DepthScale;

                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex * depth);
                o.uv = TRANSFORM_TEX(uv, _MainTex);
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
