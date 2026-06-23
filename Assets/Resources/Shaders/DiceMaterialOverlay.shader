Shader "DiceKing/DiceMaterialOverlay"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Tint ("Tint", Color) = (1,1,1,1)
        _Highlight ("Highlight", Color) = (1,1,1,1)
        _PatternMode ("Pattern Mode", Float) = 0
        _PatternStrength ("Pattern Strength", Float) = 0.5
        _RimStrength ("Rim Strength", Float) = 0.4
        _PulseStrength ("Pulse Strength", Float) = 0.0
        _Alpha ("Alpha", Float) = 1.0
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            fixed4 _Tint;
            fixed4 _Highlight;
            float _PatternMode;
            float _PatternStrength;
            float _RimStrength;
            float _PulseStrength;
            float _Alpha;

            fixed4 frag(v2f_img i) : SV_Target
            {
                float2 uv = i.uv;
                fixed4 src = tex2D(_MainTex, uv);

                float2 centered = abs(uv - 0.5) * 2.0;
                float edge = max(centered.x, centered.y);
                float rim = smoothstep(0.54, 0.98, edge);
                float softCenter = 1.0 - smoothstep(0.0, 0.98, length(uv - 0.5) * 1.55);
                float diagonal = frac((uv.x + uv.y) * 7.0 + _Time.y * 0.08);
                float stripe = 1.0 - smoothstep(0.08, 0.22, abs(diagonal - 0.5));
                float vertical = 1.0 - smoothstep(0.05, 0.18, abs(frac(uv.x * 6.0) - 0.5));
                float ring = 1.0 - smoothstep(0.015, 0.08, abs(length(uv - 0.5) - 0.34));
                float glint = pow(saturate(1.0 - abs((uv.x - uv.y) - 0.1) * 5.0), 6.0);
                float pulse = (sin(_Time.y * 2.1) * 0.5 + 0.5) * _PulseStrength;

                float pattern = stripe;
                if (_PatternMode > 0.5 && _PatternMode <= 1.5)
                {
                    pattern = max(ring, stripe * 0.35) + pulse * softCenter;
                }
                else if (_PatternMode > 1.5 && _PatternMode <= 2.5)
                {
                    pattern = max(glint, softCenter * 0.45);
                }
                else if (_PatternMode > 2.5 && _PatternMode <= 3.5)
                {
                    pattern = vertical * 0.45 - softCenter * 0.16;
                }
                else if (_PatternMode > 3.5)
                {
                    pattern = max(vertical * 0.55, ring * 0.5);
                }

                float patternAmount = saturate(pattern * _PatternStrength);
                float3 tinted = lerp(src.rgb, src.rgb * _Tint.rgb, saturate(_Tint.a));
                tinted = lerp(tinted, _Highlight.rgb, patternAmount);
                tinted = lerp(tinted, _Highlight.rgb, rim * _RimStrength);

                src.rgb = tinted;
                src.a *= saturate(_Alpha);
                return src;
            }
            ENDCG
        }
    }

    Fallback Off
}
