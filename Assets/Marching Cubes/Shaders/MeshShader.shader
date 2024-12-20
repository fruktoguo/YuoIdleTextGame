Shader "Custom/MeshShader"
{
    // 在Inspector面板中显示的属性
    Properties
    {
        _BaseMap("基础贴图", 2D) = "white" {}
        _BaseColor("基础颜色", Color) = (1, 1, 1, 1)
        _Metallic("金属度", Range(0, 1)) = 0
        _Smoothness("光滑度", Range(0, 1)) = 0.5
    }
    
    SubShader
    {
        // 渲染设置标签
        Tags { 
            "RenderType" = "Opaque"          // 渲染类型：不透明
            "RenderPipeline" = "UniversalPipeline"  // 指定为URP管线
            "Queue" = "Geometry"              // 渲染队列：几何体
        }

        Pass {
            Name "ForwardLit"    // Pass名称：前向渲染
            Tags { "LightMode" = "UniversalForward" }  // 指定为URP前向渲染Pass

            HLSLPROGRAM
            #pragma vertex LitPassVertex       // 指定顶点着色器
            #pragma fragment LitPassFragment   // 指定片段着色器
            
            // 编译指令：启用各种光照特性
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS           // 主光源阴影
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE   // 阴影级联
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS  // 额外光源
            #pragma multi_compile _ _SHADOWS_SOFT                 // 软阴影
            #pragma multi_compile_fog                            // 雾效

            // 引入URP必要的库
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            // 定义材质属性
            TEXTURE2D(_BaseMap);              // 声明基础贴图
            SAMPLER(sampler_BaseMap);         // 声明采样器

            // 材质属性缓冲区
            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;           // 贴图的缩放和偏移
                float4 _BaseColor;            // 基础颜色
                float _Metallic;              // 金属度
                float _Smoothness;            // 光滑度
            CBUFFER_END

            // 顶点着色器输入结构
            struct Attributes
            {
                float4 positionOS : POSITION;      // 物体空间位置
                float3 normalOS : NORMAL;          // 物体空间法线
                float2 uv : TEXCOORD0;            // UV坐标
            };

            // 顶点着色器输出结构（片段着色器输入）
            struct Varyings
            {
                float4 positionCS : SV_POSITION;   // 裁剪空间位置
                float2 uv : TEXCOORD0;            // UV坐标
                float3 positionWS : TEXCOORD1;    // 世界空间位置
                float3 normalWS : TEXCOORD2;      // 世界空间法线
                float3 viewDirWS : TEXCOORD3;     // 世界空间视角方向
            };

            // 顶点着色器
            Varyings LitPassVertex(Attributes input)
            {
                Varyings output;

                // 坐标转换
                VertexPositionInputs positionInputs = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs normalInputs = GetVertexNormalInputs(input.normalOS);

                // 填充输出结构
                output.positionCS = positionInputs.positionCS;
                output.positionWS = positionInputs.positionWS;
                output.normalWS = normalInputs.normalWS;
                output.viewDirWS = GetWorldSpaceViewDir(positionInputs.positionWS);
                
                // 计算UV（应用贴图的缩放和偏移）
                output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
                
                return output;
            }

            // 片段着色器
            float4 LitPassFragment(Varyings input) : SV_Target
            {
                // 采样贴图并与基础颜色相乘
                float4 baseMap = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv);
                float4 color = baseMap * _BaseColor;
                
                // 准备光照计算所需的输入数据
                InputData inputData = (InputData)0;
                inputData.positionWS = input.positionWS;
                inputData.normalWS = normalize(input.normalWS);
                inputData.viewDirectionWS = normalize(input.viewDirWS);
                inputData.shadowCoord = TransformWorldToShadowCoord(input.positionWS);
                inputData.fogCoord = 0;
                inputData.vertexLighting = float3(0, 0, 0);
                inputData.bakedGI = float3(0, 0, 0);
                inputData.normalizedScreenSpaceUV = float2(0, 0);
                inputData.shadowMask = float4(1, 1, 1, 1);
                inputData.tangentToWorld = float3x3(float3(1, 0, 0), float3(0, 1, 0), inputData.normalWS);

                // 设置表面属性
                SurfaceData surfaceData = (SurfaceData)0;
                surfaceData.albedo = color.rgb;        // 反照率
                surfaceData.metallic = _Metallic;      // 金属度
                surfaceData.specular = float3(0.0h, 0.0h, 0.0h);  // 高光颜色
                surfaceData.smoothness = _Smoothness;   // 光滑度
                surfaceData.normalTS = float3(0, 0, 1); // 切线空间法线
                surfaceData.emission = float3(0, 0, 0); // 自发光
                surfaceData.occlusion = 1;             // 环境光遮蔽
                surfaceData.alpha = color.a;           // 透明度
                surfaceData.clearCoatMask = 0;         // 清漆遮罩
                surfaceData.clearCoatSmoothness = 1;   // 清漆光滑度

                // 使用URP的PBR光照计算并返回最终颜色
                return UniversalFragmentPBR(inputData, surfaceData);
            }
            ENDHLSL
        }
    }
}