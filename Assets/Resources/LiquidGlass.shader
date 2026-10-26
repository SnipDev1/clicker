Shader "UI/LiquidGlass"
{
    Properties
    {
        [PerRendererData] _MainTex ("Shape (R = distance to edge, A = mask)", 2D) = "white" {}
        _BlurSize ("Blur Size", Range(0, 0.05)) = 0.005
        _Refraction ("Refraction", Range(0, 0.05)) = 0.006
        _TintStrength ("Tint Strength", Range(0, 1)) = 0.55
        _Opacity ("Opacity", Range(0, 1)) = 0.92
        _RimColor ("Rim Color", Color) = (1, 1, 1, 1)
        _RimStrength ("Rim Strength", Range(0, 4)) = 0.9
        _RimWidth ("Rim Width", Range(0.01, 1)) = 0.35
        _Shade ("Bottom Shade", Range(0, 1)) = 0.12
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
            "PreviewType" = "Plane"
            "CanUseSpriteAtlas" = "True"
            "RenderPipeline" = "UniversalPipeline"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            Name "LiquidGlass"

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            // глобальная текстура фона: Shader.SetGlobalTexture("_BackgroundTex", ...)
            TEXTURE2D(_BackgroundTex);
            SAMPLER(sampler_BackgroundTex);

            CBUFFER_START(UnityPerMaterial)
                float _BlurSize;
                float _Refraction;
                float _TintStrength;
                float _Opacity;
                float4 _RimColor;
                float _RimStrength;
                float _RimWidth;
                float _Shade;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
                float4 screenPos : TEXCOORD1;
            };

            // 13 отсчётов: центр, восемь по кругу и четыре подальше
            float3 SampleBackground(float2 uv, float2 texel)
            {
                float3 color = SAMPLE_TEXTURE2D(_BackgroundTex, sampler_BackgroundTex, uv).rgb * 0.18;

                color += SAMPLE_TEXTURE2D(_BackgroundTex, sampler_BackgroundTex, uv + texel * float2( 1.0,  0.0)).rgb * 0.075;
                color += SAMPLE_TEXTURE2D(_BackgroundTex, sampler_BackgroundTex, uv + texel * float2(-1.0,  0.0)).rgb * 0.075;
                color += SAMPLE_TEXTURE2D(_BackgroundTex, sampler_BackgroundTex, uv + texel * float2( 0.0,  1.0)).rgb * 0.075;
                color += SAMPLE_TEXTURE2D(_BackgroundTex, sampler_BackgroundTex, uv + texel * float2( 0.0, -1.0)).rgb * 0.075;

                color += SAMPLE_TEXTURE2D(_BackgroundTex, sampler_BackgroundTex, uv + texel * float2( 0.707,  0.707)).rgb * 0.075;
                color += SAMPLE_TEXTURE2D(_BackgroundTex, sampler_BackgroundTex, uv + texel * float2(-0.707,  0.707)).rgb * 0.075;
                color += SAMPLE_TEXTURE2D(_BackgroundTex, sampler_BackgroundTex, uv + texel * float2( 0.707, -0.707)).rgb * 0.075;
                color += SAMPLE_TEXTURE2D(_BackgroundTex, sampler_BackgroundTex, uv + texel * float2(-0.707, -0.707)).rgb * 0.075;

                color += SAMPLE_TEXTURE2D(_BackgroundTex, sampler_BackgroundTex, uv + texel * float2( 2.2,  0.0)).rgb * 0.055;
                color += SAMPLE_TEXTURE2D(_BackgroundTex, sampler_BackgroundTex, uv + texel * float2(-2.2,  0.0)).rgb * 0.055;
                color += SAMPLE_TEXTURE2D(_BackgroundTex, sampler_BackgroundTex, uv + texel * float2( 0.0,  2.2)).rgb * 0.055;
                color += SAMPLE_TEXTURE2D(_BackgroundTex, sampler_BackgroundTex, uv + texel * float2( 0.0, -2.2)).rgb * 0.055;

                return color;
            }

            // аналог ComputeScreenPos из UnityCG: URP его не приносит
            float4 ComputeScreenPosition(float4 positionCS)
            {
                float4 result = positionCS * 0.5;
                result.xy = float2(result.x, result.y * _ProjectionParams.x) + result.w;
                result.zw = positionCS.zw;
                return result;
            }

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.color = input.color;
                output.uv = input.uv;
                output.screenPos = ComputeScreenPosition(output.positionCS);
                return output;
            }

            float4 frag(Varyings input) : SV_Target
            {
                // R — расстояние до кромки (0 у края, 1 в глубине), A — маска формы
                float4 shape = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
                float distanceToEdge = shape.r;
                float mask = shape.a;

                float2 screenUV = input.screenPos.xy / input.screenPos.w;

                // blur должен быть одинаковым по X и Y в пикселях экрана
                float2 texel = float2(_BlurSize * (_ScreenParams.y / _ScreenParams.x), _BlurSize);

                // у самой кромки подмешиваем «линзу»: фон слегка уезжает к центру
                float edge = 1.0 - smoothstep(0.0, 0.9, distanceToEdge);
                float2 toCenter = float2(0.5, 0.5) - input.uv;
                float2 direction = toCenter * rsqrt(max(dot(toCenter, toCenter), 1e-6));
                float2 backgroundUV = screenUV + direction * edge * _Refraction;

                float3 blurred = SampleBackground(backgroundUV, texel);
                float3 glass = lerp(blurred, input.color.rgb, _TintStrength);

                // лёгкое затемнение снизу
                glass *= lerp(1.0 - _Shade, 1.0, saturate(input.uv.y));

                // блик по кромке, сильнее сверху
                float rim = 1.0 - smoothstep(0.0, _RimWidth, distanceToEdge);
                rim *= lerp(0.35, 1.0, saturate(input.uv.y));
                glass += _RimColor.rgb * rim * _RimStrength * 0.35;

                return float4(glass, mask * _Opacity);
            }
            ENDHLSL
        }
    }
}
