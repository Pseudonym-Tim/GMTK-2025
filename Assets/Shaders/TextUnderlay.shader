Shader "GMTK 2025/TextUnderlay"
{
    Properties
    {
        _MainTex        ("Font Atlas", 2D) = "white" {}
        _FaceTex        ("Font Texture", 2D) = "white" {}
        _FaceColor      ("Text Color", Color) = (1,1,1,1)

        _UnderlayColor  ("Underlay Color", Color) = (0,0,0,0)
        _UnderlayX      ("Underlay Offset X", Float) = 1
        _UnderlayY      ("Underlay Offset Y", Float) = 1

        _VertexOffsetX  ("Vertex OffsetX", float) = 0
        _VertexOffsetY  ("Vertex OffsetY", float) = 0
        _MaskSoftnessX  ("Mask SoftnessX", float) = 0
        _MaskSoftnessY  ("Mask SoftnessY", float) = 0

        _Stencil        ("Stencil Ref", Int) = 0
        _StencilComp    ("Stencil Comp", Int) = 8
        _StencilOp      ("Stencil Operation", Int) = 0
        _StencilWriteMask ("Stencil Write Mask", Int) = 255
        _StencilReadMask  ("Stencil Read Mask", Int) = 255
        _ColorMask      ("Color Mask", Int) = 15
    }

    SubShader
    {
        Tags { "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" }

        Stencil
        {
            Ref[_Stencil]
            Comp[_StencilComp]
            Pass[_StencilOp]
            ReadMask[_StencilReadMask]
            WriteMask[_StencilWriteMask]
        }

        Lighting Off
        Cull [_CullMode]
        ZTest [unity_GUIZTestMode]
        ZWrite Off
        Fog { Mode Off }
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask[_ColorMask]

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord0: TEXCOORD0;
                float  texcoord1: TEXCOORD1;
                float2 mask     : TEXCOORD2;
            };

            struct v2f {
                float4 pos       : SV_POSITION;
                float4 color     : COLOR;
                float2 uvMain    : TEXCOORD0;
                float2 uvFace    : TEXCOORD1;
                float4 mask      : TEXCOORD2;
            };

            sampler2D _MainTex;
            float4   _MainTex_TexelSize;
            sampler2D _FaceTex;
            float4   _FaceTex_ST;
            float4   _FaceTex_TexelSize;
            fixed4   _FaceColor;
            fixed4   _UnderlayColor;
            float    _UnderlayX;
            float    _UnderlayY;
            float    _VertexOffsetX;
            float    _VertexOffsetY;
            float4   _ClipRect;
            float    _MaskSoftnessX;
            float    _MaskSoftnessY;
            #if UNITY_VERSION < 530
            bool     _UseClipRect;
            #endif

            float2 UnpackUV(float uv)
            {
                float2 output;
                output.x = floor(uv / 4096);
                output.y = uv - 4096 * output.x;
                return output * 0.001953125;
            }

            v2f vert(appdata_t v)
            {
                v2f o;
                float4 pos = v.vertex;

                // Base vertex offset...
                pos.x += _VertexOffsetX * pos.w;
                pos.y += _VertexOffsetY * pos.w;
                o.pos = UnityPixelSnap(UnityObjectToClipPos(pos));

                o.uvMain = v.texcoord0;
                o.uvFace = TRANSFORM_TEX(UnpackUV(v.texcoord1), _FaceTex);
                o.color  = v.color;

                // Compute mask for clipping...
                float2 pixelSize = o.pos.w;
                pixelSize /= abs(float2(_ScreenParams.x * UNITY_MATRIX_P[0][0], _ScreenParams.y * UNITY_MATRIX_P[1][1]));
                float4 clampedRect = clamp(_ClipRect, -2e10, 2e10);
                o.mask = float4(pos.xy * 2 - clampedRect.xy, 0.25 * half2(_MaskSoftnessX, _MaskSoftnessY) + pixelSize.xy);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // Base alpha mask...
                fixed baseA = tex2D(_MainTex, i.uvMain).a;

                // Compute UV offsets...
                float2 offsetFaceUV = float2(_UnderlayX * _FaceTex_TexelSize.x, _UnderlayY * _FaceTex_TexelSize.y);
                float2 offsetMainUV = float2(_UnderlayX * _MainTex_TexelSize.x, _UnderlayY * _MainTex_TexelSize.y);

                // Sample the underlay (face texture moved by exact pixel offset)...
                fixed4 under = tex2D(_FaceTex, i.uvFace + offsetFaceUV) * _UnderlayColor;
                under.a *= tex2D(_MainTex, i.uvMain + offsetMainUV).a;

                // Sample the main face...
                fixed4 face = tex2D(_FaceTex, i.uvFace) * _FaceColor;
                face.a *= baseA;

                // Composite...
                fixed4 c;
                c.rgb = lerp(under.rgb, face.rgb, face.a);
                c.a   = max(face.a, under.a);

                // Clipping...
                half2 m = saturate((_ClipRect.zw - _ClipRect.xy - abs(i.mask.xy)) * i.mask.zw);
                c *= m.x * m.y;

                return c * i.color;
            }

            ENDCG
        }
    }
}
