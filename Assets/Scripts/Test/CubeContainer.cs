using UnityEngine;
using System.Collections.Generic;

public class CubeContainer : MonoBehaviour
{
    [System.Serializable]
    public class FaceSettings
    {
        public Color color = Color.white;
        public int stencilID = 1;
    }

    public float size = 5f; // 容器的大小
    public Material faceMaterialTemplate; // 面的材质模板
    public Material objectMaterialTemplate; // 内部物体的材质模板

    [Header("Face Settings")] public FaceSettings frontFace = new FaceSettings { stencilID = 1 };
    public FaceSettings backFace = new FaceSettings { stencilID = 2 };
    public FaceSettings leftFace = new FaceSettings { stencilID = 3 };
    public FaceSettings rightFace = new FaceSettings { stencilID = 4 };
    public FaceSettings topFace = new FaceSettings { stencilID = 5 };
    public FaceSettings bottomFace = new FaceSettings { stencilID = 6 };

    private Dictionary<string, GameObject> faces = new Dictionary<string, GameObject>();

    void Start()
    {
        CreateContainer();
    }

    void CreateContainer()
    {
        // 创建父物体来组织所有的面
        GameObject facesParent = new GameObject("Faces");
        facesParent.transform.SetParent(transform);
        facesParent.transform.localPosition = Vector3.zero;

        // 创建六个面
        float halfSize = size * 0.5f;

        // Front face
        faces["Front"] = CreateFace("Front", new Vector3(0, 0, halfSize), Quaternion.identity,
            frontFace.stencilID, frontFace.color);

        // Back face
        faces["Back"] = CreateFace("Back", new Vector3(0, 0, -halfSize), Quaternion.Euler(0, 180, 0),
            backFace.stencilID, backFace.color);

        // Left face
        faces["Left"] = CreateFace("Left", new Vector3(-halfSize, 0, 0), Quaternion.Euler(0, -90, 0),
            leftFace.stencilID, leftFace.color);

        // Right face
        faces["Right"] = CreateFace("Right", new Vector3(halfSize, 0, 0), Quaternion.Euler(0, 90, 0),
            rightFace.stencilID, rightFace.color);

        // Top face
        faces["Top"] = CreateFace("Top", new Vector3(0, halfSize, 0), Quaternion.Euler(90, 0, 0),
            topFace.stencilID, topFace.color);

        // Bottom face
        faces["Bottom"] = CreateFace("Bottom", new Vector3(0, -halfSize, 0), Quaternion.Euler(-90, 0, 0),
            bottomFace.stencilID, bottomFace.color);

        // 设置所有面的父物体
        foreach (var face in faces.Values)
        {
            face.transform.SetParent(facesParent.transform);
        }
    }

    GameObject CreateFace(string name, Vector3 position, Quaternion rotation, int stencilID, Color color)
    {
        GameObject face = new GameObject(name);
        face.transform.localPosition = position;
        face.transform.localRotation = rotation;

        // 添加MeshFilter组件
        MeshFilter meshFilter = face.AddComponent<MeshFilter>();
        meshFilter.mesh = CreateQuadMesh();

        // 添加MeshRenderer组件
        MeshRenderer meshRenderer = face.AddComponent<MeshRenderer>();

        // 创建材质实例
        Material faceMaterial = new Material(faceMaterialTemplate);
        faceMaterial.SetInt("_StencilID", stencilID);
        faceMaterial.SetColor("_Color", color);
        meshRenderer.material = faceMaterial;

        return face;
    }

    Mesh CreateQuadMesh()
    {
        Mesh mesh = new Mesh();

        // 顶点
        Vector3[] vertices = new Vector3[4]
        {
            new Vector3(-size / 2, -size / 2, 0),
            new Vector3(size / 2, -size / 2, 0),
            new Vector3(-size / 2, size / 2, 0),
            new Vector3(size / 2, size / 2, 0)
        };

        // UV坐标
        Vector2[] uv = new Vector2[4]
        {
            new Vector2(0, 0),
            new Vector2(1, 0),
            new Vector2(0, 1),
            new Vector2(1, 1)
        };

        // 三角形
        int[] triangles = new int[6]
        {
            0, 2, 1,
            2, 3, 1
        };

        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        return mesh;
    }

    // 获取面的引用
    public GameObject GetFace(string faceName)
    {
        if (faces.ContainsKey(faceName))
            return faces[faceName];
        return null;
    }

    // 设置物体可见面
    public void SetObjectVisibility(GameObject obj, string faceName)
    {
        if (!faces.ContainsKey(faceName))
            return;

        MeshRenderer renderer = obj.GetComponent<MeshRenderer>();
        if (renderer == null)
            return;

        // 获取对应面的StencilID
        int stencilID = 0;
        switch (faceName)
        {
            case "Front": stencilID = frontFace.stencilID; break;
            case "Back": stencilID = backFace.stencilID; break;
            case "Left": stencilID = leftFace.stencilID; break;
            case "Right": stencilID = rightFace.stencilID; break;
            case "Top": stencilID = topFace.stencilID; break;
            case "Bottom": stencilID = bottomFace.stencilID; break;
        }

        // 创建新材质并设置StencilID
        Material objMaterial = new Material(objectMaterialTemplate);
        objMaterial.SetInt("_StencilID", stencilID);
        renderer.material = objMaterial;
    }
}