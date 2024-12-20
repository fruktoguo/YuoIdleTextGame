using Sirenix.OdinInspector;
using UnityEngine;

public class UILerp : MonoBehaviour
{
    [Header("Progress")] 
    [Range(0, 1)] 
    [OnValueChanged(nameof(OnLerp))]
    public float progress = 0; // 控制进度值 0-1

    [Header("Reference Transforms")]
    [Required("需要设置起始参考UI")] 
    public RectTransform startRect;
    [Required("需要设置结束参考UI")] 
    public RectTransform endRect;

    [FoldoutGroup("Lerp Settings")]
    public bool lerpPosition = true;
    [FoldoutGroup("Lerp Settings")]
    public bool lerpRotation = true;
    [FoldoutGroup("Lerp Settings")]
    public bool lerpScale = true;
    [FoldoutGroup("Lerp Settings")]
    public bool lerpSizeDelta = true;
    [FoldoutGroup("Lerp Settings")]
    public bool lerpAnchoredPosition = true;
    [FoldoutGroup("Lerp Settings")]
    public bool lerpPivot = true;

    private RectTransform _rect;
    public RectTransform Rect => _rect ??= transform as RectTransform;

    private void Start()
    {
        // 初始化时进行一次插值
        OnLerp(progress);
    }

    void OnLerp(float value)
    {
        if (startRect == null || endRect == null || Rect == null)
            return;

        if (lerpPosition)
        {
            Rect.position = Vector3.Lerp(startRect.position, endRect.position, value);
        }

        if (lerpRotation)
        {
            Rect.rotation = Quaternion.Lerp(startRect.rotation, endRect.rotation, value);
        }

        if (lerpScale)
        {
            Rect.localScale = Vector3.Lerp(startRect.localScale, endRect.localScale, value);
        }

        if (lerpSizeDelta)
        {
            Rect.sizeDelta = Vector2.Lerp(startRect.sizeDelta, endRect.sizeDelta, value);
        }

        if (lerpAnchoredPosition)
        {
            Rect.anchoredPosition = Vector2.Lerp(startRect.anchoredPosition, endRect.anchoredPosition, value);
            Rect.anchorMin = Vector2.Lerp(startRect.anchorMin, endRect.anchorMin, value);
            Rect.anchorMax = Vector2.Lerp(startRect.anchorMax, endRect.anchorMax, value);
        }

        if (lerpPivot)
        {
            Rect.pivot = Vector2.Lerp(startRect.pivot, endRect.pivot, value);
        }
    }

    [Button("设置为起始状态")]
    void SetToStart()
    {
        SetProgress(0);
    }

    [Button("设置为结束状态")]
    void SetToEnd()
    {
        SetProgress(1);
    }

    /// <summary>
    /// 设置进度值
    /// </summary>
    /// <param name="value">0-1之间的值</param>
    public void SetProgress(float value)
    {
        progress = Mathf.Clamp01(value);
        OnLerp(progress);
    }

    /// <summary>
    /// 复制当前状态到起始引用
    /// </summary>
    [Button("复制当前状态到起始引用")]
    public void CopyCurrentToStart()
    {
        if (startRect == null)
        {
            Debug.LogError("起始引用未设置");
            return;
        }
        CopyRectTransform(Rect, startRect);
    }

    /// <summary>
    /// 复制当前状态到结束引用
    /// </summary>
    [Button("复制当前状态到结束引用")]
    public void CopyCurrentToEnd()
    {
        if (endRect == null)
        {
            Debug.LogError("结束引用未设置");
            return;
        }
        CopyRectTransform(Rect, endRect);
    }

    private void CopyRectTransform(RectTransform source, RectTransform target)
    {
        target.position = source.position;
        target.rotation = source.rotation;
        target.localScale = source.localScale;
        target.sizeDelta = source.sizeDelta;
        target.anchoredPosition = source.anchoredPosition;
        target.anchorMin = source.anchorMin;
        target.anchorMax = source.anchorMax;
        target.pivot = source.pivot;
    }
}
