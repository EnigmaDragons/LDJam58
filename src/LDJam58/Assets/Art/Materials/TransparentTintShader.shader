Shader "Custom/TransparentTintShader"
{
    Properties
    {
        _BaseTint ("Base Tint", Color) = (1,1,1,1)
        _BorderWidth ("Border Width", Range(0.01, 0.5)) = 0.1
        _CenterOpacity ("Center Opacity", Range(0, 1)) = 0.2
        _BorderOpacity ("Border Opacity", Range(0, 1)) = 0.7
    }
    
    SubShader
    {
        Tags 
        { 
            "RenderType"="Transparent" 
            "Queue"="Transparent"
            "RenderPipeline"="UniversalPipeline"
        }
        
        LOD 100
        
        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }
            
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            
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
            
            CBUFFER_START(UnityPerMaterial)
                float4 _BaseTint;
                float _BorderWidth;
                float _CenterOpacity;
                float _BorderOpacity;
            CBUFFER_END
            
            Varyings vert (Attributes input)
            {
                Varyings output;
                output.positionHCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                return output;
            }
            
            float4 frag (Varyings input) : SV_Target
            {
                float2 uv = input.uv;
                
                // Calculate distance from edges
                float distFromEdge = min(min(uv.x, 1.0 - uv.x), min(uv.y, 1.0 - uv.y));
                
                // Create smooth transition between center and border
                float borderFactor = smoothstep(0.0, _BorderWidth, distFromEdge);
                
                // Interpolate between center and border opacity
                float alpha = lerp(_BorderOpacity, _CenterOpacity, borderFactor);
                
                // Apply the base tint with calculated alpha
                float4 color = _BaseTint;
                color.a = alpha;
                
                return color;
            }
            ENDHLSL
        }
    }
    
    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
