using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using YuoTools;

public static class YuoSearchHelper
{
    public static int[,] Map { get; private set; }
    private static MapNode[] nodeMap;

    public static void DisposeAll(this ICollection<IDisposable> array)
    {
        foreach (var t in array)
        {
            t.Dispose();
        }
    }

    public static void SetMap(int[,] map)
    {
        Map = map;
        nodeMap = new MapNode[map.GetLength(0) * map.GetLength(1)];
        for (int x = 0; x < map.GetLength(0); x++)
        {
            for (int y = 0; y < map.GetLength(1); y++)
            {
                nodeMap[x * map.GetLength(1) + y] = new MapNode(x, y, map[x, y] != 1);
            }
        }
    }

    /// <summary>
    /// 用Jobs搜索单个路径
    /// </summary>
    public static int2[] SearchOfJobs(Vector2Int startPos, Vector2Int endPos)
    {
        var data = InitJob(new[] { startPos }, new[] { endPos });
        data.job.Schedule().Complete();
        int2[] result = data.result.ToArray();
        data.disposables.DisposeAll();
        return result;
    }

    /// <summary>
    /// 用Jobs搜索多个路径
    /// </summary>
    public static int2[][] SearchOfJobs(Vector2Int[] startPos, Vector2Int[] endPos)
    {
        var data = InitJob(startPos, endPos);
        data.job.Schedule().Complete();
        var result = SplitPath(data.result.ToArray(), data.resultIndex.ToArray());
        data.disposables.DisposeAll();
        return result;
    }

    /// <summary>
    /// 拆分路径
    /// </summary>
    static int2[][] SplitPath(int2[] path, int[] resultIndex)
    {
        int2[][] result = new int2[resultIndex.Length][];
        int startIndex = 0;
        for (int i = 0; i < resultIndex.Length; i++)
        {
            var endIndex = resultIndex[i];
            int2[] resultItem = new int2[endIndex - startIndex];
            Array.Copy(path, startIndex, resultItem, 0, endIndex - startIndex);
            result[i] = resultItem;
            startIndex = endIndex;
        }

        return result;
    }

    public static int2[][] Search(Vector2Int[] startPos, Vector2Int[] endPos)
    {
        var jobCount = CalculateJobNum(startPos.Length);
        int lastIndex = 0;
        NativeArray<JobHandle> jobs = new NativeArray<JobHandle>(jobCount.Length, Allocator.TempJob);
        List<IDisposable> disposables = new List<IDisposable>();
        List<NativeList<int2>> resultList = new List<NativeList<int2>>();
        List<NativeArray<int>> resultIndexList = new List<NativeArray<int>>();
        for (int i = 0; i < jobCount.Length; i++)
        {
            var startArray = startPos.Skip(lastIndex).Take(jobCount[i]).ToArray();
            var endArray = endPos.Skip(lastIndex).Take(jobCount[i]).ToArray();
            var jobData = InitJob(startArray, endArray);
            var result = jobData.result;
            var resultIndex = jobData.resultIndex;
            resultList.Add(result);
            resultIndexList.Add(resultIndex);
            jobs[i] = jobData.job.Schedule();
            disposables.AddRange(jobData.disposables);
        }

        JobHandle.CompleteAll(jobs);
        int2[][] totalResult = new int2[startPos.Length][];
        lastIndex = 0;
        for (int i = 0; i < resultList.Count; i++)
        {
            var r = SplitPath(resultList[i].ToArray(), resultIndexList[i].ToArray());
            for (int j = 0; j < r.Length; j++)
            {
                totalResult[lastIndex + j] = r[j];
            }

            lastIndex += r.Length;
        }

        jobs.Dispose();
        disposables.DisposeAll();
        return totalResult;
    }

    static int[] CalculateJobNum(int allCount)
    {
        const int normalMaxJobCount = 100;
        const int overflowThreshold = 20 * normalMaxJobCount;
        const int maxJobs = 20;
        int jobNum;
        int[] jobArray;

        if (allCount <= overflowThreshold)
        {
            // 如果所有的计数都小于或等于阈值，我们按正常方式进行计算。
            jobNum = allCount / normalMaxJobCount;

            // 如果不能平均分配，那么我们将需要额外的一个任务来处理余下的部分。
            if (allCount % normalMaxJobCount > 0)
                jobNum++;

            jobArray = new int[jobNum];

            for (var i = 0; i < jobNum; i++)
            {
                jobArray[i] = normalMaxJobCount;
            }
        }
        else
        {
            // 如果总计数超过阈值，我们将创建 20 个作业，并平均分配所有任务。
            jobNum = maxJobs;
            int singleJobCount = allCount / maxJobs;
            jobArray = new int[jobNum];

            // 如果不能平均分配，余下的任务将分配给一部分作业。
            var remainder = allCount % maxJobs;
            for (int i = 0; i < jobNum; i++)
            {
                jobArray[i] = singleJobCount;

                // 如果还有剩余的任务，将它分配给当前的作业。
                if (remainder > 0)
                {
                    jobArray[i]++;
                    remainder--;
                }
            }
        }

        return jobArray;
    }

    private static (AStarSearchJob job, NativeList<int2> result, NativeArray<int> resultIndex, IDisposable[] disposables
        )
        InitJob(Vector2Int[] startPos,
            Vector2Int[] endPos)
    {
        var map = new NativeArray<MapNode>(nodeMap, Allocator.TempJob);

        NativeQueue<MapNode> openQueue = new NativeQueue<MapNode>(Allocator.TempJob);
        var result = new NativeList<int2>(Allocator.TempJob);
        var resultIndex = new NativeArray<int>(startPos.Length, Allocator.TempJob);

        var startPosArray = new NativeArray<int2>(startPos.Length, Allocator.TempJob);
        for (int i = 0; i < startPos.Length; i++)
        {
            startPosArray[i] = new int2(startPos[i].x, startPos[i].y);
        }

        var endPosArray = new NativeArray<int2>(endPos.Length, Allocator.TempJob);
        for (int i = 0; i < endPos.Length; i++)
        {
            endPosArray[i] = new int2(endPos[i].x, endPos[i].y);
        }

        var job = new AStarSearchJob()
        {
            StartPosArray = startPosArray,
            EndPosArray = endPosArray,
            Min = new MapNode(int.MaxValue, int.MaxValue, false),
            OpenQueue = openQueue,
            Result = result,
            ResultIndex = resultIndex,
            NodeMap = map,
            MapSizeX = Map.GetLength(0),
            MapSizeY = Map.GetLength(1),
            SearchIndex = 0,
        };
        return (job, result, resultIndex,
            new IDisposable[] { startPosArray, endPosArray, map, openQueue, result, resultIndex });
    }
}

public struct MapNode : IEquatable<MapNode>, IComparable<MapNode>
{
    public bool Equals(MapNode other)
    {
        return Pos.x == other.Pos.x && Pos.y == other.Pos.y;
    }

    public int CompareTo(MapNode other)
    {
        return F.CompareTo(other.F);
    }

    public bool CanMove;
    public bool Open;
    public bool Close;

    public int F;
    public int G;
    public int H;

    public int2 Parent;

    public int2 Pos;

    public MapNode(int x, int y, bool canMove)
    {
        Pos = new int2(x, y);
        CanMove = canMove;
        Open = false;
        Close = false;
        F = 0;
        G = 0;
        H = 0;
        Parent = new(int.MinValue, int.MinValue);
    }

    public void Reset()
    {
        Open = false;
        Close = false;
        F = 0;
        G = 0;
        H = 0;
        Parent = new(int.MinValue, int.MinValue);
    }

    public void OpenNode(MapNode parent, int2 target)
    {
        Parent = parent.Pos;
        int wi = Math.Abs(Pos.x - parent.Pos.x);
        int he = Math.Abs(Pos.y - parent.Pos.y);
        if (wi == 1 && he == 1)
        {
            G = parent.G + 14;
        }
        else
            G = parent.G + 10;

        H = (Math.Abs(Pos.x - target.x) + Math.Abs(Pos.y - target.y)) * 10;
        F = G + H;
    }
}

[BurstCompile]
public struct AStarSearchJob : IJob
{
    public NativeArray<MapNode> NodeMap;
    public int MapSizeX;
    public int MapSizeY;
    public NativeQueue<MapNode> OpenQueue;
    public NativeArray<int2> StartPosArray;
    public NativeArray<int2> EndPosArray;
    public MapNode Min;
    public NativeList<int2> Result;
    public NativeArray<int> ResultIndex;
    public int SearchIndex;
    int2 StartPos => StartPosArray[SearchIndex];
    int2 EndPos => EndPosArray[SearchIndex];

    public void Execute()
    {
        for (var index = 0; index < StartPosArray.Length; index++)
        {
            ExecuteIndex(index);
            ResetMap();
        }
    }

    void ExecuteIndex(int index)
    {
        SearchIndex = index;
        var startNode = GetNode(StartPos);
        var endNode = GetNode(EndPos);
        Min = startNode;
        Open(StartPos, default, false);

        int maxSearchNum = MapSizeX * MapSizeY * 100;

        while (OpenQueue.Count > 0)
        {
            maxSearchNum--;
            if (maxSearchNum < 0)
            {
                Debug.LogError("运行超时");
                return;
            }

            FindNeighbors(Min);
            Close(Min.Pos);
            if (GetNode(EndPos).Open)
            {
                // Debug.Log($"AStar运行成功,总共计算{MapSizeX * MapSizeY * 100 - maxSearchNum}次");
                SetResult();
                return;
            }
        }
    }

    void SetResult()
    {
        NativeList<int2> path = new NativeList<int2>(Allocator.Temp);
        var pos = EndPos;
        while (!pos.Equals(StartPos))
        {
            path.Add(pos);
            pos = GetNode(pos).Parent;
        }

        Result.AddRange(path);
        ResultIndex[SearchIndex] = Result.Length;
        path.Dispose();
    }

    void ResetMap()
    {
        for (var index = 0; index < NodeMap.Length; index++)
        {
            var mapNode = NodeMap[index];
            mapNode.Reset();
            NodeMap[index] = mapNode;
        }

        OpenQueue.Clear();
    }

    MapNode GetNode(int2 pos) => NodeMap[pos.x * MapSizeY + pos.y];

    bool InRange(int2 pos) => pos.x.InRange(0, MapSizeX - 1) && pos.y.InRange(0, MapSizeY - 1);

    private void FindNeighbors(MapNode grid)
    {
        for (int x = -1; x < 2; x++)
        {
            for (int y = -1; y < 2; y++)
            {
                if (x == 0 && y == 0)
                    continue;
                var nextPos = grid.Pos + new int2(x, y);
                if (!InRange(nextPos))
                    continue;
                var gridTemp = GetNode(nextPos);
                if (!gridTemp.CanMove)
                    continue;
                if (gridTemp.Open || gridTemp.Close)
                    continue;
                if (x != y && x != -y)
                {
                    Open(gridTemp.Pos, grid);
                }
                else if (true)
                {
                    //Open(gridTemp);
                }
            }
        }
    }

    void Open(int2 pos, MapNode parent, bool hasParent = true)
    {
        var node = GetNode(pos);
        if (node.Open == false)
        {
            OpenQueue.Enqueue(node);
        }

        if (hasParent) node.OpenNode(parent, EndPos);
        node.Open = true;
        node.Close = false;
        if (node.F < Min.F)
        {
            Min = node;
        }

        SetMapNode(pos, node);
    }

    void SetMapNode(int2 pos, MapNode node)
    {
        NodeMap[pos.x * MapSizeY + pos.y] = node;
    }

    void Close(int2 pos)
    {
        var node = GetNode(pos);
        if (node.Open)
        {
            OpenQueue.Dequeue();
            if (OpenQueue.Count > 0)
            {
                Min = OpenQueue.Peek();
            }

            SetMapNode(pos, node);
        }
    }
}