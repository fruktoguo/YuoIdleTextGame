using UnityEngine;

public class StencilDebugSetup : MonoBehaviour
{
    [SerializeField] private Camera debugCamera;
    [SerializeField] private Material stencilVisualizeMaterial;
    private GameObject debugQuad;

    void OnEnable()
    {
        // 创建一个用于显示的四边形
        debugQuad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        debugQuad.name = "Stencil Debug Overlay";

        // 设置四边形位置和大小
        debugQuad.transform.parent = debugCamera.transform;
        debugQuad.transform.localPosition = new Vector3(0, 0, 0.5f);
        debugQuad.transform.localRotation = Quaternion.identity;

        // 计算合适的缩放以覆盖整个屏幕
        float camHeight = 2f * 0.5f * Mathf.Tan(debugCamera.fieldOfView * 0.5f * Mathf.Deg2Rad);
        float camWidth = camHeight * debugCamera.aspect;
        debugQuad.transform.localScale = new Vector3(camWidth, camHeight, 1);

        // 应用可视化材质
        var renderer = debugQuad.GetComponent<MeshRenderer>();
        renderer.material = stencilVisualizeMaterial;
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        renderer.receiveShadows = false;
    }

    void OnDisable()
    {
        if (debugQuad != null)
            DestroyImmediate(debugQuad);
    }
}