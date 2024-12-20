  using UnityEngine;
  using System.Collections.Generic;

  public class MetaballMeshManager : MonoBehaviour
  {
      [Header("Metaball Settings")]
      public float threshold = 1.0f;
      public float smoothing = 2.0f;
      public float radius = 1.0f;
      
      [Header("Grid Settings")]
      public int gridSize = 20;
      public float cellSize = 0.5f;
      
      [Header("Visual Settings")]
      public Material material;
      public Color color = Color.cyan;
      
      private List<Transform> metaballs = new List<Transform>();
      private MeshFilter meshFilter;
      private MeshRenderer meshRenderer;
      private Vector3[] vertices;
      private int[] triangles;
      private List<Vector3> surfacePoints = new List<Vector3>();
      private List<int> surfaceTriangles = new List<int>();
      
      void Start()
      {
          InitializeMesh();
      }
      
      void InitializeMesh()
      {
          meshFilter = gameObject.AddComponent<MeshFilter>();
          meshRenderer = gameObject.AddComponent<MeshRenderer>();
          meshRenderer.material = material;
          material.color = color;
          
          // 创建网格
          CreateGrid();
      }
      
      public void AddMetaball(Transform ball)
      {
          if (!metaballs.Contains(ball))
          {
              metaballs.Add(ball);
          }
      }
      
      public void RemoveMetaball(Transform ball)
      {
          metaballs.Remove(ball);
      }
      
      float GetMetaballValue(Vector3 point)
      {
          float sum = 0;
          foreach (var ball in metaballs)
          {
              if (ball != null)
              {
                  float distance = Vector3.Distance(point, ball.position);
                  sum += (radius * radius) / (distance * distance);
              }
          }
          return sum;
      }
      
      void CreateGrid()
      {
          float halfSize = gridSize * cellSize * 0.5f;
          vertices = new Vector3[(gridSize + 1) * (gridSize + 1) * (gridSize + 1)];
          
          int index = 0;
          for (int x = 0; x <= gridSize; x++)
          {
              for (int y = 0; y <= gridSize; y++)
              {
                  for (int z = 0; z <= gridSize; z++)
                  {
                      vertices[index++] = new Vector3(
                          x * cellSize - halfSize,
                          y * cellSize - halfSize,
                          z * cellSize - halfSize
                      );
                  }
              }
          }
          
          // 创建三角形索引
          CreateTriangles();
      }
      
      void CreateTriangles()
      {
          int numCubes = gridSize * gridSize * gridSize;
          triangles = new int[numCubes * 6 * 6]; // 每个立方体6个面，每个面2个三角形（6个顶点）
          
          int tIndex = 0;
          int vIndex = 0;
          
          for (int x = 0; x < gridSize; x++)
          {
              for (int y = 0; y < gridSize; y++)
              {
                  for (int z = 0; z < gridSize; z++)
                  {
                      // 计算立方体的8个顶点索引
                      int v0 = vIndex;
                      int v1 = v0 + 1;
                      int v2 = v0 + (gridSize + 1);
                      int v3 = v2 + 1;
                      int v4 = v0 + (gridSize + 1) * (gridSize + 1);
                      int v5 = v4 + 1;
                      int v6 = v4 + (gridSize + 1);
                      int v7 = v6 + 1;
                      
                      // 添加三角形（6个面）
                      // 前面
                      triangles[tIndex++] = v0; triangles[tIndex++] = v2; triangles[tIndex++] = v1;
                      triangles[tIndex++] = v2; triangles[tIndex++] = v3; triangles[tIndex++] = v1;
                      
                      // 后面
                      triangles[tIndex++] = v5; triangles[tIndex++] = v6; triangles[tIndex++] = v4;
                      triangles[tIndex++] = v5; triangles[tIndex++] = v7; triangles[tIndex++] = v6;
                      
                      // 顶面
                      triangles[tIndex++] = v2; triangles[tIndex++] = v6; triangles[tIndex++] = v3;
                      triangles[tIndex++] = v3; triangles[tIndex++] = v6; triangles[tIndex++] = v7;
                      
                      // 底面
                      triangles[tIndex++] = v0; triangles[tIndex++] = v1; triangles[tIndex++] = v4;
                      triangles[tIndex++] = v1; triangles[tIndex++] = v5; triangles[tIndex++] = v4;
                      
                      // 左面
                      triangles[tIndex++] = v0; triangles[tIndex++] = v4; triangles[tIndex++] = v2;
                      triangles[tIndex++] = v2; triangles[tIndex++] = v4; triangles[tIndex++] = v6;
                      
                      // 右面
                      triangles[tIndex++] = v1; triangles[tIndex++] = v3; triangles[tIndex++] = v5;
                      triangles[tIndex++] = v3; triangles[tIndex++] = v7; triangles[tIndex++] = v5;
                      
                      vIndex++;
                  }
                  vIndex++;
              }
              vIndex += gridSize + 1;
          }
      }
      
      void UpdateMesh()
      {
          Mesh mesh = new Mesh();
          surfacePoints.Clear();
          surfaceTriangles.Clear();
          
          // 计算每个顶点的metaball值
          float[] values = new float[vertices.Length];
          for (int i = 0; i < vertices.Length; i++)
          {
              values[i] = GetMetaballValue(vertices[i]);
          }
          
          // 生成表面网格
          for (int i = 0; i < triangles.Length; i += 3)
          {
              Vector3 v1 = vertices[triangles[i]];
              Vector3 v2 = vertices[triangles[i + 1]];
              Vector3 v3 = vertices[triangles[i + 2]];
              
              float val1 = values[triangles[i]];
              float val2 = values[triangles[i + 1]];
              float val3 = values[triangles[i + 2]];
              
              if (val1 > threshold || val2 > threshold || val3 > threshold)
              {
                  int index = surfacePoints.Count;
                  surfacePoints.Add(v1);
                  surfacePoints.Add(v2);
                  surfacePoints.Add(v3);
                  
                  surfaceTriangles.Add(index);
                  surfaceTriangles.Add(index + 1);
                  surfaceTriangles.Add(index + 2);
              }
          }
          
          mesh.vertices = surfacePoints.ToArray();
          mesh.triangles = surfaceTriangles.ToArray();
          mesh.RecalculateNormals();
          mesh.RecalculateBounds();
          
          meshFilter.mesh = mesh;
      }
      
      void Update()
      {
          UpdateMesh();
      }
  }