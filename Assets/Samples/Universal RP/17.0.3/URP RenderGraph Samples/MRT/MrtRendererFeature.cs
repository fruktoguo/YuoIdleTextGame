using System;
using UnityEngine;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

// 在此示例中展示了如何在使用URP的RenderGraph中使用多重渲染目标（MRT）。当一个通道需要写入超过4个通道的数据（单个RGBA纹理）时，这非常有用。
public class MrtRendererFeature : ScriptableRendererFeature
{
    // 此通道使用MRT并将输出到3个不同的渲染目标。
    class MrtPass : ScriptableRenderPass
    {
        // 记录后传递给渲染函数的数据。
        class PassData
        {
            // 颜色输入的纹理句柄。
            public TextureHandle color;
            // 材质的输入纹理名称。
            public string texName;
            // 用于MRT通道的材质。
            public Material material;
        }

        // 材质的输入纹理名称。
        string m_texName;
        // 用于MRT通道的材质。
        Material m_Material;
        // MRT目标的RTHandle输出。
        RTHandle[] m_RTs = new RTHandle[3];
        RenderTargetInfo[] m_RTInfos = new RenderTargetInfo[3];

        // 从渲染器特性传递材质到渲染通道的函数。
        public void Setup(string texName, Material material, RenderTexture[] renderTextures)
        {
            m_Material = material;
            m_texName = String.IsNullOrEmpty(texName) ? "_ColorTexture" : texName;

            // 如果RenderTextures发生变化，则创建RTHandles。
            for (int i = 0; i < 3; i++)
            {
                if (m_RTs[i] == null || m_RTs[i].rt != renderTextures[i])
                {
                    m_RTs[i]?.Release();
                    m_RTs[i] = RTHandles.Alloc(renderTextures[i], $"ChannelTexture[{i}]");
                    m_RTInfos[i] = new RenderTargetInfo()
                    {
                        format = renderTextures[i].graphicsFormat,
                        height = renderTextures[i].height,
                        width = renderTextures[i].width,
                        bindMS = renderTextures[i].bindTextureMS,
                        msaaSamples = 1,
                        volumeDepth = renderTextures[i].volumeDepth,
                    };
                }
            }
        }

        // 此函数为给定材质绘制整个屏幕。
        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            var handles = new TextureHandle[3];
            // 将纹理句柄导入RenderGraph。
            for (int i = 0; i < 3; i++)
            {
                handles[i] = renderGraph.ImportTexture(m_RTs[i], m_RTInfos[i]);
            }
            // 开始记录给定名称的渲染图通道，并输出用于传递数据以执行渲染函数的数据。
            using (var builder = renderGraph.AddRasterRenderPass<PassData>("MRT Pass", out var passData))
            {
                // 获取通用资源数据以提取摄像机的颜色附件。
                var resourceData = frameData.Get<UniversalResourceData>();

                // 填充渲染函数使用的通道数据。
                // 使用摄像机的颜色附件作为输入。
                passData.color = resourceData.activeColorTexture;
                // 材质的输入纹理名称。
                passData.texName = m_texName;
                // 通道中使用的材质。
                passData.material = m_Material;

                // 设置输入附件。
                builder.UseTexture(passData.color);
                // 设置颜色附件。
                for (int i = 0; i < 3; i++)
                {
                    builder.SetRenderAttachment(handles[i], i);
                }

                // 设置渲染函数。
                builder.SetRenderFunc((PassData data, RasterGraphContext rgContext) => ExecutePass(data, rgContext));
            }
        }

        // ExecutePass 是每个blit渲染图记录的渲染函数。
        // 这是一种良好的实践，以避免使用调用它的lambda之外的变量。
        // 它是静态的，以避免使用成员变量，这可能会导致意外行为。
        static void ExecutePass(PassData data, RasterGraphContext rgContext)
        {
            // 将输入颜色纹理设置为MRTPass中使用的名称。
            data.material.SetTexture(data.texName, data.color);
            // 使用MRT着色器绘制全屏三角形。
            rgContext.cmd.DrawProcedural(Matrix4x4.identity, data.material, 0, MeshTopology.Triangles, 3);
        }
    }

    [Tooltip("在制作MRT通道时使用的材质。")]
    public Material mrtMaterial;
    [Tooltip("将摄像机的颜色附件应用于给定材质的名称。")]
    public string textureName = "_ColorTexture";
    [Tooltip("输出结果的渲染纹理。必须有3个元素。")]
    public RenderTexture[] renderTextures = new RenderTexture[3];

    MrtPass m_MrtPass;

    // 在这里可以创建通道并进行初始化。每次序列化时都会调用此方法。
    public override void Create()
    {
        m_MrtPass = new MrtPass();

        // 配置渲染通道应注入的位置。
        m_MrtPass.renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;
    }

    // 在这里可以注入一个或多个渲染通道到渲染器中。
    // 当设置渲染器时，每台摄像机都会调用此方法。
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        // 由于它们具有相同的RenderPassEvent，因此在排队时顺序很重要。

        // 如果没有材质，则提前退出。
        if (mrtMaterial == null || renderTextures.Length != 3)
        {
            Debug.LogWarning("由于材质为空或渲染纹理的大小不是3，跳过MRTPass。");
            return;
        }

        foreach (var rt in renderTextures)
        {
            if (rt == null)
            {
                Debug.LogWarning("由于其中一个渲染纹理为空，跳过MRTPass。");
                return;
            }
        }

        // 调用通道Setup函数，将RendererFeature设置传递到RenderPass。
        m_MrtPass.Setup(textureName, mrtMaterial, renderTextures);
        renderer.EnqueuePass(m_MrtPass);
    }
}
