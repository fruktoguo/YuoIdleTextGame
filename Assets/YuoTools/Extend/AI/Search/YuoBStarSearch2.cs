using UnityEngine;
using System;
using System.Collections.Generic;
using YuoTools.Extend.Helper;
using YuoTools.Extend.MathFunction;

namespace YuoTools
{
    public class YuoBStarSearch2
    {
        public class YuoGrid
        {
            public YuoInt2 Pos { get; }
            public bool CanMove { get; }
            public bool IsMoved { get;  set; }
            public YuoGrid Parent { get; private set; }
            public int PathIndex { get; private set; }

            public YuoGrid(int x, int y, bool canMove)
            {
                Pos = new YuoInt2(x, y);
                CanMove = canMove;
            }

            public void Reset()
            {
                Parent = null;
                IsMoved = false;
                PathIndex = 0;
            }

            public void MoveTo(YuoGrid grid)
            {
                grid.Parent = this;
                grid.IsMoved = true;
                grid.PathIndex = PathIndex + 1;
            }
        }

        private readonly YuoGrid[,] _map;
        private readonly int _mapSizeX;
        private readonly int _mapSizeY;
        private YuoInt2 _startPoint;
        private YuoInt2 _endPoint;
        private int _maxSearchNum;
        private bool _searchEnd;

        public YuoBStarSearch2(int[,] data)
        {
            _mapSizeX = data.GetLength(0);
            _mapSizeY = data.GetLength(1);
            _map = new YuoGrid[_mapSizeX, _mapSizeY];

            for (int x = 0; x < _mapSizeX; x++)
            {
                for (int y = 0; y < _mapSizeY; y++)
                {
                    _map[x, y] = new YuoGrid(x, y, data[x, y] == 0);
                }
            }
        }

        public List<YuoInt2> Search(YuoInt2 start, YuoInt2 end)
        {
            if (!IsValidPosition(start) || !IsValidPosition(end))
            {
                Debug.LogError("Invalid start or end position");
                return new List<YuoInt2>();
            }

            ResetMap();
            _startPoint = start;
            _endPoint = end;
            _maxSearchNum = _mapSizeX * _mapSizeY * 100;
            _searchEnd = false;

            StopwatchHelper.Start();
            Move();
            var ms = StopwatchHelper.Stop();

            Debug.Log($"BStar search {(_searchEnd ? "succeeded" : "failed")}, " +
                      $"calculated {_mapSizeX * _mapSizeY * 100 - _maxSearchNum} times, " +
                      $"took {ms} milliseconds");

            return _searchEnd ? ReconstructPath() : new List<YuoInt2>();
        }

        private void Move()
        {
            var queue = new Queue<YuoGrid>();
            var startGrid = _map[_startPoint.x, _startPoint.y];
            queue.Enqueue(startGrid);
            startGrid.IsMoved = true;

            while (queue.Count > 0 && _maxSearchNum > 0)
            {
                var current = queue.Dequeue();

                if (IsEnd(current.Pos))
                {
                    _searchEnd = true;
                    return;
                }

                GoToNext(current, queue);
            }
        }

        private void GoToNext(YuoGrid current, Queue<YuoGrid> queue)
        {
            _maxSearchNum--;

            int dx = Math.Sign(_endPoint.x - current.Pos.x);
            int dy = Math.Sign(_endPoint.y - current.Pos.y);

            var directions = new List<YuoInt2>
            {
                new YuoInt2(dx, 0),
                new YuoInt2(0, dy),
                new YuoInt2(-dx, 0),
                new YuoInt2(0, -dy)
            };

            foreach (var dir in directions)
            {
                YuoInt2 nextPos = current.Pos + dir;
                if (IsValidPosition(nextPos) && CanMove(nextPos))
                {
                    var nextGrid = _map[nextPos.x, nextPos.y];
                    if (!nextGrid.IsMoved)
                    {
                        current.MoveTo(nextGrid);
                        queue.Enqueue(nextGrid);
                    }
                }
            }
        }

        private bool IsValidPosition(YuoInt2 pos)
        {
            return pos.x >= 0 && pos.x < _mapSizeX && pos.y >= 0 && pos.y < _mapSizeY;
        }

        private bool CanMove(YuoInt2 pos)
        {
            return _map[pos.x, pos.y].CanMove;
        }

        private bool IsEnd(YuoInt2 point)
        {
            return point.x == _endPoint.x && point.y == _endPoint.y;
        }

        private void ResetMap()
        {
            foreach (var grid in _map)
            {
                grid.Reset();
            }
        }

        private List<YuoInt2> ReconstructPath()
        {
            var path = new List<YuoInt2>();
            var current = _map[_endPoint.x, _endPoint.y];

            while (current != null && current.Pos != _startPoint)
            {
                path.Add(current.Pos);
                current = current.Parent;
            }

            path.Reverse();
            return path;
        }
    }
}
