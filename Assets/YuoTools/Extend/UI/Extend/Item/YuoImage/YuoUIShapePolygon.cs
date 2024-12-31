using UnityEngine;
using UnityEngine.UI;

namespace YuoTools
{
    [System.Serializable]
    public class YuoUIShapePolygon : IYuoUIShape
    {
        public PolygonCollider2D SourceCollider; // 源碰撞器
        [Range(0, 360)] public float Rotation; // 旋转角度
        [Range(0.01f, 10f)] public float Scale = 1f; // 缩放比例
        public bool PreserveAspect = true; // 是否保持长宽比

        public string ShapeName => "多边形(碰撞器)";

        private Vector2 mPivotVector;
        private Vector2 mCenter;
        private Vector4 mUv;
        private float uvScaleX;
        private float uvScaleY;

        public bool UseOriginalUV = true;

        public void Draw(VertexHelper vh, YuoImage image)
        {
            SourceCollider ??= image.gameObject.GetComponent<PolygonCollider2D>();
            if (SourceCollider == null)
            {
                Debug.LogWarning("YuoUIShapePolygon: No PolygonCollider2D assigned!");
                return;
            }

            vh.Clear();
            ComputeUV(image);
            var rect = image.rectTransform.rect;

            // 获取碰撞器的点
            var points = SourceCollider.points;
            if (points.Length < 3)
            {
                Debug.LogWarning("YuoUIShapePolygon: Not enough points in collider!");
                return;
            }

            // 计算多边形的边界
            Vector2 min = points[0];
            Vector2 max = points[0];
            foreach (var point in points)
            {
                min = Vector2.Min(min, point);
                max = Vector2.Max(max, point);
            }

            Vector2 size = max - min;
            Vector2 center = (max + min) * 0.5f;

            // 添加中心顶点
            vh.AddVert(GetUIVertex(Vector2.zero, true, image.color));

            // 计算缩放比例
            float scaleX = rect.width / size.x;
            float scaleY = rect.height / size.y;

            // 如果需要保持长宽比
            if (PreserveAspect)
            {
                float minScale = Mathf.Min(scaleX, scaleY);
                scaleX = scaleY = minScale;
            }

            // 应用自定义缩放
            scaleX *= Scale;
            scaleY *= Scale;

            // 旋转角度转弧度
            float rotationRad = Rotation * Mathf.Deg2Rad;
            float cos = Mathf.Cos(rotationRad);
            float sin = Mathf.Sin(rotationRad);

            // 添加多边形的顶点
            for (int i = 0; i < points.Length; i++)
            {
                // 将点相对于中心点偏移
                Vector2 point = points[i] - center;

                // 应用缩放
                point.x *= scaleX;
                point.y *= scaleY;

                // 应用旋转
                Vector2 rotatedPoint = new Vector2(
                    point.x * cos - point.y * sin,
                    point.x * sin + point.y * cos
                );

                vh.AddVert(GetUIVertex(rotatedPoint, false, image.color));
            }

            // 构建三角形
            for (int i = 0; i < points.Length; i++)
            {
                vh.AddTriangle(0, i + 1, ((i + 1) % points.Length) + 1);
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

        public void SetRotation(float angle)
        {
            Rotation = angle % 360f;
        }

        public void SetScale(float scale)
        {
            Scale = Mathf.Clamp(scale, 0.01f, 10f);
        }

        public void SetCollider(PolygonCollider2D collider)
        {
            SourceCollider = collider;
        }

        public void Rotate(float deltaAngle)
        {
            Rotation = (Rotation + deltaAngle) % 360f;
        }

        public void SetPreserveAspect(bool preserve)
        {
            PreserveAspect = preserve;
        }
    }
}