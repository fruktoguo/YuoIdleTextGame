using UnityEngine;
using UnityEngine.UI;

namespace YuoTools
{
    /// <summary>
    /// UI顶点排布会影响渲染顺序,暂时没法用
    /// </summary>
    [System.Serializable]
    public class YuoUIShapeThick : IYuoUIShape
    {
        public float Thickness = 10f;
        [Range(0.1f, 1f)] public float SideColorScale = 0.8f;

        public string ShapeName => "厚度(弃用)";

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

            Color mainColor = image.color;
            Color sideColor = new Color(
                mainColor.r * SideColorScale,
                mainColor.g * SideColorScale,
                mainColor.b * SideColorScale,
                mainColor.a
            );

            // 绘制顺序：后面 -> 侧面 -> 前面
            CreateBackFace(vh, rect, pivotOffset, uv, -Thickness, mainColor);
            CreateSides(vh, rect, pivotOffset, uv, sideColor, Thickness);
            CreateFrontFace(vh, rect, pivotOffset, uv, 0, mainColor);
        }

        private void CreateFrontFace(VertexHelper vh, Rect rect, Vector2 pivotOffset, Vector4 uv, float zOffset,
            Color color)
        {
            int startIndex = vh.currentVertCount;

            // 顶点位置（左下 -> 右下 -> 右上 -> 左上）
            Vector3[] corners = new Vector3[4]
            {
                new Vector3(-rect.width * 0.5f, -rect.height * 0.5f, zOffset) + (Vector3)pivotOffset, // 左下
                new Vector3(rect.width * 0.5f, -rect.height * 0.5f, zOffset) + (Vector3)pivotOffset, // 右下
                new Vector3(rect.width * 0.5f, rect.height * 0.5f, zOffset) + (Vector3)pivotOffset, // 右上
                new Vector3(-rect.width * 0.5f, rect.height * 0.5f, zOffset) + (Vector3)pivotOffset // 左上
            };

            Vector2[] uvs = new Vector2[4]
            {
                new Vector2(uv.x, uv.y), // 左下
                new Vector2(uv.z, uv.y), // 右下
                new Vector2(uv.z, uv.w), // 右上
                new Vector2(uv.x, uv.w) // 左上
            };

            // 添加顶点
            for (int i = 0; i < 4; i++)
            {
                UIVertex vertex = UIVertex.simpleVert;
                vertex.position = corners[i];
                vertex.color = color;
                vertex.uv0 = uvs[i];
                vh.AddVert(vertex);
            }

            // 前面：逆时针顺序
            vh.AddTriangle(startIndex + 0, startIndex + 2, startIndex + 1);
            vh.AddTriangle(startIndex + 0, startIndex + 3, startIndex + 2);
        }

        private void CreateBackFace(VertexHelper vh, Rect rect, Vector2 pivotOffset, Vector4 uv, float zOffset, Color color)
        {
            int startIndex = vh.currentVertCount;

            Vector3[] corners = new Vector3[4]
            {
                new Vector3(-rect.width * 0.5f, -rect.height * 0.5f, zOffset) + (Vector3)pivotOffset,
                new Vector3(rect.width * 0.5f, -rect.height * 0.5f, zOffset) + (Vector3)pivotOffset,
                new Vector3(rect.width * 0.5f, rect.height * 0.5f, zOffset) + (Vector3)pivotOffset,
                new Vector3(-rect.width * 0.5f, rect.height * 0.5f, zOffset) + (Vector3)pivotOffset
            };

            Vector2[] uvs = new Vector2[4]
            {
                new Vector2(uv.x, uv.y),
                new Vector2(uv.z, uv.y),
                new Vector2(uv.z, uv.w),
                new Vector2(uv.x, uv.w)
            };

            for (int i = 0; i < 4; i++)
            {
                UIVertex vertex = UIVertex.simpleVert;
                vertex.position = corners[i];
                vertex.color = color;
                vertex.uv0 = uvs[i];
                vh.AddVert(vertex);
            }

            // 背面：顺时针顺序
            vh.AddTriangle(startIndex + 0, startIndex + 1, startIndex + 2);
            vh.AddTriangle(startIndex + 0, startIndex + 2, startIndex + 3);
        }

        private void CreateSides(VertexHelper vh, Rect rect, Vector2 pivotOffset, Vector4 uv, Color sideColor,
            float thickness)
        {
            Vector3[] vertices = new Vector3[8];

            // 前面4个顶点
            vertices[0] = new Vector3(-rect.width * 0.5f, -rect.height * 0.5f, 0) + (Vector3)pivotOffset;
            vertices[1] = new Vector3(rect.width * 0.5f, -rect.height * 0.5f, 0) + (Vector3)pivotOffset;
            vertices[2] = new Vector3(rect.width * 0.5f, rect.height * 0.5f, 0) + (Vector3)pivotOffset;
            vertices[3] = new Vector3(-rect.width * 0.5f, rect.height * 0.5f, 0) + (Vector3)pivotOffset;

            // 后面4个顶点
            for (int i = 0; i < 4; i++)
            {
                vertices[i + 4] = vertices[i] + new Vector3(0, 0, -thickness);
            }

            // 定义侧面
            var sides = new[]
            {
                // 底边
                new
                {
                    Indices = new[] { 0, 1, 5, 4 },
                    UVs = new[]
                    {
                        new Vector2(uv.x, uv.y),
                        new Vector2(uv.z, uv.y),
                        new Vector2(uv.z, uv.y),
                        new Vector2(uv.x, uv.y)
                    }
                },
                // 右边
                new
                {
                    Indices = new[] { 1, 2, 6, 5 },
                    UVs = new[]
                    {
                        new Vector2(uv.z, uv.y),
                        new Vector2(uv.z, uv.w),
                        new Vector2(uv.z, uv.w),
                        new Vector2(uv.z, uv.y)
                    }
                },
                // 顶边
                new
                {
                    Indices = new[] { 2, 3, 7, 6 },
                    UVs = new[]
                    {
                        new Vector2(uv.z, uv.w),
                        new Vector2(uv.x, uv.w),
                        new Vector2(uv.x, uv.w),
                        new Vector2(uv.z, uv.w)
                    }
                },
                // 左边
                new
                {
                    Indices = new[] { 3, 0, 4, 7 },
                    UVs = new[]
                    {
                        new Vector2(uv.x, uv.w),
                        new Vector2(uv.x, uv.y),
                        new Vector2(uv.x, uv.y),
                        new Vector2(uv.x, uv.w)
                    }
                }
            };

            foreach (var side in sides)
            {
                int startIndex = vh.currentVertCount;

                for (int i = 0; i < 4; i++)
                {
                    UIVertex vertex = UIVertex.simpleVert;
                    vertex.position = vertices[side.Indices[i]];
                    vertex.color = sideColor;
                    vertex.uv0 = side.UVs[i];
                    vh.AddVert(vertex);
                }

                // 侧面三角形顺序
                vh.AddTriangle(startIndex + 0, startIndex + 2, startIndex + 1);
                vh.AddTriangle(startIndex + 0, startIndex + 3, startIndex + 2);
            }
        }
    }
}