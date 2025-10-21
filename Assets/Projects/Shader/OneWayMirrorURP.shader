Shader "Custom/OneWayMirror_OutsideOnly"
{
    Properties
    {
        _BaseMap("Texture", 2D) = "white" {}
        _BaseColor("Color", Color) = (1,1,1,1)
        _Transparency("Transparency", Range(0,1)) = 0.3
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            Name "ForwardUnlit"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);
            float4 _BaseColor;
            float _Transparency;

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 normalWS : TEXCOORD0;
                float3 viewDirWS : TEXCOORD1;
                float2 uv : TEXCOORD2;
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS);
                OUT.normalWS = normalize(TransformObjectToWorldNormal(IN.normalOS));
                float3 worldPos = TransformObjectToWorld(IN.positionOS);
                OUT.viewDirWS = normalize(GetCameraPositionWS() - worldPos);
                OUT.uv = IN.uv;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                // dot > 0 → 外側（不透明） / dot < 0 → 内側（透過）
                float dotNV = dot(normalize(IN.normalWS), normalize(IN.viewDirWS));
                float alphaFactor = saturate(dotNV * 0.5 + 0.5);
                half4 baseColor = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv) * _BaseColor;
                baseColor.a = lerp(_Transparency, 1.0, alphaFactor); 
                return baseColor;
            }
            ENDHLSL
        }
    }
}
