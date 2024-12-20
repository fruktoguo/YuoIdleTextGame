using UnityEngine;

public class MeshModifierExample : MonoBehaviour
{
    private MeshModifier meshModifier;
    private MeshFilter meshFilter;

    private MeshCollider meshCollider;
    
    public float radius = 0.5f;
    public float height = 0.2f;
    public float falloff = 2f;
    public float smoothRadius = 0.5f;

    void Start()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshCollider = GetComponent<MeshCollider>();
        meshModifier = new MeshModifier(meshFilter.mesh, transform);
    }

    void Update()
    {
        // 示例：在鼠标点击位置创建凸起
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                // 创建凸起：位置、半径0.5、高度0.2、衰减系数2
                meshModifier.CreateBump(hit.point, radius, height, falloff);
                meshModifier.SmoothArea(hit.point, radius, smoothRadius);
                meshCollider.sharedMesh = meshFilter.mesh;
            }
        }

        // 示例：在鼠标右键点击位置创建凹陷
        if (Input.GetMouseButtonDown(1))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                // 创建凹陷：位置、半径0.5、深度0.2、衰减系数2
                meshModifier.CreateDent(hit.point, radius, height, falloff);
                meshModifier.SmoothArea(hit.point, radius, smoothRadius);
                meshCollider.sharedMesh = meshFilter.mesh;
            }
        }
    }
}