// Faceted style with surface detail (user request, November 2, 2026). Palette-strip colour (_BaseMap at the mesh UV,
// tinted by _BaseColor) multiplied by a grayscale detail texture chosen per colour: the palette alpha holds the
// detail type (alpha = type*8 + 4; 255 = none), the type picks a tile of the 8x4 _DetailMap atlas, and the tile is
// projected onto each face in object space (triplanar, _DetailScale tiles per metre). No mesh UV changes needed.
// Lighting: URP main light with shadows, spherical-harmonics ambient and fog, plus per-pixel additional point lights
// (diffuse, unshadowed: the counter lamps, Milestone 14). Painted lanterns elsewhere stay unlit.
Shader "TheTrailWalker/FacetedDetail"
{
    Properties
    {
        _BaseMap ("Palette", 2D) = "white" {}
        _BaseColor ("Tint", Color) = (1,1,1,1)
        _DetailMap ("Detail atlas (8x4 grayscale)", 2D) = "gray" {}
        _DetailScale ("Detail tiles per metre", Float) = 1
        _DetailStrength ("Detail strength", Range(0, 1)) = 0.75
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" "Queue"="Geometry" }

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        CBUFFER_START(UnityPerMaterial)
            float4 _BaseMap_ST;
            half4 _BaseColor;
            float4 _DetailMap_ST;
            float _DetailScale;
            half _DetailStrength;
        CBUFFER_END
        TEXTURE2D(_BaseMap); SAMPLER(sampler_BaseMap);
        TEXTURE2D(_DetailMap); SAMPLER(sampler_DetailMap);
        ENDHLSL

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile_fragment _ _SHADOWS_SOFT _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH
            #pragma multi_compile_fog
            #pragma multi_compile_instancing
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _CLUSTER_LIGHT_LOOP
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes { float4 positionOS : POSITION; float3 normalOS : NORMAL; float2 uv : TEXCOORD0; UNITY_VERTEX_INPUT_INSTANCE_ID };
            struct Varyings
            {
                float4 positionCS : SV_POSITION; float2 uv : TEXCOORD0; float3 positionWS : TEXCOORD1; float3 normalWS : TEXCOORD2;
                float3 positionOS : TEXCOORD3; float3 normalOS : TEXCOORD4; half fog : TEXCOORD5;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            Varyings Vert(Attributes input)
            {
                Varyings o = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(input); UNITY_TRANSFER_INSTANCE_ID(input, o);
                VertexPositionInputs p = GetVertexPositionInputs(input.positionOS.xyz);
                o.positionCS = p.positionCS; o.positionWS = p.positionWS;
                o.normalWS = TransformObjectToWorldNormal(input.normalOS);
                o.positionOS = input.positionOS.xyz; o.normalOS = input.normalOS;
                o.uv = TRANSFORM_TEX(input.uv, _BaseMap);
                o.fog = ComputeFogFactor(p.positionCS.z);
                return o;
            }

            // One projection of the chosen atlas tile; gradients come from the unwrapped coordinate so tile seams
            // never pick a blurry mip.
            half SampleTile(float2 coord, float2 tile)
            {
                float2 local = frac(coord);
                // 256px tiles, 8px wrapped gutters. Limit the footprint to mip 3
                // so minification never reaches a neighbouring material tile.
                float2 atlasSize = float2(2048, 1024);
                float2 innerSize = 240.0 / atlasSize;
                float2 dx = ddx(coord) * innerSize, dy = ddy(coord) * innerSize;
                float limit = min(1.0, 8.0 / max(1.0, max(length(dx * atlasSize), length(dy * atlasSize))));
                return SAMPLE_TEXTURE2D_GRAD(_DetailMap, sampler_DetailMap,
                    (tile * 256.0 + 8.0) / atlasSize + local * innerSize, dx * limit, dy * limit).r;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                half4 palette = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv);
                half3 albedo = palette.rgb * _BaseColor.rgb;
                int type = (int)floor(palette.a * 255.0 / 8.0);
                if (type > 0 && type < 26)
                {
                    float2 tile = float2(type % 8, 3 - type / 8);          // atlas row 0 is the top of the image
                    float3 n = abs(normalize(input.normalOS));
                    float3 w = pow(n, 4); w /= (w.x + w.y + w.z);
                    float3 p = input.positionOS * _DetailScale;
                    // Side projections keep +Y as the tile's up, so bark ridges, planks and brick rows stay level.
                    half detail = SampleTile(p.zy, tile) * w.x + SampleTile(p.xz, tile) * w.y + SampleTile(p.xy, tile) * w.z;
                    albedo *= lerp(1.0h, detail * 2.0h, _DetailStrength);
                }
                float3 normal = normalize(input.normalWS);
                Light light = GetMainLight(TransformWorldToShadowCoord(input.positionWS));
                half ndl = saturate(dot(normal, light.direction));
                half3 color = albedo * (light.color * (ndl * light.shadowAttenuation * light.distanceAttenuation) + SampleSH(normal));
                #if defined(_ADDITIONAL_LIGHTS)
                InputData inputData = (InputData)0;   // LIGHT_LOOP_BEGIN reads these two under the cluster light loop
                inputData.positionWS = input.positionWS; inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(input.positionCS);
                uint count = GetAdditionalLightsCount();
                LIGHT_LOOP_BEGIN(count)
                    Light extra = GetAdditionalLight(lightIndex, input.positionWS);
                    color += albedo * extra.color * (saturate(dot(normal, extra.direction)) * extra.distanceAttenuation);
                LIGHT_LOOP_END
                #endif
                color = MixFog(color, input.fog);
                return half4(color, 1);
            }
            ENDHLSL
        }

        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode"="ShadowCaster" }
            ZWrite On ZTest LEqual ColorMask 0
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_instancing
            #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"
            float3 _LightDirection; float3 _LightPosition;
            struct Attributes { float4 positionOS : POSITION; float3 normalOS : NORMAL; UNITY_VERTEX_INPUT_INSTANCE_ID };
            struct Varyings { float4 positionCS : SV_POSITION; };
            Varyings Vert(Attributes input)
            {
                Varyings o; UNITY_SETUP_INSTANCE_ID(input);
                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
                float3 normalWS = TransformObjectToWorldNormal(input.normalOS);
                #if _CASTING_PUNCTUAL_LIGHT_SHADOW
                    float3 lightDirection = normalize(_LightPosition - positionWS);
                #else
                    float3 lightDirection = _LightDirection;
                #endif
                float4 positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, lightDirection));
                #if UNITY_REVERSED_Z
                    positionCS.z = min(positionCS.z, UNITY_NEAR_CLIP_VALUE);
                #else
                    positionCS.z = max(positionCS.z, UNITY_NEAR_CLIP_VALUE);
                #endif
                o.positionCS = positionCS; return o;
            }
            half4 Frag(Varyings input) : SV_Target { return 0; }
            ENDHLSL
        }

        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode"="DepthOnly" }
            ZWrite On ColorMask R
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_instancing
            struct Attributes { float4 positionOS : POSITION; UNITY_VERTEX_INPUT_INSTANCE_ID };
            struct Varyings { float4 positionCS : SV_POSITION; };
            Varyings Vert(Attributes input) { Varyings o; UNITY_SETUP_INSTANCE_ID(input); o.positionCS = TransformObjectToHClip(input.positionOS.xyz); return o; }
            half4 Frag(Varyings input) : SV_Target { return input.positionCS.z; }
            ENDHLSL
        }

        Pass
        {
            Name "DepthNormals"
            Tags { "LightMode"="DepthNormals" }
            ZWrite On
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_instancing
            struct Attributes { float4 positionOS : POSITION; float3 normalOS : NORMAL; UNITY_VERTEX_INPUT_INSTANCE_ID };
            struct Varyings { float4 positionCS : SV_POSITION; float3 normalWS : TEXCOORD0; };
            Varyings Vert(Attributes input) { Varyings o; UNITY_SETUP_INSTANCE_ID(input); o.positionCS = TransformObjectToHClip(input.positionOS.xyz); o.normalWS = TransformObjectToWorldNormal(input.normalOS); return o; }
            half4 Frag(Varyings input) : SV_Target { return half4(normalize(input.normalWS), 0); }
            ENDHLSL
        }
    }
    FallBack Off
}
