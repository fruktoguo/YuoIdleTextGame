using System;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Burst;
using UnityEngine;
using System.Collections.Generic;

public class JobifiedAStar : MonoBehaviour
{
    [Serializable]
    public struct GridSettings
    {
        public int width;
        public int height;
        public float nodeSize;
    }

    public GridSettings gridSettings;
    private NativeArray<bool> obstacles;


    public void SetGrid(bool[,] map)
    {
        obstacles = new NativeArray<bool>(map.GetLength(1) * map.GetLength(0), Allocator.Persistent);
        for (int y = 0; y < map.GetLength(1); y++)
        {
            for (int x = 0; x < map.GetLength(0); x++)
            {
                obstacles[y * gridSettings.width + x] = map[x, y];
            }
        }
    }

    private void OnDestroy()
    {
        if (obstacles.IsCreated) obstacles.Dispose();
    }

    public struct PathRequest
    {
        public float3 startPos;
        public float3 endPos;
    }

    public List<List<float3>> RequestPaths(List<PathRequest> requests)
    {
        NativeArray<PathRequest> jobRequests = new NativeArray<PathRequest>(requests.ToArray(), Allocator.TempJob);
        NativeList<float3> allPaths = new NativeList<float3>(Allocator.TempJob);
        NativeArray<int> pathLengths = new NativeArray<int>(requests.Count, Allocator.TempJob);

        AStarPathfindingJob job = new AStarPathfindingJob
        {
            requests = jobRequests,
            gridSize = new int2(gridSettings.width, gridSettings.height),
            nodeSize = gridSettings.nodeSize,
            obstacles = obstacles,
            allPaths = allPaths,
            pathLengths = pathLengths
        };

        JobHandle handle = job.Schedule(requests.Count, 1);
        handle.Complete();

        var resultList = new List<List<float3>>();
        // 处理结果
        int pathStartIndex = 0;
        for (int i = 0; i < requests.Count; i++)
        {
            var result = new List<float3>();
            int pathLength = pathLengths[i];
            for (int j = 0; j < pathLength; j++)
            {
                result.Add(allPaths[pathStartIndex + j]);
            }

            pathStartIndex += pathLength;

            // 在这里使用计算出的路径，例如分配给单位
            Debug.Log($"Path {i} found with {pathLength} points");
            resultList.Add(result);
        }

        // 清理
        jobRequests.Dispose();
        allPaths.Dispose();
        pathLengths.Dispose();
        return resultList;
    }

    [BurstCompile]
    public struct AStarPathfindingJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<PathRequest> requests;
        [ReadOnly] public int2 gridSize;
        [ReadOnly] public float nodeSize;
        [ReadOnly] public NativeArray<bool> obstacles;

        public NativeList<float3> allPaths;
        public NativeArray<int> pathLengths;

        public void Execute(int index)
        {
            PathRequest request = requests[index];
            int2 startNode = WorldToGrid(request.startPos);
            int2 targetNode = WorldToGrid(request.endPos);

            NativeList<int2> path = FindPath(startNode, targetNode);

            // 平滑路径
            NativeList<float3> smoothPath = SmoothPath(path);

            // 记录路径长度
            pathLengths[index] = smoothPath.Length;

            // 添加到所有路径中
            for (int i = 0; i < smoothPath.Length; i++)
            {
                allPaths.Add(smoothPath[i]);
            }

            path.Dispose();
            smoothPath.Dispose();
        }

        private NativeList<int2> FindPath(int2 startPos, int2 targetPos)
        {
            NativeList<int2> path = new NativeList<int2>(Allocator.Temp);
            NativeHashMap<int2, PathNode> allNodes =
                new NativeHashMap<int2, PathNode>(gridSize.x * gridSize.y, Allocator.Temp);
            NativeList<PathNode> openSet = new NativeList<PathNode>(Allocator.Temp);

            PathNode startNode = new PathNode
                { position = startPos, gCost = 0, hCost = ManhattanDistance(startPos, targetPos) };
            openSet.Add(startNode);
            allNodes.Add(startPos, startNode);

            while (openSet.Length > 0)
            {
                PathNode currentNode = GetLowestFCostNode(openSet);
                openSet.RemoveAtSwapBack(openSet.IndexOf(currentNode));

                if (currentNode.position.Equals(targetPos))
                {
                    RetracePath(allNodes, startPos, targetPos, ref path);
                    break;
                }

                for (int x = -1; x <= 1; x++)
                {
                    for (int y = -1; y <= 1; y++)
                    {
                        if (x == 0 && y == 0) continue;

                        int2 neighborPos = new int2(currentNode.position.x + x, currentNode.position.y + y);

                        if (!IsWalkable(neighborPos)) continue;

                        int newGCost = currentNode.gCost + ((x == 0 || y == 0) ? 10 : 14);
                        PathNode neighborNode;

                        if (!allNodes.TryGetValue(neighborPos, out neighborNode))
                        {
                            neighborNode = new PathNode
                            {
                                position = neighborPos,
                                parent = currentNode.position,
                                gCost = newGCost,
                                hCost = ManhattanDistance(neighborPos, targetPos)
                            };
                            allNodes.Add(neighborPos, neighborNode);
                            openSet.Add(neighborNode);
                        }
                        else if (newGCost < neighborNode.gCost)
                        {
                            neighborNode.gCost = newGCost;
                            neighborNode.parent = currentNode.position;
                            allNodes[neighborPos] = neighborNode;
                        }
                    }
                }
            }

            openSet.Dispose();
            allNodes.Dispose();
            return path;
        }

        private PathNode GetLowestFCostNode(NativeList<PathNode> openSet)
        {
            PathNode lowestNode = openSet[0];
            for (int i = 1; i < openSet.Length; i++)
            {
                if (openSet[i].fCost < lowestNode.fCost ||
                    (openSet[i].fCost == lowestNode.fCost && openSet[i].hCost < lowestNode.hCost))
                {
                    lowestNode = openSet[i];
                }
            }

            return lowestNode;
        }

        private void RetracePath(NativeHashMap<int2, PathNode> allNodes, int2 startPos, int2 endPos,
            ref NativeList<int2> path)
        {
            int2 currentPos = endPos;
            while (!currentPos.Equals(startPos))
            {
                path.Add(currentPos);
                currentPos = allNodes[currentPos].parent;
            }

            path.Add(startPos);
        }

        private NativeList<float3> SmoothPath(NativeList<int2> path)
        {
            NativeList<float3> smoothPath = new NativeList<float3>(Allocator.Temp);
            if (path.Length < 2) return smoothPath;

            smoothPath.Add(GridToWorld(path[0]));

            for (int i = 2; i < path.Length; i++)
            {
                float3 dir = GridToWorld(path[i]) - GridToWorld(path[i - 2]);
                float3 perp = math.normalize(new float3(-dir.z, 0, dir.x));
                float3 pos = GridToWorld(path[i - 1]);
                smoothPath.Add(pos + perp * nodeSize * 0.3f);
                smoothPath.Add(pos - perp * nodeSize * 0.3f);
            }

            smoothPath.Add(GridToWorld(path[path.Length - 1]));
            return smoothPath;
        }

        private bool IsWalkable(int2 pos)
        {
            if (pos.x < 0 || pos.x >= gridSize.x || pos.y < 0 || pos.y >= gridSize.y)
                return false;
            return !obstacles[pos.x + pos.y * gridSize.x];
        }

        private int ManhattanDistance(int2 a, int2 b)
        {
            return math.abs(a.x - b.x) + math.abs(a.y - b.y);
        }

        private int2 WorldToGrid(float3 worldPos)
        {
            return new int2(
                (int)(worldPos.x / nodeSize),
                (int)(worldPos.z / nodeSize)
            );
        }

        private float3 GridToWorld(int2 gridPos)
        {
            return new float3(
                gridPos.x * nodeSize + nodeSize / 2,
                0,
                gridPos.y * nodeSize + nodeSize / 2
            );
        }
    }

    public struct PathNode : IEquatable<PathNode>
    {
        public int2 position;
        public int2 parent;
        public int gCost;
        public int hCost;
        public int fCost => gCost + hCost;

        public bool Equals(PathNode other)
        {
            return position.Equals(other.position) && parent.Equals(other.parent) && gCost == other.gCost &&
                   hCost == other.hCost;
        }

        public override bool Equals(object obj)
        {
            return obj is PathNode node && Equals(node);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(position, parent, gCost, hCost);
        }
    }
}