using System.Diagnostics;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

/// <summary>
/// 凸包生成工具类
/// </summary>
public static class ConvexHullHelper
{
    /// <summary>
    /// 表示未分配的点
    /// </summary>
    const int Unassigned = -2;

    /// <summary>
    /// 表示在凸包内部的点
    /// </summary>
    const int Inside = -1;

    /// <summary>
    /// 浮点数比较精度
    /// </summary>
    const float Epsilon = 0.0001f;

    /// <summary>
    /// 表示凸包的一个面
    /// </summary>
    struct Face
    {
        /// <summary>
        /// 面的三个顶点索引
        /// </summary>
        public int Vertex0;

        public int Vertex1;
        public int Vertex2;

        /// <summary>
        /// 与三条边相对的面的索引
        /// </summary>
        public int Opposite0;

        public int Opposite1;
        public int Opposite2;

        /// <summary>
        /// 面的法线
        /// </summary>
        public Vector3 normal;

        /// <summary>
        /// 构造函数
        /// </summary>
        public Face(int v0, int v1, int v2, int o0, int o1, int o2, Vector3 normal)
        {
            Vertex0 = v0;
            Vertex1 = v1;
            Vertex2 = v2;
            Opposite0 = o0;
            Opposite1 = o1;
            Opposite2 = o2;
            this.normal = normal;
        }

        /// <summary>
        /// 判断两个面是否相等
        /// </summary>
        public bool Equals(Face other)
        {
            return (this.Vertex0 == other.Vertex0)
                   && (this.Vertex1 == other.Vertex1)
                   && (this.Vertex2 == other.Vertex2)
                   && (this.Opposite0 == other.Opposite0)
                   && (this.Opposite1 == other.Opposite1)
                   && (this.Opposite2 == other.Opposite2)
                   && (this.normal == other.normal);
        }
    }

    /// <summary>
    /// 点和面的关系结构
    /// </summary>
    struct PointFace
    {
        /// <summary>
        /// 点的索引
        /// </summary>
        public int point;

        /// <summary>
        /// 面的索引
        /// </summary>
        public int face;

        /// <summary>
        /// 距离
        /// </summary>
        public float distance;

        public PointFace(int p, int f, float d)
        {
            point = p;
            face = f;
            distance = d;
        }
    }

    struct HorizonEdge
    {
        public int face;
        public int edge0;
        public int edge1;
    }

    /// <summary>
    /// 存储凸包面信息的字典
    /// </summary>
    static Dictionary<int, Face> faces;

    /// <summary>
    /// 存储点和面的关系列表
    /// </summary>
    static List<PointFace> openSet;

    /// <summary>
    /// 存储已点亮的面集合
    /// </summary>
    static HashSet<int> litFaces;

    /// <summary>
    /// 存储地平线边缘列表
    /// </summary>
    static List<HorizonEdge> horizon;

    /// <summary>
    /// 存储凸包顶点映射字典
    /// </summary>
    static Dictionary<int, int> hullVerts;

    /// <summary>
    /// 开放集合尾部索引
    /// </summary>
    static int openSetTail = -1;

    /// <summary>
    /// 面的计数器
    /// </summary>
    static int faceCount = 0;

    /// <summary>
    /// 生成凸包的主方法
    /// </summary>
    /// <param name="points">输入的点集</param>
    /// <param name="splitVerts">是否分割顶点</param>
    /// <returns>返回生成的凸包网格数据(顶点列表,三角形列表,法线列表)</returns>
    public static (List<Vector3> verts, List<int> tris, List<Vector3> normals) GenerateHull(
        List<Vector3> points,
        bool splitVerts)
    {
        // 存储凸包的顶点列表
        List<Vector3> verts = new();
        // 存储凸包的三角形索引列表
        List<int> tris = new();
        // 存储凸包的法线列表
        List<Vector3> normals = new();

        // 检查输入点数量是否足够
        if (points.Count < 4)
        {
            throw new System.ArgumentException("Need at least 4 points to generate a convex hull");
        }

        // 初始化数据结构
        Initialize(points, splitVerts);

        // 生成初始的四面体凸包
        GenerateInitialHull(points);

        // 迭代添加剩余点,直到所有点都被处理
        while (openSetTail >= 0)
        {
            GrowHull(points);
        }

        // 导出凸包网格数据
        ExportMesh(points, splitVerts, ref verts, ref tris, ref normals);

        // 返回生成的凸包数据
        return (verts, tris, normals);
    }

    /// <summary>
    /// 初始化凸包生成所需的数据结构
    /// </summary>
    /// <param name="points">输入的点集</param>
    /// <param name="splitVerts">是否分割顶点</param>
    static void Initialize(List<Vector3> points, bool splitVerts)
    {
        // 重置面计数器和开放集合尾部索引
        faceCount = 0;
        openSetTail = -1;

        // 首次调用时初始化所需的数据结构
        if (faces == null)
        {
            faces = new Dictionary<int, Face>();
            litFaces = new HashSet<int>();
            horizon = new List<HorizonEdge>();
            openSet = new List<PointFace>(points.Count);
        }
        else
        {
            // 清空已有数据
            faces.Clear();
            litFaces.Clear();
            horizon.Clear();
            openSet.Clear();

            // 确保开放集合容量足够
            if (openSet.Capacity < points.Count)
            {
                openSet.Capacity = points.Count;
            }
        }

        // 如果不分割顶点,则初始化顶点映射字典
        if (!splitVerts)
        {
            if (hullVerts == null)
            {
                hullVerts = new Dictionary<int, int>();
            }
            else
            {
                hullVerts.Clear();
            }
        }
    }

    /// <summary>
    /// 生成初始的四面体凸包
    /// </summary>
    /// <param name="points">输入的点集</param>
    static void GenerateInitialHull(List<Vector3> points)
    {
        // 找到初始四面体的四个顶点索引
        int b0, b1, b2, b3;
        FindInitialHullIndices(points, out b0, out b1, out b2, out b3);

        // 获取四个顶点的坐标
        var v0 = points[b0];
        var v1 = points[b1];
        var v2 = points[b2];
        var v3 = points[b3];

        // 判断第四个点是否在三角形上方
        var above = Dot(v3 - v1, Cross(v1 - v0, v2 - v0)) > 0.0f;

        // 重置面计数器
        faceCount = 0;

        // 根据点的位置构建四个三角形面
        if (above)
        {
            faces[faceCount++] = new Face(b0, b2, b1, 3, 1, 2, Normal(points[b0], points[b2], points[b1]));
            faces[faceCount++] = new Face(b0, b1, b3, 3, 2, 0, Normal(points[b0], points[b1], points[b3]));
            faces[faceCount++] = new Face(b0, b3, b2, 3, 0, 1, Normal(points[b0], points[b3], points[b2]));
            faces[faceCount++] = new Face(b1, b2, b3, 2, 1, 0, Normal(points[b1], points[b2], points[b3]));
        }
        else
        {
            faces[faceCount++] = new Face(b0, b1, b2, 3, 2, 1, Normal(points[b0], points[b1], points[b2]));
            faces[faceCount++] = new Face(b0, b3, b1, 3, 0, 2, Normal(points[b0], points[b3], points[b1]));
            faces[faceCount++] = new Face(b0, b2, b3, 3, 1, 0, Normal(points[b0], points[b2], points[b3]));
            faces[faceCount++] = new Face(b1, b3, b2, 2, 0, 1, Normal(points[b1], points[b3], points[b2]));
        }

        // 将剩余点添加到开放集合中
        for (int i = 0; i < points.Count; i++)
        {
            if (i == b0 || i == b1 || i == b2 || i == b3) continue;

            openSet.Add(new PointFace(i, Unassigned, 0.0f));
        }

        // 将初始四面体的顶点标记为内部点
        openSet.Add(new PointFace(b0, Inside, float.NaN));
        openSet.Add(new PointFace(b1, Inside, float.NaN));
        openSet.Add(new PointFace(b2, Inside, float.NaN));
        openSet.Add(new PointFace(b3, Inside, float.NaN));

        openSetTail = openSet.Count - 5;

        // 为每个未分配的点找到最近的面
        for (int i = 0; i <= openSetTail; i++)
        {
            var assigned = false;
            var fp = openSet[i];

            for (int j = 0; j < 4; j++)
            {
                var face = faces[j];

                var dist = PointFaceDistance(points[fp.point], points[face.Vertex0], face);

                if (dist > 0)
                {
                    fp.face = j;
                    fp.distance = dist;
                    openSet[i] = fp;

                    assigned = true;
                    break;
                }
            }

            // 如果点在所有面的内部,将其标记为内部点
            if (!assigned)
            {
                fp.face = Inside;
                fp.distance = float.NaN;

                openSet[i] = openSet[openSetTail];
                openSet[openSetTail] = fp;

                openSetTail -= 1;
                i -= 1;
            }
        }
    }

    /// <summary>
    /// 寻找初始四面体的四个顶点索引
    /// </summary>
    /// <param name="points">输入点集</param>
    /// <param name="b0">第一个顶点索引</param>
    /// <param name="b1">第二个顶点索引</param>
    /// <param name="b2">第三个顶点索引</param>
    /// <param name="b3">第四个顶点索引</param>
    static void FindInitialHullIndices(List<Vector3> points, out int b0, out int b1, out int b2, out int b3)
    {
        var count = points.Count;

        // 遍历所有可能的四点组合
        for (int i0 = 0; i0 < count - 3; i0++)
        {
            for (int i1 = i0 + 1; i1 < count - 2; i1++)
            {
                var p0 = points[i0];
                var p1 = points[i1];

                // 检查两点是否重合
                if (AreCoincident(p0, p1)) continue;

                for (int i2 = i1 + 1; i2 < count - 1; i2++)
                {
                    var p2 = points[i2];

                    // 检查三点是否共线
                    if (AreCollinear(p0, p1, p2)) continue;

                    for (int i3 = i2 + 1; i3 < count - 0; i3++)
                    {
                        var p3 = points[i3];

                        // 检查四点是否共面
                        if (AreCoplanar(p0, p1, p2, p3)) continue;
                        // 找到合适的四点,返回它们的索引
                        b0 = i0;
                        b1 = i1;
                        b2 = i2;
                        b3 = i3;
                        return;
                    }
                }
            }
        }

        // 如果找不到合适的四点,说明所有点都共面
        throw new System.ArgumentException("Can't generate hull, points are coplanar");
    }

    /// <summary>
    /// 扩展凸包,添加新的点
    /// </summary>
    /// <param name="points">输入点集</param>
    static void GrowHull(List<Vector3> points)
    {
        // 找到距离当前凸包最远的点
        var farthestPoint = 0;
        var dist = openSet[0].distance;

        for (int i = 1; i <= openSetTail; i++)
        {
            if (openSet[i].distance > dist)
            {
                farthestPoint = i;
                dist = openSet[i].distance;
            }
        }

        // 寻找地平线边缘
        FindHorizon(
            points,
            points[openSet[farthestPoint].point],
            openSet[farthestPoint].face,
            faces[openSet[farthestPoint].face]);

        // 构建新的锥形凸包
        ConstructCone(points, openSet[farthestPoint].point);

        // 重新分配剩余点到新的凸包面
        ReassignPoints(points);
    }

    /// <summary>
    /// 寻找地平线边缘
    /// </summary>
    /// <param name="points">输入点集</param>
    /// <param name="point">当前处理的点</param>
    /// <param name="fi">起始面索引</param>
    /// <param name="face">起始面</param>
    static void FindHorizon(List<Vector3> points, Vector3 point, int fi, Face face)
    {
        // 清空已点亮的面集合和地平线边缘列表
        litFaces.Clear();
        horizon.Clear();

        // 将起始面添加到已点亮集合
        litFaces.Add(fi);

        // 检查第一条边的对面
        {
            var oppositeFace = faces[face.Opposite0];

            var dist = PointFaceDistance(
                point,
                points[oppositeFace.Vertex0],
                oppositeFace);

            // 如果点在对面的背面,将该边加入地平线
            if (dist <= 0.0f)
            {
                horizon.Add(new HorizonEdge
                {
                    face = face.Opposite0,
                    edge0 = face.Vertex1,
                    edge1 = face.Vertex2,
                });
            }
            // 否则继续递归搜索
            else
            {
                SearchHorizon(points, point, fi, face.Opposite0, oppositeFace);
            }
        }

        // 检查第二条边的对面(如果还未处理)
        if (!litFaces.Contains(face.Opposite1))
        {
            var oppositeFace = faces[face.Opposite1];

            var dist = PointFaceDistance(
                point,
                points[oppositeFace.Vertex0],
                oppositeFace);

            if (dist <= 0.0f)
            {
                horizon.Add(new HorizonEdge
                {
                    face = face.Opposite1,
                    edge0 = face.Vertex2,
                    edge1 = face.Vertex0,
                });
            }
            else
            {
                SearchHorizon(points, point, fi, face.Opposite1, oppositeFace);
            }
        }

        // 检查第三条边的对面(如果还未处理)
        if (!litFaces.Contains(face.Opposite2))
        {
            var oppositeFace = faces[face.Opposite2];

            var dist = PointFaceDistance(
                point,
                points[oppositeFace.Vertex0],
                oppositeFace);

            if (dist <= 0.0f)
            {
                horizon.Add(new HorizonEdge
                {
                    face = face.Opposite2,
                    edge0 = face.Vertex0,
                    edge1 = face.Vertex1,
                });
            }
            else
            {
                SearchHorizon(points, point, fi, face.Opposite2, oppositeFace);
            }
        }
    }

    /// <summary>
    /// 递归搜索地平线边缘
    /// </summary>
    /// <param name="points">输入点集</param>
    /// <param name="point">当前处理的点</param>
    /// <param name="prevFaceIndex">前一个面的索引</param>
    /// <param name="faceCount">当前面的索引</param>
    /// <param name="face">当前面</param>
    static void SearchHorizon(List<Vector3> points, Vector3 point, int prevFaceIndex, int faceCount, Face face)
    {
        // 将当前面添加到已点亮集合
        litFaces.Add(faceCount);

        // 声明变量用于存储相邻面和边的信息
        int nextFaceIndex0;
        int nextFaceIndex1;
        int edge0;
        int edge1;
        int edge2;

        // 根据前一个面是哪个对面,确定下一步要检查的面和边
        if (prevFaceIndex == face.Opposite0)
        {
            nextFaceIndex0 = face.Opposite1;
            nextFaceIndex1 = face.Opposite2;

            edge0 = face.Vertex2;
            edge1 = face.Vertex0;
            edge2 = face.Vertex1;
        }
        else if (prevFaceIndex == face.Opposite1)
        {
            nextFaceIndex0 = face.Opposite2;
            nextFaceIndex1 = face.Opposite0;

            edge0 = face.Vertex0;
            edge1 = face.Vertex1;
            edge2 = face.Vertex2;
        }
        else
        {
            nextFaceIndex0 = face.Opposite0;
            nextFaceIndex1 = face.Opposite1;

            edge0 = face.Vertex1;
            edge1 = face.Vertex2;
            edge2 = face.Vertex0;
        }

        // 检查第一个相邻面(如果还未处理)
        if (!litFaces.Contains(nextFaceIndex0))
        {
            var oppositeFace = faces[nextFaceIndex0];

            var dist = PointFaceDistance(
                point,
                points[oppositeFace.Vertex0],
                oppositeFace);

            if (dist <= 0.0f)
            {
                horizon.Add(new HorizonEdge
                {
                    face = nextFaceIndex0,
                    edge0 = edge0,
                    edge1 = edge1,
                });
            }
            else
            {
                SearchHorizon(points, point, faceCount, nextFaceIndex0, oppositeFace);
            }
        }

        // 检查第二个相邻面(如果还未处理)
        if (!litFaces.Contains(nextFaceIndex1))
        {
            var oppositeFace = faces[nextFaceIndex1];

            var dist = PointFaceDistance(
                point,
                points[oppositeFace.Vertex0],
                oppositeFace);

            if (dist <= 0.0f)
            {
                horizon.Add(new HorizonEdge
                {
                    face = nextFaceIndex1,
                    edge0 = edge1,
                    edge1 = edge2,
                });
            }
            else
            {
                SearchHorizon(points, point, faceCount, nextFaceIndex1, oppositeFace);
            }
        }
    }

    /// <summary>
    /// 构建锥形凸包
    /// </summary>
    /// <param name="points">输入点集</param>
    /// <param name="farthestPoint">最远点的索引</param>
    static void ConstructCone(List<Vector3> points, int farthestPoint)
    {
        // 移除所有已点亮的面
        foreach (var fi in litFaces)
        {
            faces.Remove(fi);
        }

        // 记录第一个新面的索引
        var firstNewFace = faceCount;

        // 为每条地平线边缘创建新的三角形面
        for (int i = 0; i < horizon.Count; i++)
        {
            var v0 = farthestPoint;
            var v1 = horizon[i].edge0;
            var v2 = horizon[i].edge1;

            var o0 = horizon[i].face;
            var o1 = (i == horizon.Count - 1) ? firstNewFace : firstNewFace + i + 1;
            var o2 = (i == 0) ? (firstNewFace + horizon.Count - 1) : firstNewFace + i - 1;

            var fi = faceCount++;

            // 创建新面
            faces[fi] = new Face(
                v0, v1, v2,
                o0, o1, o2,
                Normal(points[v0], points[v1], points[v2]));

            // 更新地平线边缘面的对面信息
            var horizonFace = faces[horizon[i].face];

            if (horizonFace.Vertex0 == v1)
            {
                horizonFace.Opposite1 = fi;
            }
            else if (horizonFace.Vertex1 == v1)
            {
                horizonFace.Opposite2 = fi;
            }
            else
            {
                horizonFace.Opposite0 = fi;
            }

            faces[horizon[i].face] = horizonFace;
        }
    }

    /// <summary>
    /// 重新分配点到新的凸包面
    /// </summary>
    /// <param name="points">输入点集</param>
    static void ReassignPoints(List<Vector3> points)
    {
        // 遍历开放集合中的所有点
        for (int i = 0; i <= openSetTail; i++)
        {
            var fp = openSet[i];

            // 如果点属于被点亮的面,需要重新分配
            if (litFaces.Contains(fp.face))
            {
                var assigned = false;
                var point = points[fp.point];

                // 遍历所有当前凸包面
                foreach (var kvp in faces)
                {
                    var fi = kvp.Key;
                    var face = kvp.Value;
                    // 计算点到面的距离
                    var dist = PointFaceDistance(
                        point,
                        points[face.Vertex0],
                        face);

                    // 如果点在面的正面,将其分配给该面
                    if (dist > Epsilon)
                    {
                        assigned = true;
                        fp.face = fi;
                        fp.distance = dist;

                        openSet[i] = fp;
                        break;
                    }
                }

                // 如果点不在任何面的正面,说明它在凸包内部
                if (!assigned)
                {
                    fp.face = Inside;
                    fp.distance = float.NaN;

                    // 将该点移到开放集合尾部
                    openSet[i] = openSet[openSetTail];
                    openSet[openSetTail] = fp;

                    i--;
                    openSetTail--;
                }
            }
        }
    }

    /// <summary>
    /// 导出凸包网格数据
    /// </summary>
    /// <param name="points">输入点集</param>
    /// <param name="splitVerts">是否分割顶点</param>
    /// <param name="verts">输出顶点列表</param>
    /// <param name="tris">输出三角形索引列表</param>
    /// <param name="normals">输出法线列表</param>
    static void ExportMesh(
        List<Vector3> points,
        bool splitVerts,
        ref List<Vector3> verts,
        ref List<int> tris,
        ref List<Vector3> normals)
    {
        // 初始化或清空输出列表
        if (verts == null)
        {
            verts = new List<Vector3>();
        }
        else
        {
            verts.Clear();
        }

        if (tris == null)
        {
            tris = new List<int>();
        }
        else
        {
            tris.Clear();
        }

        if (normals == null)
        {
            normals = new List<Vector3>();
        }
        else
        {
            normals.Clear();
        }

        // 遍历所有凸包面
        foreach (var face in faces.Values)
        {
            int vi0, vi1, vi2;

            // 如果需要分割顶点
            if (splitVerts)
            {
                // 为每个面的顶点创建新的顶点
                vi0 = verts.Count;
                verts.Add(points[face.Vertex0]);
                vi1 = verts.Count;
                verts.Add(points[face.Vertex1]);
                vi2 = verts.Count;
                verts.Add(points[face.Vertex2]);

                // 添加面法线
                normals.Add(face.normal);
                normals.Add(face.normal);
                normals.Add(face.normal);
            }
            else
            {
                // 复用已有顶点
                if (!hullVerts.TryGetValue(face.Vertex0, out vi0))
                {
                    vi0 = verts.Count;
                    hullVerts[face.Vertex0] = vi0;
                    verts.Add(points[face.Vertex0]);
                }

                if (!hullVerts.TryGetValue(face.Vertex1, out vi1))
                {
                    vi1 = verts.Count;
                    hullVerts[face.Vertex1] = vi1;
                    verts.Add(points[face.Vertex1]);
                }

                if (!hullVerts.TryGetValue(face.Vertex2, out vi2))
                {
                    vi2 = verts.Count;
                    hullVerts[face.Vertex2] = vi2;
                    verts.Add(points[face.Vertex2]);
                }
            }

            // 添加三角形索引
            tris.Add(vi0);
            tris.Add(vi1);
            tris.Add(vi2);
        }

        // 如果分割顶点则直接返回
        if (splitVerts)
            return;

        // 为每个顶点初始化法线
        for (int i = 0; i < verts.Count; i++)
        {
            normals.Add(Vector3.zero);
        }

        // 累加每个面对顶点法线的贡献
        foreach (var face in faces.Values)
        {
            normals[hullVerts[face.Vertex0]] += face.normal;
            normals[hullVerts[face.Vertex1]] += face.normal;
            normals[hullVerts[face.Vertex2]] += face.normal;
        }

        // 归一化所有顶点法线
        for (int i = 0; i < normals.Count; i++)
        {
            normals[i] = normals[i].normalized;
        }
    }

    /// <summary>
    /// 计算点到面的距离
    /// </summary>
    /// <param name="point">要计算的点</param>
    /// <param name="pointOnFace">面上的一个点</param>
    /// <param name="face">要计算距离的面</param>
    /// <returns>点到面的有符号距离</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static float PointFaceDistance(Vector3 point, Vector3 pointOnFace, Face face)
    {
        // 使用点积计算点到面的距离
        // 面法线与点到面上一点的向量的点积即为有符号距离
        return Dot(face.normal, point - pointOnFace);
    }

    /// <summary>
    /// 计算三角形面的法线向量
    /// </summary>
    /// <param name="v0">三角形第一个顶点</param>
    /// <param name="v1">三角形第二个顶点</param>
    /// <param name="v2">三角形第三个顶点</param>
    /// <returns>归一化的法线向量</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static Vector3 Normal(Vector3 v0, Vector3 v1, Vector3 v2)
    {
        // 使用两个边向量的叉积计算法线
        // 并进行归一化处理
        return Cross(v1 - v0, v2 - v0).normalized;
    }

    /// <summary>
    /// 计算两个向量的点积
    /// </summary>
    /// <param name="a">第一个向量</param>
    /// <param name="b">第二个向量</param>
    /// <returns>点积结果</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static float Dot(Vector3 a, Vector3 b)
    {
        // 计算向量的点积
        return a.x * b.x + a.y * b.y + a.z * b.z;
    }

    /// <summary>
    /// 计算两个向量的叉积
    /// </summary>
    /// <param name="a">第一个向量</param>
    /// <param name="b">第二个向量</param>
    /// <returns>叉积结果向量</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static Vector3 Cross(Vector3 a, Vector3 b)
    {
        // 计算向量的叉积
        return new Vector3(
            a.y * b.z - a.z * b.y,
            a.z * b.x - a.x * b.z,
            a.x * b.y - a.y * b.x);
    }

    /// <summary>
    /// 判断两点是否重合
    /// </summary>
    /// <param name="a">第一个点</param>
    /// <param name="b">第二个点</param>
    /// <returns>如果两点距离小于精度则返回true</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static bool AreCoincident(Vector3 a, Vector3 b)
    {
        // 判断两点之间的距离是否小于精度值
        return (a - b).magnitude <= Epsilon;
    }

    /// <summary>
    /// 判断三点是否共线
    /// </summary>
    /// <param name="a">第一个点</param>
    /// <param name="b">第二个点</param>
    /// <param name="c">第三个点</param>
    /// <returns>如果三点共线则返回true</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static bool AreCollinear(Vector3 a, Vector3 b, Vector3 c)
    {
        // 判断由三点构成的两个向量的叉积是否接近零向量
        return Cross(c - a, c - b).magnitude <= Epsilon;
    }

    /// <summary>
    /// 判断四点是否共面
    /// </summary>
    /// <param name="a">第一个点</param>
    /// <param name="b">第二个点</param>
    /// <param name="c">第三个点</param>
    /// <param name="d">第四个点</param>
    /// <returns>如果四点共面则返回true</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static bool AreCoplanar(Vector3 a, Vector3 b, Vector3 c, Vector3 d)
    {
        // 计算两个法线向量
        var n1 = Cross(c - a, c - b);
        var n2 = Cross(d - a, d - b);

        // 计算法线向量的长度
        var m1 = n1.magnitude;
        var m2 = n2.magnitude;

        // 判断法线向量是否平行或为零向量
        return m1 <= Epsilon
               || m2 <= Epsilon
               || AreCollinear(Vector3.zero,
                   (1.0f / m1) * n1,
                   (1.0f / m2) * n2);
    }
}