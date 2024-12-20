using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasRenderer))]
[AddComponentMenu("UI/YuoImage", 12)]
public class YuoImage : Image
{
    public bool UseCustomShape;

    // 使用 SerializeReference 支持多态序列化
    [SerializeReference] public IYuoUIShape Shape;

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        if (UseCustomShape && Shape != null)
        {
            vh.Clear();
            Shape.Draw(vh, this);
        }
        else
        {
            base.OnPopulateMesh(vh);
        }
    }
}

public interface IYuoUIShape
{
    public string ShapeName { get; }
    public void Draw(VertexHelper vh, YuoImage image);
}