  using UnityEngine;
  using UnityEngine.UI;

  namespace YuoTools
  {
      [System.Serializable]
      public class YuoUIShapeCircle : IYuoUIShape
      {
          [Range(3, 64)] public int SegmentCount = 32;        // 圆形段数
          [Range(0, 1.0f)] public float InnerRadius = 0f;     // 内圆半径比例
          [Range(0, 360)] public float Rotation = 0f;         // 旋转角度
          public bool UseOriginalUV = true;                   // 是否使用原始UV

          public string ShapeName => "圆形";

          private Vector2 mPivotVector;
          private Vector2 mCenter;
          private Vector4 mUv;
          private float uvScaleX;
          private float uvScaleY;

          private void ComputeUV(YuoImage image)
          {
              var rect = image.rectTransform.rect;
              var pivot = image.rectTransform.pivot;
              mCenter = new Vector2(rect.width * 0.5f, rect.height * 0.5f);
              mPivotVector = new Vector2(rect.width * pivot.x, rect.height * pivot.y);

              if (image.overrideSprite != null)
              {
                  mUv = UnityEngine.Sprites.DataUtility.GetOuterUV(image.overrideSprite);
                  var inner = UnityEngine.Sprites.DataUtility.GetInnerUV(image.overrideSprite);
                  uvScaleX = (inner.z - inner.x) / rect.width;
                  uvScaleY = (inner.w - inner.y) / rect.height;
              }
              else
              {
                  mUv = new Vector4(0, 0, 1, 1);
                  uvScaleX = 1f / rect.width;
                  uvScaleY = 1f / rect.height;
              }
          }

          public void Draw(VertexHelper vh, YuoImage image)
          {
              vh.Clear();
              ComputeUV(image);
              var rect = image.rectTransform.rect;

              // 使用较小的边作为圆的直径
              float radius = Mathf.Min(rect.width, rect.height) * 0.5f;
              float innerRadius = radius * InnerRadius;

              if (InnerRadius <= 0)
              {
                  DrawSolidCircle(vh, radius, image.color, rect);
              }
              else
              {
                  DrawRingCircle(vh, radius, innerRadius, image.color, rect);
              }
          }

          private Vector2 GetUV(Vector2 position, Rect rect)
          {
              if (UseOriginalUV)
              {
                  // 考虑pivot偏移的UV计算
                  float u = (position.x + mPivotVector.x) * uvScaleX;
                  float v = (position.y + mPivotVector.y) * uvScaleY;
                  return new Vector2(
                      Mathf.Lerp(mUv.x, mUv.z, u),
                      Mathf.Lerp(mUv.y, mUv.w, v)
                  );
              }
              else
              {
                  // 将位置从圆形映射回方形，考虑pivot
                  float x = (position.x + mPivotVector.x - mCenter.x) / mCenter.x;
                  float y = (position.y + mPivotVector.y - mCenter.y) / mCenter.y;
                  float magnitude = Mathf.Sqrt(x * x + y * y);

                  if (magnitude > 0)
                  {
                      // 保持方形边界的映射
                      float scale = Mathf.Max(Mathf.Abs(x), Mathf.Abs(y));
                      x = (x / scale * 0.5f + 0.5f);
                      y = (y / scale * 0.5f + 0.5f);
                  }
                  else
                  {
                      x = y = 0.5f;
                  }

                  return new Vector2(
                      Mathf.Lerp(mUv.x, mUv.z, x),
                      Mathf.Lerp(mUv.y, mUv.w, y)
                  );
              }
          }

          private void DrawSolidCircle(VertexHelper vh, float radius, Color color, Rect rect)
          {
              // 添加中心点
              vh.AddVert(GetUIVertex(Vector2.zero, color));

              float rotationRad = Rotation * Mathf.Deg2Rad;
              float anglePerSegment = (2f * Mathf.PI) / SegmentCount;

              for (int i = 0; i <= SegmentCount; i++)
              {
                  float angle = anglePerSegment * i + rotationRad;
                  Vector2 point = new Vector2(
                      Mathf.Cos(angle) * radius,
                      Mathf.Sin(angle) * radius
                  );
                  vh.AddVert(GetUIVertex(point, color));
              }

              for (int i = 0; i < SegmentCount; i++)
              {
                  vh.AddTriangle(0, i + 1, i + 2);
              }
          }

          private void DrawRingCircle(VertexHelper vh, float outerRadius, float innerRadius, Color color, Rect rect)
          {
              float rotationRad = Rotation * Mathf.Deg2Rad;
              float anglePerSegment = (2f * Mathf.PI) / SegmentCount;

              for (int i = 0; i <= SegmentCount; i++)
              {
                  float angle = anglePerSegment * i + rotationRad;
                  float cos = Mathf.Cos(angle);
                  float sin = Mathf.Sin(angle);

                  // 外圈顶点
                  Vector2 outerPoint = new Vector2(cos * outerRadius, sin * outerRadius);
                  vh.AddVert(GetUIVertex(outerPoint, color));

                  // 内圈顶点
                  Vector2 innerPoint = new Vector2(cos * innerRadius, sin * innerRadius);
                  vh.AddVert(GetUIVertex(innerPoint, color));
              }

              for (int i = 0; i < SegmentCount; i++)
              {
                  int currentOuterVertex = i * 2;
                  int nextOuterVertex = (i + 1) * 2;
                  int currentInnerVertex = currentOuterVertex + 1;
                  int nextInnerVertex = nextOuterVertex + 1;

                  vh.AddTriangle(currentOuterVertex, nextOuterVertex, currentInnerVertex);
                  vh.AddTriangle(currentInnerVertex, nextOuterVertex, nextInnerVertex);
              }
          }

          private UIVertex GetUIVertex(Vector2 point, Color color)
          {
              var uv = GetUV(point, new Rect(-mCenter.x, -mCenter.y, mCenter.x * 2, mCenter.y * 2));
              return new UIVertex
              {
                  position = (Vector3)point,
                  color = color,
                  uv0 = uv
              };
          }
      }
  }