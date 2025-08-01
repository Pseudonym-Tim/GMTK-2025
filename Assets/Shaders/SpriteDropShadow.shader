Shader "GMTK 2025/SpriteDropShadow"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        [MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
        _ShadowColor ("Shadow", Color) = (0,0,0,1)
        _ShadowOffset ("ShadowOffset", Vector) = (0,-0.1,0,0)
        _Flash ("Flash Amount", Float) = 0
        _Scale ("Scale", Float) = 1.0
        _RotationZ ("Rotation Z (Degrees)", Float) = 0.0
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend One OneMinusSrcAlpha

        CGINCLUDE
        float2 RotateZ(float2 position, float angle)
        {
            float rad = angle * 3.14159 / 180.0;
            float s = sin(rad);
            float c = cos(rad);
            return float2(position.x * c - position.y * s, position.x * s + position.y * c);
        }
        ENDCG

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ PIXELSNAP_ON
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            fixed4 _ShadowColor;
            float2 _ShadowOffset;
            float _Scale;

            v2f vert(appdata_t IN)
            {
                v2f OUT;

                float4 clipPos = UnityObjectToClipPos(IN.vertex);

                clipPos.xy += (IN.vertex.xy * _Scale - IN.vertex.xy) + _ShadowOffset * clipPos.w;

                OUT.vertex  = clipPos;
                OUT.texcoord = IN.texcoord;
                OUT.color    = IN.color * _ShadowColor;

                #ifdef PIXELSNAP_ON
                OUT.vertex = UnityPixelSnap(OUT.vertex);
                #endif

                return OUT;
            }

            sampler2D _MainTex;
            sampler2D _AlphaTex;
            float    _AlphaSplitEnabled;

            fixed4 SampleSpriteTexture(float2 uv)
            {
                fixed4 c = tex2D(_MainTex, uv);
                #if UNITY_TEXTURE_ALPHASPLIT_ALLOWED
                    if (_AlphaSplitEnabled)
                        c.a = tex2D(_AlphaTex, uv).r;
                #endif
                c.rgb = _ShadowColor.rgb;
                return c;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                fixed4 c = SampleSpriteTexture(IN.texcoord) * IN.color;
                c.rgb *= c.a;
                return c;
            }

            ENDCG
        }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ PIXELSNAP_ON
            #pragma multi_compile_fog
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                UNITY_FOG_COORDS(1)
                fixed4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            fixed4 _Color;
            float _Flash;
            float _Scale;
            float _RotationZ;

            v2f vert(appdata_t IN)
            {
                v2f OUT;

                float2 rotatedPosition = RotateZ(IN.vertex.xy * _Scale, _RotationZ);
                float4 transformedVertex = float4(rotatedPosition, IN.vertex.z, IN.vertex.w);

                OUT.vertex = UnityObjectToClipPos(transformedVertex);
                OUT.texcoord = IN.texcoord;
                OUT.color = IN.color * _Color;

                #ifdef PIXELSNAP_ON
                OUT.vertex = UnityPixelSnap(OUT.vertex);
                #endif

                UNITY_TRANSFER_FOG(OUT, OUT.vertex);

                return OUT;
            }

            sampler2D _MainTex;
            sampler2D _AlphaTex;
            float _AlphaSplitEnabled;

            fixed4 SampleSpriteTexture(float2 uv)
            {
                fixed4 color = tex2D(_MainTex, uv);

                #if UNITY_TEXTURE_ALPHASPLIT_ALLOWED
                if (_AlphaSplitEnabled)
                    color.a = tex2D(_AlphaTex, uv).r;
                #endif

                return color;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                fixed4 col = SampleSpriteTexture(IN.texcoord) * IN.color;

                col.rgb = lerp(col.rgb, float3(1, 1, 1), _Flash);

                col.rgb *= col.a;
                UNITY_APPLY_FOG(IN.fogCoord, col);
                return col;
            }

            ENDCG
        }
    }
}
