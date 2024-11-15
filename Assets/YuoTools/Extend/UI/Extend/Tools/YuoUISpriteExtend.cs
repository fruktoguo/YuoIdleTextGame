using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using YuoTools;
using Sirenix.OdinInspector;

[RequireComponent(typeof(Image))]
public class YuoUISpriteExtend : SerializedMonoBehaviour, IGenerateCode
{
    public List<Sprite> sprites = new List<Sprite>();

    Image image;
    Image Graphic => image ??= GetComponent<Image>();

    public void SetSprite(int index)
    {
        if (index >= 0 && index < sprites.Count)
        {
            Graphic.sprite = sprites[index];
        }
    }

    private void SetImageSprite(Sprite sprite)
    {
        Graphic.sprite = sprite;
    }
#if UNITY_EDITOR
    [OnInspectorGUI]
    private void CustomInspector()
    {
        GUILayout.Space(10);
        GUILayout.Label("点击预览图设置精灵", UnityEditor.EditorStyles.boldLabel);
        GUILayout.BeginHorizontal();
        for (int i = 0; i < sprites.Count; i++)
        {
            if (sprites[i] != null)
            {
                if (GUILayout.Button(new GUIContent(sprites[i].texture), GUILayout.Width(50), GUILayout.Height(50)))
                {
                    SetSprite(i);
                }
            }
        }

        GUILayout.EndHorizontal();
    }
#endif
}