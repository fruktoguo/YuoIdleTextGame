using UnityEngine;
using System.Collections.Generic;

[ExecuteAlways]
[RequireComponent(typeof(MeshFilter))]
public class OptimizedWireframeRenderer : MonoBehaviour
{
    private static readonly int ZWrite = Shader.PropertyToID("_ZWrite");
    private static readonly int ZTest = Shader.PropertyToID("_ZTest");
    private static readonly int Color1 = Shader.PropertyToID("_Color");
    
    public Color wireframeColor = Color.green;
    public bool drawWireframe = true;

    private MeshFilter meshFilter;
    private Material wireframeMaterial;
    private List<Vector3> lineVertices;
    private bool isDirty = true;
    private Mesh lastMesh;

    void OnEnable()
    {
        meshFilter = GetComponent<MeshFilter>();
        CreateWireframeMaterial();
        isDirty = true;
    }

    void CreateWireframeMaterial()
    {
        if (wireframeMaterial == null)
        {
            wireframeMaterial = new Material(Shader.Find("Hidden/Internal-Colored"));
            wireframeMaterial.hideFlags = HideFlags.HideAndDontSave;
            wireframeMaterial.SetInt(ZWrite, 1);
            wireframeMaterial.SetInt(ZTest, (int)UnityEngine.Rendering.CompareFunction.Always);
        }
    }

    void GenerateWireframe()
    {
        if (meshFilter == null || meshFilter.sharedMesh == null)
            return;

        Mesh mesh = meshFilter.sharedMesh;
        if (mesh == lastMesh && !isDirty)
            return;

        lastMesh = mesh;
        int[] triangles = mesh.triangles;
        Vector3[] vertices = mesh.vertices;

        lineVertices = new List<Vector3>(triangles.Length * 2);

        for (int i = 0; i < triangles.Length; i += 3)
        {
            lineVertices.Add(vertices[triangles[i]]);
            lineVertices.Add(vertices[triangles[i + 1]]);

            lineVertices.Add(vertices[triangles[i + 1]]);
            lineVertices.Add(vertices[triangles[i + 2]]);

            lineVertices.Add(vertices[triangles[i + 2]]);
            lineVertices.Add(vertices[triangles[i]]);
        }

        isDirty = false;
    }

    void LateUpdate()
    {
        if (isDirty || meshFilter.sharedMesh != lastMesh)
        {
            GenerateWireframe();
        }
    }

    void OnRenderObject()
    {
        if (!drawWireframe || lineVertices == null || lineVertices.Count == 0)
            return;

        wireframeMaterial.SetColor(Color1, wireframeColor);
        wireframeMaterial.SetPass(0);

        GL.PushMatrix();
        GL.MultMatrix(transform.localToWorldMatrix);

        GL.Begin(GL.LINES);
        foreach (var t in lineVertices)
        {
            GL.Vertex(t);
        }
        GL.End();

        GL.PopMatrix();
    }

    void OnValidate()
    {
        CreateWireframeMaterial();
        isDirty = true;
    }

    void OnDisable()
    {
        if (wireframeMaterial != null)
        {
            DestroyImmediate(wireframeMaterial);
        }
    }

    // 公共方法，用于外部触发更新
    public void SetDirty()
    {
        isDirty = true;
    }
}
