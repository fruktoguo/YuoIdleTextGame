using UnityEngine;
using UnityEngine.UI;

namespace YuoTools
{
    [System.Serializable]
    public class YuoUIShapeRoundedRect : IYuoUIShape
    {
        [Range(0, 1.0f)] public float Radius = 1f;
        [Range(1, 64)] public int TriangleCount = 24;

        public string ShapeName => "圆角矩形";

        private Vector2 mPivotVector;
        private Vector2 mCenter;
        private Vector4 mUv;
        private float uvScaleX;
        private float uvScaleY;

        public bool UseOriginalUV = true; // 控制是否应用UV映射

        public void Draw(VertexHelper vh, YuoImage image)
        {
            vh.Clear();
            ComputeUV(image);
            var rect = image.rectTransform.rect;

            // 计算真实半径范围
            float realRadius;
            if (rect.height < rect.width)
                realRadius = 0.5f * rect.height * Radius;
            else
                realRadius = 0.5f * rect.width * Radius;

            // 四个圆角的圆心坐标
            Vector2 leftBottomCenter = new Vector2(-0.5f * rect.width + realRadius, -0.5f * rect.height + realRadius);
            Vector2 leftTopCenter = new Vector2(-0.5f * rect.width + realRadius, 0.5f * rect.height - realRadius);
            Vector2 rightTopCenter = new Vector2(0.5f * rect.width - realRadius, 0.5f * rect.height - realRadius);
            Vector2 rightBottomCenter = new Vector2(0.5f * rect.width - realRadius, -0.5f * rect.height + realRadius);

            // 添加中心点
            vh.AddVert(GetUIVertex(Vector2.zero, true, image.color));

            // 绘制四个圆角
            float angle = 90f / TriangleCount * Mathf.Deg2Rad;
            var centers = new[] { leftBottomCenter, leftTopCenter, rightTopCenter, rightBottomCenter };

            for (int i = 0; i < centers.Length; i++)
            {
                DrawCircle(vh, centers[i], realRadius, TriangleCount, angle, image.color, i * 90 + 180);
            }

            // 连接三角形
            var count = vh.currentVertCount;
            for (int i = 1; i < count - 1; i++)
            {
                vh.AddTriangle(0, i, i + 1);
            }

            vh.AddTriangle(0, count - 1, 1);
        }

        private void DrawCircle(VertexHelper vh, Vector2 center, float r, int triangle, float angle, Color c,
            float offsetAngle)
        {
            offsetAngle = offsetAngle * Mathf.Deg2Rad;
            for (int i = 0; i < triangle + 1; i++)
            {
                float newAngle = angle * i + offsetAngle;
                Vector2 borderXY = new Vector2(r * Mathf.Sin(newAngle), r * Mathf.Cos(newAngle));
                Vector2 borderPos = center + borderXY;
                vh.AddVert(GetUIVertex(borderPos, false, c));
            }
        }

        private void ComputeUV(YuoImage image)
        {
            var rect = image.rectTransform.rect;
            var pivot = image.rectTransform.pivot;
            float tw = rect.width;
            float th = rect.height;
            mPivotVector = new Vector2(tw * (0.5f - pivot.x), th * (0.5f - pivot.y));
            mUv = image.overrideSprite != null
                ? UnityEngine.Sprites.DataUtility.GetOuterUV(image.overrideSprite)
                : Vector4.zero;
            mCenter = new Vector2(mUv.x + mUv.z, mUv.y + mUv.w) * 0.5f;
            uvScaleX = (mUv.z - mUv.x) / tw;
            uvScaleY = (mUv.w - mUv.y) / th;
        }

        private UIVertex GetUIVertex(Vector2 point, bool isCenter, Color color)
        {
            return new UIVertex
            {
                color = color,
                position = point + mPivotVector,
                uv0 = UseOriginalUV || isCenter
                    ? new Vector2(point.x * uvScaleX, point.y * uvScaleY) + mCenter
                    : Vector2.zero
            };
        }

        public void SetRadius(float radius)
        {
            Radius = Mathf.Clamp01(radius);
        }

        public void SetTriangleCount(int count)
        {
            TriangleCount = Mathf.Clamp(count, 1, 64);
        }
    }
}