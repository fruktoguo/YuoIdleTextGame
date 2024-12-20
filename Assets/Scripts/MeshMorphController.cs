using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;

public class MeshMorphController : MonoBehaviour
{
    public Mesh sourceMesh;
    public Mesh targetMesh;

    [Range(0, 1)]
    [OnValueChanged(nameof(UpdateMesh))]
    public float progress;

    private Mesh currentMesh;
    private MeshFilter meshFilter;
    private Vector3[] sourceVertices;
    private Vector3[] sourceNormals;
    private Vector2[] sourceUVs;
    private Vector3[] targetVertices;
    private Vector3[] targetNormals;
    private Vector2[] targetUVs;
    private int[] triangles;

    // 用于存储顶点映射关系
    private int[] sourceToTargetMapping;
    private int[] targetToSourceMapping;
    private float[] vertexWeights;

    // 缓存数组，避免重复创建
    private Vector3[] vertices;
    private Vector3[] normals;
    private Vector2[] uvs;

    // 顶点匹配数据结构
    private class VertexMatch
    {
        public int targetIndex;
        public float distance;
        
        public VertexMatch(int index, float dist)
        {
            targetIndex = index;
            distance = dist;
        }
    }

    private void Start()
    {
        InitializeMeshes();
    }

    private void InitializeMeshes()
    {
        meshFilter = GetComponent<MeshFilter>();
        if (meshFilter == null)
        {
            meshFilter = gameObject.AddComponent<MeshFilter>();
        }

        currentMesh = new Mesh();
        currentMesh.name = "MorphedMesh";
        meshFilter.mesh = currentMesh;

        // 初始化网格数据
        sourceVertices = sourceMesh.vertices;
        sourceNormals = sourceMesh.normals;
        sourceUVs = sourceMesh.uv;
        
        targetVertices = targetMesh.vertices;
        targetNormals = targetMesh.normals;
        targetUVs = targetMesh.uv;

        // 使用顶点数较多的网格的三角形索引
        if (sourceMesh.vertexCount >= targetMesh.vertexCount)
        {
            triangles = sourceMesh.triangles;
        }
        else
        {
            triangles = targetMesh.triangles;
        }

        // 使用较大的顶点数来初始化数组
        int maxVertexCount = Mathf.Max(sourceMesh.vertexCount, targetMesh.vertexCount);
        vertices = new Vector3[maxVertexCount];
        normals = new Vector3[maxVertexCount];
        uvs = new Vector2[maxVertexCount];

        // 初始化映射数组
        sourceToTargetMapping = new int[sourceMesh.vertexCount];
        targetToSourceMapping = new int[targetMesh.vertexCount];
        vertexWeights = new float[maxVertexCount];

        // 计算双向顶点映射关系
        CalculateVertexMapping();

        UpdateMesh(0);
    }

    private void CalculateVertexMapping()
    {
        // 用于跟踪顶点使用次数
        int[] targetVertexUsageCount = new int[targetVertices.Length];
        int[] sourceVertexUsageCount = new int[sourceVertices.Length];

        // 为每个顶点存储所有可能的匹配（按距离排序）
        var sourceToTargetDistances = new List<VertexMatch>[sourceVertices.Length];
        var targetToSourceDistances = new List<VertexMatch>[targetVertices.Length];

        // 初始化列表
        for (int i = 0; i < sourceVertices.Length; i++)
        {
            sourceToTargetDistances[i] = new List<VertexMatch>();
        }
        for (int i = 0; i < targetVertices.Length; i++)
        {
            targetToSourceDistances[i] = new List<VertexMatch>();
        }

        // 计算所有源顶点到目标顶点的距离
        for (int i = 0; i < sourceVertices.Length; i++)
        {
            for (int j = 0; j < targetVertices.Length; j++)
            {
                float dist = Vector3.Distance(sourceVertices[i], targetVertices[j]);
                sourceToTargetDistances[i].Add(new VertexMatch(j, dist));
            }
            sourceToTargetDistances[i].Sort((a, b) => a.distance.CompareTo(b.distance));
        }

        // 计算所有目标顶点到源顶点的距离
        for (int i = 0; i < targetVertices.Length; i++)
        {
            for (int j = 0; j < sourceVertices.Length; j++)
            {
                float dist = Vector3.Distance(targetVertices[i], sourceVertices[j]);
                targetToSourceDistances[i].Add(new VertexMatch(j, dist));
            }
            targetToSourceDistances[i].Sort((a, b) => a.distance.CompareTo(b.distance));
        }

        // 计算每个顶点的最大使用次数
        int maxSourceUsage = Mathf.CeilToInt((float)targetVertices.Length / sourceVertices.Length);
        int maxTargetUsage = Mathf.CeilToInt((float)sourceVertices.Length / targetVertices.Length);

        // 为源顶点分配目标顶点
        for (int i = 0; i < sourceVertices.Length; i++)
        {
            bool matched = false;
            foreach (var match in sourceToTargetDistances[i])
            {
                if (targetVertexUsageCount[match.targetIndex] < maxTargetUsage)
                {
                    sourceToTargetMapping[i] = match.targetIndex;
                    targetVertexUsageCount[match.targetIndex]++;
                    vertexWeights[i] = 1.0f / (match.distance + 0.0001f);
                    matched = true;
                    break;
                }
            }

            // 如果没有找到匹配，使用最近的点
            if (!matched)
            {
                sourceToTargetMapping[i] = sourceToTargetDistances[i][0].targetIndex;
                vertexWeights[i] = 1.0f / (sourceToTargetDistances[i][0].distance + 0.0001f);
            }
        }

        // 为目标顶点分配源顶点
        for (int i = 0; i < targetVertices.Length; i++)
        {
            bool matched = false;
            foreach (var match in targetToSourceDistances[i])
            {
                if (sourceVertexUsageCount[match.targetIndex] < maxSourceUsage)
                {
                    targetToSourceMapping[i] = match.targetIndex;
                    sourceVertexUsageCount[match.targetIndex]++;
                    matched = true;
                    break;
                }
            }

            // 如果没有找到匹配，使用最近的点
            if (!matched)
            {
                targetToSourceMapping[i] = targetToSourceDistances[i][0].targetIndex;
            }
        }

        Debug.Log($"Max source usage: {maxSourceUsage}, Max target usage: {maxTargetUsage}");
        Debug.Log($"Source vertices: {sourceVertices.Length}, Target vertices: {targetVertices.Length}");
    }

    public void UpdateMesh(float newProgress)
    {
        if (!Application.isPlaying) return;

        progress = Mathf.Clamp01(newProgress);

        // 获取顶点数较多的网格的顶点数
        int maxVertexCount = Mathf.Max(sourceVertices.Length, targetVertices.Length);

        // 如果源网格顶点数较多
        if (sourceVertices.Length >= targetVertices.Length)
        {
            for (int i = 0; i < sourceVertices.Length; i++)
            {
                int targetIndex = sourceToTargetMapping[i];
                vertices[i] = Vector3.Lerp(sourceVertices[i], targetVertices[targetIndex], progress);
                normals[i] = Vector3.Lerp(sourceNormals[i], targetNormals[targetIndex], progress).normalized;
                uvs[i] = Vector2.Lerp(sourceUVs[i], targetUVs[targetIndex], progress);
            }
        }
        // 如果目标网格顶点数较多
        else
        {
            for (int i = 0; i < targetVertices.Length; i++)
            {
                int sourceIndex = targetToSourceMapping[i];
                vertices[i] = Vector3.Lerp(sourceVertices[sourceIndex], targetVertices[i], progress);
                normals[i] = Vector3.Lerp(sourceNormals[sourceIndex], targetNormals[i], progress).normalized;
                uvs[i] = Vector2.Lerp(sourceUVs[sourceIndex], targetUVs[i], progress);
            }
        }

        // 更新网格
        currentMesh.Clear();
        currentMesh.vertices = vertices.Take(maxVertexCount).ToArray();
        currentMesh.normals = normals.Take(maxVertexCount).ToArray();
        currentMesh.uv = uvs.Take(maxVertexCount).ToArray();
        currentMesh.triangles = triangles;
        currentMesh.RecalculateBounds();
    }
}
