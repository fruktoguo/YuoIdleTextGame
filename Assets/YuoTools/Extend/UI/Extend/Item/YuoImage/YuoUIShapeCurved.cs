using UnityEngine;
using UnityEngine.UI;

namespace YuoTools
{
    [System.Serializable]
    public class YuoUIShapeCurved : IYuoUIShape
    {
        public enum CurveDirection
        {
            Horizontal2D,     // 2D平面横向弯曲（上下弯）
            Vertical2D,       // 2D平面纵向弯曲（左右弯）
            HorizontalDepth,  // 3D空间横向弯曲（左右形成圆柱面）
            VerticalDepth     // 3D空间纵向弯曲（上下形成圆柱面）
        }

        [Range(-1f, 1f)] 
        public float CurveIntensity = 0.2f;
        [Range(2, 64)] 
        public int Segments = 10;
        public CurveDirection Direction = CurveDirection.HorizontalDepth;

        public string ShapeName => "弯曲";

        public void Draw(VertexHelper vh, YuoImage image)
        {
            vh.Clear();
          
            var rect = image.rectTransform.rect;
            var pivot = image.rectTransform.pivot;
            Vector2 pivotOffset = new Vector2(
                rect.width * (0.5f - pivot.x),
                rect.height * (0.5f - pivot.y)
            );

            Vector4 uv = image.overrideSprite != null
                ? UnityEngine.Sprites.DataUtility.GetOuterUV(image.overrideSprite)
                : new Vector4(0, 0, 1, 1);

            float halfWidth = rect.width * 0.5f;
            float halfHeight = rect.height * 0.5f;

            // 根据弯曲方向决定分段方向
            int segmentsX = Direction == CurveDirection.Vertical2D || Direction == CurveDirection.VerticalDepth ? 1 : Segments;
            int segmentsY = Direction == CurveDirection.Horizontal2D || Direction == CurveDirection.HorizontalDepth ? 1 : Segments;

            // 创建顶点网格
            for (int y = 0; y <= segmentsY; y++)
            {
                for (int x = 0; x <= segmentsX; x++)
                {
                    float xRatio = x / (float)Mathf.Max(segmentsX, 1);
                    float yRatio = y / (float)Mathf.Max(segmentsY, 1);

                    float xPos = (xRatio - 0.5f) * rect.width;
                    float yPos = (yRatio - 0.5f) * rect.height;
                    float zPos = 0;

                    // 计算弯曲
                    switch (Direction)
                    {
                        case CurveDirection.Horizontal2D:
                        {
                            float normalizedX = xPos / halfWidth;
                            float curveAmount = (1 - normalizedX * normalizedX) * CurveIntensity;
                            yPos += curveAmount * halfHeight;
                            break;
                        }
                        case CurveDirection.Vertical2D:
                        {
                            float normalizedY = yPos / halfHeight;
                            float curveAmount = (1 - normalizedY * normalizedY) * CurveIntensity;
                            xPos += curveAmount * halfWidth;
                            break;
                        }
                        case CurveDirection.HorizontalDepth:
                        {
                            float normalizedX = xPos / halfWidth;
                            float curveAmount = (1 - normalizedX * normalizedX) * -CurveIntensity;
                            zPos = -curveAmount * halfHeight;
                            // 补偿边缘拉伸
                            xPos = xPos * (1 + Mathf.Abs(curveAmount) * 0.1f);
                            break;
                        }
                        case CurveDirection.VerticalDepth:
                        {
                            float normalizedY = yPos / halfHeight;
                            float curveAmount = (1 - normalizedY * normalizedY) * -CurveIntensity;
                            zPos = -curveAmount * halfWidth;
                            // 补偿边缘拉伸
                            yPos = yPos * (1 + Mathf.Abs(curveAmount) * 0.1f);
                            break;
                        }
                    }

                    Vector3 vertPos = new Vector3(xPos, yPos, zPos) + (Vector3)pivotOffset;

                    float uvX = Mathf.Lerp(uv.x, uv.z, xRatio);
                    float uvY = Mathf.Lerp(uv.y, uv.w, yRatio);
                  
                    UIVertex vertex = new UIVertex
                    {
                        position = vertPos,
                        color = image.color,
                        uv0 = new Vector2(uvX, uvY)
                    };

                    vh.AddVert(vertex);
                }
            }

            // 创建三角形
            for (int y = 0; y < segmentsY; y++)
            {
                for (int x = 0; x < segmentsX; x++)
                {
                    int currentRow = y * (segmentsX + 1);
                    int nextRow = (y + 1) * (segmentsX + 1);

                    int currentVertex = currentRow + x;
                    int nextVertex = currentVertex + 1;
                    int nextRowVertex = nextRow + x;
                    int nextRowNextVertex = nextRow + x + 1;

                    vh.AddTriangle(currentVertex, nextVertex, nextRowVertex);
                    vh.AddTriangle(nextVertex, nextRowNextVertex, nextRowVertex);
                }
            }
        }
    }
}