Shader "Apophis/LaserBeam"
{
    Properties
    {
        [HDR] _Color ("Color", Color) = (1, 0.2, 0.2, 1)
        _Length ("Length", Float) = 5
        _Width ("Width", Float) = 0.2
        _GlowIntensity ("Glow Intensity", Range(0, 10)) = 3
        _EdgeFalloff ("Edge Falloff", Range(0.1, 10)) = 2
        _CoreBoost ("Core Boost", Range(0, 5)) = 1.5
        _TipFadeStart ("Tip Fade Start", Range(0, 1)) = 0.5
        _TipFadePower ("Tip Fade Power", Range(0.1, 5)) = 1.5
    }

    SubShader
    {
        Tags
        {
            "RenderType"="Transparent"
            "Queue"="Transparent"
            "RenderPipeline"="UniversalPipeline"
            "IgnoreProjector"="True"
        }

        Pass
        {
            Name "LaserBeam"
            Blend SrcAlpha One
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _Color;
                float _Length;
                float _Width;
                float _GlowIntensity;
                float _EdgeFalloff;
                float _CoreBoost;
                float _TipFadeStart;
                float _TipFadePower;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;

                float lengthCoord = IN.positionOS.x * _Length;
                float widthCoord  = IN.positionOS.y * _Width;

                float3 centerWS    = mul(unity_ObjectToWorld, float4(0, 0, 0, 1)).xyz;
                float3 lengthDirWS = normalize(mul((float3x3)unity_ObjectToWorld, float3(1, 0, 0)));
                float3 viewDirWS   = _WorldSpaceCameraPos.xyz - centerWS;

                float3 widthDirWS = cross(viewDirWS, lengthDirWS);
                float widthLen = length(widthDirWS);
                widthDirWS = widthLen > 1e-4 ? widthDirWS / widthLen : float3(0, 1, 0);

                float3 worldPos = centerWS + lengthDirWS * lengthCoord + widthDirWS * widthCoord;

                OUT.positionHCS = TransformWorldToHClip(worldPos);
                OUT.uv = IN.uv;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float y = IN.uv.y * 2.0 - 1.0;
                float thickness = sqrt(saturate(1.0 - y * y));

                float shape = pow(thickness, _EdgeFalloff);
                float core  = pow(thickness, _EdgeFalloff * 5.0) * _CoreBoost;

                float tipFade = 1.0 - smoothstep(_TipFadeStart, 1.0, IN.uv.x);
                tipFade = pow(tipFade, _TipFadePower);

                half3 rgb = _Color.rgb * _GlowIntensity * (shape + core) * tipFade;
                half a = saturate(_Color.a * shape * tipFade);
                return half4(rgb, a);
            }
            ENDHLSL
        }
    }
    FallBack Off
}
