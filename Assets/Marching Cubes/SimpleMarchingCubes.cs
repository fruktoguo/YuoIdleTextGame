using UnityEngine;
using System.Collections.Generic;
using Sirenix.OdinInspector;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public partial class SimpleMarchingCubes : MonoBehaviour
{
    [Header("Grid Settings")] 
    public Vector3Int gridSize = new Vector3Int(10, 10, 10);
    public float cellSize = 1f;

    [Header("Surface Settings")] 
    public float surfaceLevel = 0.5f;
    public Material material;

    // 内部数据
    private Dictionary<Vector3Int, float> scalarField = new Dictionary<Vector3Int, float>();
    private Mesh mesh;
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private HashSet<Vector3Int> activeCells = new HashSet<Vector3Int>();

    // 网格数据
    private List<Vector3> vertices = new List<Vector3>();
    private List<int> triangles = new List<int>();

    // 电荷点数据结构
    public class ChargePoint
    {
        public GameObject gameObject;
        public Vector3 position;
        public float strength;
        public float radius; // 影响半径

        public ChargePoint(Vector3 pos, float str, float rad = 5f)
        {
            position = pos;
            strength = str;
            radius = rad;
        }
    }

    private List<ChargePoint> chargePoints = new List<ChargePoint>();

    private void Awake()
    {
        InitializeComponents();
    }

    private void InitializeComponents()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();

        if (material != null)
            meshRenderer.material = material;
        else
            meshRenderer.material = new Material(Shader.Find("Standard"));

        mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        meshFilter.mesh = mesh;
    }

    public void AddChargePoint(Vector3 position, float strength, float radius = 5f, GameObject pointer = null)
    {
        chargePoints.Add(new ChargePoint(position, strength, radius)
        {
            gameObject = pointer
        });
        UpdateScalarField();
        GenerateMesh();
    }

    private Vector3Int WorldToGrid(Vector3 worldPos)
    {
        return new Vector3Int(
            Mathf.FloorToInt(worldPos.x / cellSize),
            Mathf.FloorToInt(worldPos.y / cellSize),
            Mathf.FloorToInt(worldPos.z / cellSize)
        );
    }

    private void UpdateScalarField()
    {
        scalarField.Clear();
        activeCells.Clear();

        // 对每个电荷点
        foreach (var charge in chargePoints)
        {
            // 计算电荷影响的网格范围
            Vector3Int minGrid = WorldToGrid(charge.position - Vector3.one * charge.radius);
            Vector3Int maxGrid = WorldToGrid(charge.position + Vector3.one * charge.radius);

            // 遍历影响范围内的所有网格点
            for (int x = minGrid.x - 1; x <= maxGrid.x + 1; x++)
            {
                for (int y = minGrid.y - 1; y <= maxGrid.y + 1; y++)
                {
                    for (int z = minGrid.z - 1; z <= maxGrid.z + 1; z++)
                    {
                        Vector3Int gridPos = new Vector3Int(x, y, z);
                        Vector3 worldPos = new Vector3(x * cellSize, y * cellSize, z * cellSize);
                        
                        // 计算该点的标量值
                        float value = CalculateScalarValue(worldPos);
                        
                        // 如果该点附近可能有等值面，则记录该单元格
                        if (!scalarField.ContainsKey(gridPos))
                        {
                            scalarField[gridPos] = value;
                        }
                        else
                        {
                            scalarField[gridPos] += value;
                        }

                        // 如果该点的值接近表面值，则标记为活动单元格
                        if (Mathf.Abs(scalarField[gridPos] - surfaceLevel) < charge.radius)
                        {
                            activeCells.Add(new Vector3Int(x-1, y-1, z-1));
                            activeCells.Add(new Vector3Int(x-1, y-1, z));
                            activeCells.Add(new Vector3Int(x-1, y, z-1));
                            activeCells.Add(new Vector3Int(x, y-1, z-1));
                            activeCells.Add(new Vector3Int(x, y, z));
                        }
                    }
                }
            }
        }
    }

    private float CalculateScalarValue(Vector3 point)
    {
        float value = 0;
        foreach (var charge in chargePoints)
        {
            float distance = Vector3.Distance(point, charge.position);
            if (distance < 0.0001f) distance = 0.0001f;
            if (distance <= charge.radius)
            {
                value += charge.strength * (1.0f - distance / charge.radius);
            }
        }
        return value;
    }

    private float GetScalarValue(Vector3Int gridPos)
    {
        if (scalarField.TryGetValue(gridPos, out float value))
        {
            return value;
        }
        return 0f;
    }

    private void GenerateMesh()
    {
        vertices.Clear();
        triangles.Clear();

        foreach (var cell in activeCells)
        {
            MarchCube(cell);
        }

        UpdateMesh();
    }

    private void UpdateMesh()
    {
        mesh.Clear();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
    }

    private void MarchCube(Vector3Int position)
    {
        float[] cubeValues = new float[8];
        Vector3[] cubeVertices = new Vector3[8];

        for (int i = 0; i < 8; i++)
        {
            Vector3Int vertexOffset = GetVertexOffset(i);
            Vector3Int pos = position + vertexOffset;
            
            cubeValues[i] = GetScalarValue(pos);
            cubeVertices[i] = new Vector3(
                (pos.x) * cellSize,
                (pos.y) * cellSize,
                (pos.z) * cellSize
            );
        }

        int cubeIndex = 0;
        for (int i = 0; i < 8; i++)
        {
            if (cubeValues[i] > surfaceLevel)
            {
                cubeIndex |= 1 << i;
            }
        }

        if (MarchingCubesTable.EdgeTable[cubeIndex] == 0) return;

        Vector3[] edgeVertices = new Vector3[12];
        for (int i = 0; i < 12; i++)
        {
            if ((MarchingCubesTable.EdgeTable[cubeIndex] & (1 << i)) != 0)
            {
                int v1 = MarchingCubesTable.EdgeConnection[i, 0];
                int v2 = MarchingCubesTable.EdgeConnection[i, 1];

                edgeVertices[i] = InterpolateVertex(
                    cubeVertices[v1], cubeVertices[v2],
                    cubeValues[v1], cubeValues[v2]
                );
            }
        }

        for (int i = 0; MarchingCubesTable.TriTableOneDim[cubeIndex * 16 + i] != -1; i += 3)
        {
            int index1 = MarchingCubesTable.TriTableOneDim[cubeIndex * 16 + i];
            int index2 = MarchingCubesTable.TriTableOneDim[cubeIndex * 16 + i + 1];
            int index3 = MarchingCubesTable.TriTableOneDim[cubeIndex * 16 + i + 2];

            AddTriangle(edgeVertices[index1], edgeVertices[index2], edgeVertices[index3]);
        }
    }

    private Vector3Int GetVertexOffset(int index)
    {
        switch (index)
        {
            case 0: return new Vector3Int(0, 0, 0);
            case 1: return new Vector3Int(1, 0, 0);
            case 2: return new Vector3Int(1, 0, 1);
            case 3: return new Vector3Int(0, 0, 1);
            case 4: return new Vector3Int(0, 1, 0);
            case 5: return new Vector3Int(1, 1, 0);
            case 6: return new Vector3Int(1, 1, 1);
            case 7: return new Vector3Int(0, 1, 1);
            default: return Vector3Int.zero;
        }
    }

    private Vector3 InterpolateVertex(Vector3 v1, Vector3 v2, float value1, float value2)
    {
        if (Mathf.Abs(surfaceLevel - value1) < 0.00001f) return v1;
        if (Mathf.Abs(surfaceLevel - value2) < 0.00001f) return v2;
        if (Mathf.Abs(value1 - value2) < 0.00001f) return v1;

        float t = (surfaceLevel - value1) / (value2 - value1);
        return v1 + t * (v2 - v1);
    }

    private void AddTriangle(Vector3 v1, Vector3 v2, Vector3 v3)
    {
        int vertCount = vertices.Count;
        vertices.Add(v1);
        vertices.Add(v2);
        vertices.Add(v3);

        triangles.Add(vertCount);
        triangles.Add(vertCount + 1);
        triangles.Add(vertCount + 2);
    }

    [Button]
    public void ClearChargePoints()
    {
        chargePoints.Clear();
        UpdateScalarField();
        GenerateMesh();
    }

    public void RemoveChargePoint(Vector3 position)
    {
        chargePoints.RemoveAll(cp => Vector3.Distance(cp.position, position) < 0.01f);
        UpdateScalarField();
        GenerateMesh();
    }

    public void UpdateChargePoint(Vector3 newPosition, float strength, GameObject pointer)
    {
        chargePoints.RemoveAll(cp => cp.gameObject == pointer);
        AddChargePoint(newPosition, strength, 5f, pointer);
    }
}
