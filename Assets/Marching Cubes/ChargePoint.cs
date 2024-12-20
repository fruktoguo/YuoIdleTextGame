using UnityEngine;

public class ChargePoint : MonoBehaviour
{
    [Header("Charge Settings")] [Tooltip("电荷强度，正值为正电荷，负值为负电荷")]
    public float strength = 1.0f;

    [Tooltip("电荷影响范围")] public float radius = 2.0f;

    [Header("Visual Settings")] [Tooltip("是否显示影响范围")]
    public bool showRadius = true;

    [Tooltip("正电荷颜色")] public Color positiveColor = Color.red;

    [Tooltip("负电荷颜色")] public Color negativeColor = Color.blue;

    private SimpleMarchingCubes marchingCubes;
    private MeshRenderer meshRenderer;

    void Start()
    {
        // 查找场景中的 MarchingCubes
        marchingCubes = FindObjectOfType<SimpleMarchingCubes>();
        if (marchingCubes == null)
        {
            Debug.LogWarning("Scene is missing SimpleMarchingCubes!");
            return;
        }

        // 获取或添加渲染器组件
        meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer == null)
        {
            meshRenderer = gameObject.AddComponent<MeshRenderer>();
        }

        // 更新视觉效果
        UpdateVisuals();

        // 添加到 MarchingCubes 系统
        AddToMarchingCubes();
    }

    void OnValidate()
    {
        UpdateVisuals();
    }

    void UpdateVisuals()
    {
        if (meshRenderer == null) return;

        // 更新颜色
        Color chargeColor = strength > 0 ? positiveColor : negativeColor;

        // 如果物体使用标准材质
        if (meshRenderer.material.shader.name == "Standard")
        {
            meshRenderer.material.SetColor("_Color", chargeColor);
            meshRenderer.material.SetColor("_EmissionColor", chargeColor * 0.5f);
            meshRenderer.material.EnableKeyword("_EMISSION");
        }

        // 更新物体大小以反映强度
        float scale = Mathf.Abs(strength) * 0.3f + 0.2f;
        transform.localScale = new Vector3(scale, scale, scale);
    }

    void OnDrawGizmos()
    {
        if (!showRadius) return;

        // 绘制影响范围
        Gizmos.color = new Color(
            strength > 0 ? positiveColor.r : negativeColor.r,
            strength > 0 ? positiveColor.g : negativeColor.g,
            strength > 0 ? positiveColor.b : negativeColor.b,
            0.2f
        );
        Gizmos.DrawWireSphere(transform.position, radius);
    }

    void OnDestroy()
    {
        // 当物体被销毁时，更新 MarchingCubes
        if (marchingCubes != null)
        {
            marchingCubes.RemoveChargePoint(transform.position);
        }
    }

    public void AddToMarchingCubes()
    {
        if (marchingCubes != null)
        {
            marchingCubes.AddChargePoint(transform.position, strength);
        }
    }

    void Update()
    {
        // 如果位置发生变化，更新 MarchingCubes
        if (transform.hasChanged)
        {
            if (marchingCubes != null)
            {
                marchingCubes.UpdateChargePoint(transform.position, strength, gameObject);
            }

            transform.hasChanged = false;
        }
    }

    // 提供修改强度的方法
    public void SetStrength(float newStrength)
    {
        strength = newStrength;
        UpdateVisuals();
        if (marchingCubes != null)
        {
            marchingCubes.UpdateChargePoint(transform.position, strength, gameObject);
        }
    }
}