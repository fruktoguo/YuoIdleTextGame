Shader "Custom/MaskedObject3D"
{
    Properties
    {
        [Header(Main Settings)]
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        
        [Header(Stencil Settings)]
        [IntRange] _Stencil ("Stencil Reference", Range(0, 255)) = 0
        [IntRange] _StencilReadMask ("Stencil Read Mask", Range(0, 255)) = 255
        [IntRange] _StencilWriteMask ("Stencil Write Mask", Range(0, 255)) = 255
        [Enum(UnityEngine.Rendering.CompareFunction)]
        _StencilComp ("Stencil Comparison", Int) = 3 // Equal by default
        [Enum(UnityEngine.Rendering.StencilOp)]
        _StencilOp ("Stencil Operation", Int) = 0 // Keep by default
        
        [Header(Render Settings)]
        [Enum(UnityEngine.Rendering.CompareFunction)] 
        _ZTest("ZTest", Int) = 4 // LEqual
        [Enum(Off, 0, On, 1)] 
        _ZWrite("ZWrite", Int) = 1 // On
        [Enum(UnityEngine.Rendering.BlendMode)] 
        _SrcBlend("Src Blend Mode", Int) = 5 // SrcAlpha
        [Enum(UnityEngine.Rendering.BlendMode)] 
        _DstBlend("Dst Blend Mode", Int) = 10 // OneMinusSrcAlpha
        [Enum(UnityEngine.Rendering.CullMode)] 
        _Cull("Cull Mode", Int) = 2 // Back
    }

    SubShader
    {
        Tags 
        { 
            "RenderType" = "Transparent"
            "Queue" = "Transparent+100"
            "IgnoreProjector" = "True"
            "PreviewType" = "Plane"
        }

        Stencil
        {
            Ref [_Stencil]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
            Comp [_StencilComp]
            Pass [_StencilOp]
        }

        Lighting Off
        ZWrite [_ZWrite]
        ZTest [_ZTest]
        Blend [_SrcBlend] [_DstBlend]
        Cull [_Cull]

        Pass
        {
            Name "Forward"
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #pragma multi_compile _ UNITY_UI_CLIP_RECT
            #pragma multi_compile _ UNITY_UI_ALPHACLIP

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
                float4 color : COLOR;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;
            float4 _ClipRect;
            
            UNITY_INSTANCING_BUFFER_START(Props)
            UNITY_INSTANCING_BUFFER_END(Props)

            v2f vert(appdata v)
            {
                v2f o;
                
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                
                o.worldPosition = v.vertex;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color * _Color;
                
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // 采样纹理并应用颜色
                fixed4 color = tex2D(_MainTex, i.uv) * i.color;
                
                #ifdef UNITY_UI_CLIP_RECT
                color.a *= UnityGet2DClipping(i.worldPosition.xy, _ClipRect);
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                clip(color.a - 0.001);
                #endif
                
                return color;
            }
            ENDCG
        }
    }

    // 后备shader
    FallBack "UI/Default"
    CustomEditor "UnityEditor.UI.MaskedShaderGUI"
}