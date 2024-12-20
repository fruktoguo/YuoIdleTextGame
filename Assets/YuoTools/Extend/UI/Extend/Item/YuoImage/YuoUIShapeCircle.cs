  using UnityEngine;
  using UnityEngine.UI;

  [System.Serializable]
  public class YuoUIShapeCircle : IYuoUIShape
  {
      [Range(3, 64)] public int SegmentCount = 32;        // 圆形段数
      [Range(0, 1.0f)] public float InnerRadius = 0f;     // 内圆半径比例
      [Range(0, 360)] public float Rotation = 0f;         // 旋转角度

      public string ShapeName => "圆形";

      private Vector2 mPivotVector;
      private Vector2 mCenter;
      private Vector4 mUv;
      private float uvScaleX;
      private float uvScaleY;

      public bool UseOriginalUV = true;

      public void Draw(VertexHelper vh, YuoImage image)
      {
          vh.Clear();
          ComputeUV(image);
          var rect = image.rectTransform.rect;

          // 使用较小的边作为圆的直径
          float radius = Mathf.Min(rect.width, rect.height) * 0.5f;
          float innerRadius = radius * InnerRadius;

          // 计算缩放比例，使圆形在矩形区域内居中
          float scaleX = rect.width / (radius * 2);
          float scaleY = rect.height / (radius * 2);

          // 如果是实心圆（没有内圆）
          if (InnerRadius <= 0)
          {
              DrawSolidCircle(vh, radius, scaleX, scaleY, image.color);
          }
          // 如果是圆环
          else
          {
              DrawRingCircle(vh, radius, innerRadius, scaleX, scaleY, image.color);
          }
      }

      private void DrawSolidCircle(VertexHelper vh, float radius, float scaleX, float scaleY, Color color)
      {
          // 添加中心点
          vh.AddVert(GetUIVertex(Vector2.zero, true, color));

          // 转换旋转角度为弧度
          float rotationRad = Rotation * Mathf.Deg2Rad;

          // 添加圆周上的点
          float anglePerSegment = (2f * Mathf.PI) / SegmentCount;
          for (int i = 0; i <= SegmentCount; i++)
          {
              float angle = anglePerSegment * i + rotationRad; // 加上旋转角度
              Vector2 point = new Vector2(
                  Mathf.Cos(angle) * radius,
                  Mathf.Sin(angle) * radius
              );
              // 应用缩放
              point.x *= scaleX;
              point.y *= scaleY;
              vh.AddVert(GetUIVertex(point, false, color));
          }

          // 构建三角形
          for (int i = 0; i < SegmentCount; i++)
          {
              vh.AddTriangle(0, i + 1, i + 2);
          }
      }

      private void DrawRingCircle(VertexHelper vh, float outerRadius, float innerRadius, float scaleX, float scaleY, Color color)
      {
          float anglePerSegment = (2f * Mathf.PI) / SegmentCount;
          float rotationRad = Rotation * Mathf.Deg2Rad; // 转换旋转角度为弧度

          // 添加外圈和内圈的顶点
          for (int i = 0; i <= SegmentCount; i++)
          {
              float angle = anglePerSegment * i + rotationRad; // 加上旋转角度
              float cos = Mathf.Cos(angle);
              float sin = Mathf.Sin(angle);

              // 外圈顶点
              Vector2 outerPoint = new Vector2(
                  cos * outerRadius * scaleX,
                  sin * outerRadius * scaleY
              );
              vh.AddVert(GetUIVertex(outerPoint, false, color));

              // 内圈顶点
              Vector2 innerPoint = new Vector2(
                  cos * innerRadius * scaleX,
                  sin * innerRadius * scaleY
              );
              vh.AddVert(GetUIVertex(innerPoint, false, color));
          }

          // 构建三角形
          for (int i = 0; i < SegmentCount; i++)
          {
              int currentOuterVertex = i * 2;
              int nextOuterVertex = (i + 1) * 2;
              int currentInnerVertex = currentOuterVertex + 1;
              int nextInnerVertex = nextOuterVertex + 1;

              // 添加两个三角形形成一个四边形
              vh.AddTriangle(currentOuterVertex, nextOuterVertex, currentInnerVertex);
              vh.AddTriangle(currentInnerVertex, nextOuterVertex, nextInnerVertex);
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

      public void SetSegmentCount(int count)
      {
          SegmentCount = Mathf.Clamp(count, 3, 64);
      }

      public void SetInnerRadius(float radius)
      {
          InnerRadius = Mathf.Clamp01(radius);
      }

      public void SetRotation(float angle)
      {
          Rotation = angle % 360f;
      }

      // 增加旋转的辅助方法
      public void Rotate(float deltaAngle)
      {
          Rotation = (Rotation + deltaAngle) % 360f;
      }
  }