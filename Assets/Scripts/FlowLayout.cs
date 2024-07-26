using System;
using UnityEngine;
using YuoTools;

public class FlowLayout : MonoBehaviour, IGenerateCode
{
    public float padding = 10f; // 内边距
    public float spacing = 5f; // 子元素之间的间距

    private RectTransform root;

    public bool AutoRefresh = false;

    private void Update()
    {
        if (AutoRefresh)
        {
            ArrangeChildren();
        }
    }

    public void ArrangeChildren()
    {
        root = transform as RectTransform;
        float rootWidth = root.rect.width;
        float currentX = padding;
        float currentY = -padding;
        float rowHeight = 0;

        foreach (RectTransform child in root)
        {
            if (!child.gameObject.activeSelf) continue;

            float childWidth = child.rect.width;
            float childHeight = child.rect.height;

            // 如果当前行的剩余宽度不足以容纳该子元素，则换行
            if (currentX + childWidth > rootWidth - padding)
            {
                currentX = padding;
                currentY -= rowHeight + spacing;
                rowHeight = 0;
            }

            // 设置子元素的位置
            child.anchoredPosition = new Vector2(currentX, currentY);

            // 更新X坐标
            currentX += childWidth + spacing;

            // 更新行高度
            if (childHeight > rowHeight)
            {
                rowHeight = childHeight;
            }
        }
    }
}