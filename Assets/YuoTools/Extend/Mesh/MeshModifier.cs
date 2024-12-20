using UnityEngine;
using System.Collections.Generic;

public class MeshModifier
{
    private Mesh targetMesh;
    private Transform targetTransform;
    private Vector3[] vertices;
    private Vector3[] normals;
    private int[] triangles;
    private Vector2[] uvs;

    // 添加网格细分阈值
    private const float SUBDIVISION_EDGE_LENGTH = 0.3f; // 当边长超过这个值时进行细分

    public MeshModifier(Mesh mesh, Transform transform)
    {
        targetMesh = mesh;
        targetTransform = transform;
        RefreshMeshData();
    }

    private void RefreshMeshData()
    {
        vertices = targetMesh.vertices;
        normals = targetMesh.normals;
        triangles = targetMesh.triangles;
        uvs = targetMesh.uv;
    }

    /// <summary>
    /// 在变形之前进行网格细分
    /// </summary>
    private void SubdivideMeshInArea(Vector3 position, float radius)
    {
        Vector3 localPosition = targetTransform.InverseTransformPoint(position);
        List<Vector3> newVertices = new List<Vector3>(vertices);
        List<Vector3> newNormals = new List<Vector3>(normals);
        List<Vector2> newUVs = new List<Vector2>(uvs);
        List<int> newTriangles = new List<int>();

        // 记录需要细分的三角形
        HashSet<int> trianglesToSubdivide = new HashSet<int>();

        // 检查哪些三角形需要细分
        for (int i = 0; i < triangles.Length; i += 3)
        {
            Vector3 v1 = vertices[triangles[i]];
            Vector3 v2 = vertices[triangles[i + 1]];
            Vector3 v3 = vertices[triangles[i + 2]];

            // 计算三角形中心
            Vector3 center = (v1 + v2 + v3) / 3f;
            float distanceToCenter = Vector3.Distance(center, localPosition);

            // 如果三角形在影响范围内，且边长超过阈值，则进行细分
            if (distanceToCenter < radius)
            {
                float maxEdgeLength = Mathf.Max(
                    Vector3.Distance(v1, v2),
                    Vector3.Distance(v2, v3),
                    Vector3.Distance(v3, v1)
                );

                if (maxEdgeLength > SUBDIVISION_EDGE_LENGTH)
                {
                    trianglesToSubdivide.Add(i / 3);
                }
            }
        }

        // 对需要细分的三角形进行处理
        for (int i = 0; i < triangles.Length; i += 3)
        {
            if (trianglesToSubdivide.Contains(i / 3))
            {
                // 获取原始三角形的顶点索引
                int i1 = triangles[i];
                int i2 = triangles[i + 1];
                int i3 = triangles[i + 2];

                // 计算新的顶点（边的中点）
                Vector3 v1 = vertices[i1];
                Vector3 v2 = vertices[i2];
                Vector3 v3 = vertices[i3];

                Vector3 v12 = Vector3.Lerp(v1, v2, 0.5f);
                Vector3 v23 = Vector3.Lerp(v2, v3, 0.5f);
                Vector3 v31 = Vector3.Lerp(v3, v1, 0.5f);

                // 添加新顶点
                int i12 = newVertices.Count;
                newVertices.Add(v12);
                newNormals.Add(Vector3.Lerp(normals[i1], normals[i2], 0.5f).normalized);
                newUVs.Add(Vector2.Lerp(uvs[i1], uvs[i2], 0.5f));

                int i23 = newVertices.Count;
                newVertices.Add(v23);
                newNormals.Add(Vector3.Lerp(normals[i2], normals[i3], 0.5f).normalized);
                newUVs.Add(Vector2.Lerp(uvs[i2], uvs[i3], 0.5f));

                int i31 = newVertices.Count;
                newVertices.Add(v31);
                newNormals.Add(Vector3.Lerp(normals[i3], normals[i1], 0.5f).normalized);
                newUVs.Add(Vector2.Lerp(uvs[i3], uvs[i1], 0.5f));

                // 创建四个新三角形
                newTriangles.AddRange(new int[] { i1, i12, i31 });
                newTriangles.AddRange(new int[] { i12, i2, i23 });
                newTriangles.AddRange(new int[] { i31, i23, i3 });
                newTriangles.AddRange(new int[] { i12, i23, i31 });
            }
            else
            {
                // 保持原始三角形不变
                newTriangles.Add(triangles[i]);
                newTriangles.Add(triangles[i + 1]);
                newTriangles.Add(triangles[i + 2]);
            }
        }

        // 更新网格数据
        vertices = newVertices.ToArray();
        normals = newNormals.ToArray();
        triangles = newTriangles.ToArray();
        uvs = newUVs.ToArray();

        // 应用新的网格数据
        targetMesh.Clear();
        targetMesh.vertices = vertices;
        targetMesh.normals = normals;
        targetMesh.triangles = triangles;
        targetMesh.uv = uvs;
        targetMesh.RecalculateNormals();
        targetMesh.RecalculateBounds();
    }

    public void CreateBump(Vector3 position, float radius, float height, float falloff)
    {
        // 在变形之前先进行网格细分
        SubdivideMeshInArea(position, radius);

        Vector3 localPosition = targetTransform.InverseTransformPoint(position);

        for (int i = 0; i < vertices.Length; i++)
        {
            float distance = Vector3.Distance(vertices[i], localPosition);
            if (distance < radius)
            {
                float factor = 1 - (distance / radius);
                factor = Mathf.Pow(factor, falloff);
                vertices[i] += normals[i] * height * factor;
            }
        }

        ApplyModification();
    }

    /// <summary>
    /// 在指定位置创建凹陷/挖洞效果
    /// </summary>
    /// <param name="position">世界空间中的位置</param>
    /// <param name="radius">影响半径</param>
    /// <param name="depth">凹陷深度</param>
    /// <param name="falloff">衰减系数(0-1)</param>
    public void CreateDent(Vector3 position, float radius, float depth, float falloff)
    {
        CreateBump(position, radius, -depth, falloff);
    }

    /// <summary>
    /// 平滑指定区域的网格
    /// </summary>
    /// <param name="position">世界空间中的位置</param>
    /// <param name="radius">影响半径</param>
    /// <param name="strength">平滑强度</param>
    public void SmoothArea(Vector3 position, float radius, float strength)
    {
        Vector3 localPosition = targetTransform.InverseTransformPoint(position);
        Dictionary<int, List<int>> vertexNeighbors = FindVertexNeighbors();

        Vector3[] smoothedVertices = new Vector3[vertices.Length];
        System.Array.Copy(vertices, smoothedVertices, vertices.Length);

        for (int i = 0; i < vertices.Length; i++)
        {
            float distance = Vector3.Distance(vertices[i], localPosition);
            if (distance < radius)
            {
                Vector3 averagePosition = Vector3.zero;
                List<int> neighbors = vertexNeighbors[i];

                foreach (int neighborIndex in neighbors)
                {
                    averagePosition += vertices[neighborIndex];
                }

                if (neighbors.Count > 0)
                {
                    averagePosition /= neighbors.Count;
                    float factor = 1 - (distance / radius);
                    smoothedVertices[i] = Vector3.Lerp(vertices[i], averagePosition, strength * factor);
                }
            }
        }

        vertices = smoothedVertices;
        ApplyModification();
    }

    /// <summary>
    /// 找到每个顶点的相邻顶点
    /// </summary>
    private Dictionary<int, List<int>> FindVertexNeighbors()
    {
        Dictionary<int, List<int>> neighbors = new Dictionary<int, List<int>>();

        // 初始化字典
        for (int i = 0; i < vertices.Length; i++)
        {
            neighbors[i] = new List<int>();
        }

        // 遍历三角形找到相邻顶点
        for (int i = 0; i < triangles.Length; i += 3)
        {
            int v1 = triangles[i];
            int v2 = triangles[i + 1];
            int v3 = triangles[i + 2];

            if (!neighbors[v1].Contains(v2)) neighbors[v1].Add(v2);
            if (!neighbors[v1].Contains(v3)) neighbors[v1].Add(v3);
            if (!neighbors[v2].Contains(v1)) neighbors[v2].Add(v1);
            if (!neighbors[v2].Contains(v3)) neighbors[v2].Add(v3);
            if (!neighbors[v3].Contains(v1)) neighbors[v3].Add(v1);
            if (!neighbors[v3].Contains(v2)) neighbors[v3].Add(v2);
        }

        return neighbors;
    }

    /// <summary>
    /// 应用网格修改
    /// </summary>
    private void ApplyModification()
    {
        targetMesh.vertices = vertices;
        targetMesh.RecalculateNormals();
        targetMesh.RecalculateBounds();
    }

    /// <summary>
    /// 重置网格到原始状态
    /// </summary>
    public void Reset()
    {
        vertices = targetMesh.vertices;
        normals = targetMesh.normals;
        triangles = targetMesh.triangles;
        uvs = targetMesh.uv;
        ApplyModification();
    }
}