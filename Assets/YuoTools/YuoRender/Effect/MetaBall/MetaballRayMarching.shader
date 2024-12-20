  Shader "Custom/MetaballImproved"
  {
      Properties
      {
          _Color ("Color", Color) = (0,1,1,1)
          _Glossiness ("Smoothness", Range(0,1)) = 0.8
          _Metallic ("Metallic", Range(0,1)) = 0.2
          _RimColor ("Rim Color", Color) = (1,1,1,1)
          _RimPower ("Rim Power", Range(0.5,8.0)) = 3.0
      }
      SubShader
      {
          Tags { "RenderType"="Opaque" }
          LOD 200

          CGPROGRAM
          #pragma surface surf Standard fullforwardshadows
          #pragma target 3.0

          struct Input
          {
              float3 viewDir;
              float3 worldPos;
          };

          fixed4 _Color;
          fixed4 _RimColor;
          half _Glossiness;
          half _Metallic;
          float _RimPower;

          void surf (Input IN, inout SurfaceOutputStandard o)
          {
              // 基础颜色
              o.Albedo = _Color.rgb;
              
              // 金属度和光滑度
              o.Metallic = _Metallic;
              o.Smoothness = _Glossiness;
              
              // 边缘光
              half rim = 1.0 - saturate(dot(normalize(IN.viewDir), o.Normal));
              o.Emission = _RimColor.rgb * pow(rim, _RimPower);
          }
          ENDCG
      }
      FallBack "Diffuse"
  }