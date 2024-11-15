  Shader "Custom/MetaBalls"
  {
      Properties
      {
          _MainTex ("Texture", 2D) = "white" {}
          _Threshold ("Threshold", Range(0.1, 2.0)) = 1.0
          _Smoothness ("Smoothness", Range(0.001, 0.5)) = 0.01
          _Stickiness ("Stickiness", Range(0.1, 4.0)) = 1.0
          _ColorBlendFactor ("Color Blend Factor", Range(0, 1)) = 0.5
      }

      SubShader
      {
          Tags
          {
              "RenderType"="Transparent"
              "Queue"="Transparent"
          }

          Blend SrcAlpha OneMinusSrcAlpha
          ZWrite Off

          Pass
          {
              CGPROGRAM
              #pragma vertex vert
              #pragma fragment frag
              #pragma target 3.0

              #include "UnityCG.cginc"

              struct BallData
              {
                  float2 position;
                  float radius;
                  float4 color;  // 添加颜色属性
              };

              struct appdata
              {
                  float4 vertex : POSITION;
                  float2 uv : TEXCOORD0;
              };

              struct v2f
              {
                  float2 uv : TEXCOORD0;
                  float4 vertex : SV_POSITION;
                  float3 worldPos : TEXCOORD1;
              };

              sampler2D _MainTex;
              float4 _MainTex_ST;
              float _Threshold;
              float _Smoothness;
              float _Stickiness;
              float _ColorBlendFactor;

              StructuredBuffer<BallData> _BallBuffer;
              int _BallCount;

              v2f vert(appdata v)
              {
                  v2f o;
                  o.vertex = UnityObjectToClipPos(v.vertex);
                  o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                  o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                  return o;
              }

              float calculateMetaball(float2 p, float2 center, float radius)
              {
                  float d = distance(p, center);
                  return pow(radius / max(d, 0.01), _Stickiness);
              }

              fixed4 frag(v2f i) : SV_Target
              {
                  float sum = 0;
                  float2 worldPos2D = i.worldPos.xy;
                  
                  // 用于存储颜色混合
                  float4 finalColor = float4(0,0,0,0);
                  float totalWeight = 0;

                  // 计算所有球体的影响
                  for (int idx = 0; idx < _BallCount; idx++)
                  {
                      BallData ball = _BallBuffer[idx];
                      float fieldStrength = calculateMetaball(worldPos2D, ball.position, ball.radius);
                      sum += fieldStrength;
                      
                      // 根据场强权重混合颜色
                      finalColor += ball.color * fieldStrength * ball.color.a;
                      totalWeight += fieldStrength;
                  }

                  // 标准化颜色
                  finalColor = totalWeight > 0 ? finalColor / totalWeight : float4(0,0,0,0);
                  
                  // 使用阈值和平滑度创建融合效果
                  float smoothedAlpha = smoothstep(_Threshold - _Smoothness, _Threshold + _Smoothness, sum);

                  return float4(finalColor.rgb, smoothedAlpha * finalColor.a);
              }
              ENDCG
          }
      }
  }