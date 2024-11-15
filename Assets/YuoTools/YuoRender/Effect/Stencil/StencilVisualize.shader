  Shader "Debug/UIStencilVisualize"
  {
      Properties
      {
          _ColorMultiplier ("Color Multiplier", Range(1, 10)) = 1
          [Enum(UnityEngine.Rendering.CompareFunction)]
          _StencilComp ("Stencil Comparison", Float) = 3    // 默认为Equal
          _StencilRef ("Stencil Reference", Range(0, 255)) = 0
          _StencilReadMask ("Stencil Read Mask", Float) = 255
          _StencilWriteMask ("Stencil Write Mask", Float) = 255
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
          
          ZWrite Off
          ZTest Always
          Blend SrcAlpha OneMinusSrcAlpha
          ColorMask RGBA
          
          Stencil
          {
              Ref [_StencilRef]
              ReadMask [_StencilReadMask]
              WriteMask [_StencilWriteMask]
              Comp [_StencilComp]
              Pass Keep
              Fail Keep
              ZFail Keep
          }
          
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
                  float4 vertex : SV_POSITION;
                  float2 uv : TEXCOORD0;
                  float4 screenPos : TEXCOORD1;
              };
              
              float _ColorMultiplier;
              float _StencilRef;
              float _StencilOp;
              float _StencilComp;
              
              v2f vert (appdata v)
              {
                  v2f o;
                  o.vertex = UnityObjectToClipPos(v.vertex);
                  o.uv = v.uv;
                  o.screenPos = ComputeScreenPos(o.vertex);
                  return o;
              }
              
              float3 HSVToRGB(float3 hsv)
              {
                  float4 K = float4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
                  float3 p = abs(frac(hsv.xxx + K.xyz) * 6.0 - K.www);
                  return hsv.z * lerp(K.xxx, saturate(p - K.xxx), hsv.y);
              }
              
              fixed4 frag (v2f i) : SV_Target
              {
                  uint stencilValue = (uint)_StencilRef;
                  
                  // 将模板值映射到HSV颜色空间
                  float hue = (float)stencilValue / 8.0; // 调整为更明显的颜色变化
                  float3 hsv = float3(hue, 0.8, 1.0);
                  float3 rgb = HSVToRGB(hsv);
                  
                  // 添加网格图案以更容易识别区域
                  float2 screenUV = i.screenPos.xy / i.screenPos.w;
                  float2 grid = frac(screenUV * _ScreenParams.xy / 20.0);
                  float gridPattern = step(0.9, grid.x) + step(0.9, grid.y);
                  
                  rgb = lerp(rgb, float3(1,1,1), gridPattern * 0.2);
                  rgb *= _ColorMultiplier;
                  
                  // 使用更高的基础透明度
                  float alpha = 0.8;
                  
                  return float4(rgb, alpha);
              }
              ENDCG
          }
      }
  }