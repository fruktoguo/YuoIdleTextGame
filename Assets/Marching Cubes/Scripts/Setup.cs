using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;

// 该脚本用于设置场景并更新顶点数据
public class Setup : MonoBehaviour
{
    GameObject marchingCubes;

    // 网格尺寸参数
    public int nX = 10;
    public int nY = 10;
    public int nZ = 10;

    public float gridSize = 1f;
    public float thresholdValue = 0.5f;
    public Material meshMaterial;
    public bool vertexWelding = true;

    public NativeQueue<Triangle> triangleQueue;

    // DOTS风格的顶点数据列表
    List<Vector3> vertexList;
    Dictionary<Vector3, int> vertexDict;
    List<Vector3> normalList;
    List<int> indexList;

    Mesh mesh;
    GameObject meshGameObject;

    // 缓存组件引用
    private Potential potentialComponent;
    private MarchingCubes marchingCubesComponent;

    void Start()
    {
        marchingCubes = this.gameObject;
        potentialComponent = GetComponent<Potential>();
        marchingCubesComponent = GetComponent<MarchingCubes>();
        potentialComponent.BuildScalarField(nX, nY, nZ, gridSize);
    }

    void Update()
    {
        UpdateMesh();

        // 重建网格游戏对象
        Destroy(meshGameObject);
        meshGameObject = new GameObject("Marching Cubes Mesh");
        meshGameObject.transform.parent = transform;
        var meshFilter = meshGameObject.AddComponent<MeshFilter>();
        var meshRenderer = meshGameObject.AddComponent<MeshRenderer>();
        meshRenderer.material = meshMaterial;
    }

    void LateUpdate()
    {
        // 等待网格创建任务完成
        marchingCubesComponent.triangleListModificationJobHandle.Complete();

        CreateVertexIndexNormalListsFromTriangles(triangleQueue, ref vertexList, ref indexList, ref normalList,
            ref vertexDict);

        triangleQueue.Dispose();
        marchingCubesComponent.flagList.Dispose();

        // 创建并设置网格数据
        mesh = new Mesh();
        mesh.SetVertices(vertexList);
        mesh.SetTriangles(indexList, 0);
        mesh.normals = NormalizedArrayFromList(normalList);

        meshGameObject.GetComponent<MeshFilter>().mesh = mesh;
    }

    // 重新计算标量场,创建网格所需的列表和字典,并创建包含所有三角形的NativeQueue
    void UpdateMesh()
    {
        potentialComponent.BuildScalarField(nX, nY, nZ, gridSize);

        vertexList = new List<Vector3>();
        vertexDict = new Dictionary<Vector3, int>();
        normalList = new List<Vector3>();
        indexList = new List<int>();

        triangleQueue = new NativeQueue<Triangle>(Allocator.TempJob);

        marchingCubesComponent.GetVerticesFromField(potentialComponent.scalarField, thresholdValue);
    }

    // 将Vector3列表转换为标准化的Vector3数组,用于网格法线
    Vector3[] NormalizedArrayFromList(List<Vector3> input)
    {
        Vector3[] output = new Vector3[input.Count];

        for (int i = 0; i < input.Count; i++)
        {
            Vector3 normal = new Vector3(input[i].x, input[i].y, input[i].z);
            output[i] = normal.normalized;
        }

        return output;
    }

    // 从三角形NativeQueue中提取数据,生成带焊接顶点、索引和顶点法线的列表
    void CreateVertexIndexNormalListsFromTriangles(NativeQueue<Triangle> triangleList, ref List<Vector3> fVertexList,
        ref List<int> fIndexList, ref List<Vector3> fNormalList, ref Dictionary<Vector3, int> vertexDictionary)
    {
        while (triangleList.Count > 0)
        {
            Triangle triangle = triangleList.Dequeue();

            Vector3 edge0 = triangle.vertex1 - triangle.vertex0;
            Vector3 edge1 = triangle.vertex2 - triangle.vertex0;
            Vector3 triangleNormal = Vector3.Cross(edge1, edge0).normalized;

            if (vertexWelding)
            {
                // 顶点焊接处理
                int vertexCount = fVertexList.Count;
                List<int> triangleIndexList = new List<int>();

                // 处理三个顶点
                ProcessVertex(triangle.vertex0, triangleNormal, vertexCount, ref triangleIndexList);
                ProcessVertex(triangle.vertex1, triangleNormal, vertexCount, ref triangleIndexList);
                ProcessVertex(triangle.vertex2, triangleNormal, vertexCount, ref triangleIndexList);

                AddTriangleIndices(triangleIndexList);
            }
            else
            {
                // 非焊接顶点处理
                int offset = fIndexList.Count;
                fVertexList.Add(triangle.vertex0);
                fVertexList.Add(triangle.vertex1);
                fVertexList.Add(triangle.vertex2);

                fNormalList.Add(triangleNormal);
                fNormalList.Add(triangleNormal);
                fNormalList.Add(triangleNormal);

                AddTriangleIndices(new List<int> { offset, offset + 1, offset + 2 });
            }
        }
    }

    // 处理单个顶点的辅助方法
    private void ProcessVertex(Vector3 vertex, Vector3 normal, int vertexCount, ref List<int> triangleIndexList)
    {
        if (vertexDict.ContainsKey(vertex))
        {
            triangleIndexList.Add(vertexDict[vertex]);
            normalList[vertexDict[vertex]] += normal;
        }
        else
        {
            vertexList.Add(vertex);
            vertexDict.Add(vertex, vertexCount);
            triangleIndexList.Add(vertexCount);
            normalList.Add(normal);
            vertexCount++;
        }
    }

    // 添加三角形索引的辅助方法
    private void AddTriangleIndices(List<int> indices)
    {
        if (thresholdValue > 0f)
        {
            indexList.Add(indices[2]);
            indexList.Add(indices[1]);
            indexList.Add(indices[0]);
        }
        else
        {
            indexList.Add(indices[0]);
            indexList.Add(indices[1]);
            indexList.Add(indices[2]);
        }
    }
}

public struct Triangle
{
    public Vector3 vertex0;
    public Vector3 vertex1;
    public Vector3 vertex2;
}