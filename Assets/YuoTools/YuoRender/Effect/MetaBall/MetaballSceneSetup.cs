using UnityEngine;

public class MetaballSceneSetup : MonoBehaviour
{
    public int metaballCount = 3;
    public float moveRadius = 2f;
    public Color metaballColor = Color.cyan;
      
    void Start()
    {
        SetupScene();
    }
      
    void SetupScene()
    {
        // 创建管理器
        GameObject manager = new GameObject("MetaballManager");
        MetaballMeshManager metaballManager = manager.AddComponent<MetaballMeshManager>();
          
        // 设置材质
        Material material = new Material(Shader.Find("Standard"));
        material.SetFloat("_Glossiness", 0.8f);
        material.SetFloat("_Metallic", 0.5f);
        material.color = metaballColor;
        metaballManager.material = material;
          
        // 创建Metaballs
        for (int i = 0; i < metaballCount; i++)
        {
            GameObject ball = new GameObject($"Metaball_{i}");
            var movement = ball.AddComponent<MetaballMovement>();
            movement.moveRadius = moveRadius;
            movement.moveSpeed = 1f + i * 0.2f;
            metaballManager.AddMetaball(ball.transform);
        }
          
        // 设置相机
        if (Camera.main == null)
        {
            GameObject cam = new GameObject("Main Camera");
            cam.tag = "MainCamera";
            Camera camera = cam.AddComponent<Camera>();
            camera.transform.position = new Vector3(0, 0, -10);
            camera.transform.LookAt(Vector3.zero);
        }
          
        // 添加光源
        if (FindObjectOfType<Light>() == null)
        {
            GameObject light = new GameObject("Directional Light");
            Light directionalLight = light.AddComponent<Light>();
            directionalLight.type = LightType.Directional;
            light.transform.rotation = Quaternion.Euler(50, -30, 0);
        }
    }
}