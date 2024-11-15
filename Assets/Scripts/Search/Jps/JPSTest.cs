using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using Sirenix.Serialization;
using Unity.Mathematics;
using YuoTools.Search;
using UnityEditor;
using Random = UnityEngine.Random;

public class JPSTest : MonoBehaviour
{
    [Required] [InlineEditor] [SerializeField]
    private JPS jpsAlgorithm;

    [TitleGroup("Map Settings")] [MinValue(1)] [SerializeField]
    private int width = 20;

    [TitleGroup("Map Settings")] [MinValue(1)] [SerializeField]
    private int height = 20;

    [TitleGroup("Map Settings")] [Range(0, 1)] [SerializeField]
    private float obstaclePercentage = 0.3f;

    [TitleGroup("Path Finding")] [ValidateInput("IsValidPoint", "Start point is not valid")] [SerializeField]
    private int2 startPoint;

    [TitleGroup("Path Finding")] [ValidateInput("IsValidPoint", "End point is not valid")] [SerializeField]
    private int2 endPoint;

    [TitleGroup("Map Visualization")]
    [TableMatrix(HorizontalTitle = "X", VerticalTitle = "Y", DrawElementMethod = nameof(DrawMapCell),
        ResizableColumns = false, SquareCells = true)]
    [ShowInInspector]
    [OdinSerialize]
    private bool[,] map;

    // [ReadOnly]
    // [ShowInInspector]
    private List<int2> jumpPoints = new();

    // [ReadOnly]
    // [ShowInInspector]
    private List<int2> path = new();

    [Button("Generate Map", ButtonSizes.Large), GUIColor(0, 1, 0)]
    private void GenerateMap()
    {
        map = new bool[width, height];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                map[x, y] = Random.value > obstaclePercentage;
            }
        }

        // Ensure start and end points are walkable
        map[startPoint.x, startPoint.y] = true;
        map[endPoint.x, endPoint.y] = true;

        // Update JPS grid
        jpsAlgorithm = new JPS(map);

        jumpPoints.Clear();
        path.Clear();
    }

    [Button("Generate Jump Points", ButtonSizes.Large), GUIColor(1, 0.5f, 0)]
    [EnableIf("@map != null")]
    private void GenerateJumpPoints()
    {
        jumpPoints.Clear();
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (map[x, y] && IsJumpPoint(new int2(x, y)))
                {
                    jumpPoints.Add(new int2(x, y));
                }
            }
        }

        Debug.Log($"Found {jumpPoints.Count} jump points.");
    }

    [Button("Find Path", ButtonSizes.Large), GUIColor(0, 0.5f, 1)]
    [EnableIf("@map != null")]
    private void FindPath()
    {
        if (!IsValidPoint(startPoint) || !IsValidPoint(endPoint))
        {
            Debug.LogError("Invalid start or end point!");
            return;
        }

        jpsAlgorithm.SetStart(startPoint);
        jpsAlgorithm.SetGoal(endPoint);
        var success = jpsAlgorithm.StepAll();
        Debug.Log(success);
        path = jpsAlgorithm.GetPaths();
    }

    private bool IsValidPoint(int2 point)
    {
        return point.x >= 0 && point.x < width && point.y >= 0 && point.y < height &&
               (map == null || map[point.x, point.y]);
    }

    private bool IsJumpPoint(int2 point)
    {
        // This is a simplified check. You might want to implement a more sophisticated jump point detection.
        int neighborCount = 0;
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0) continue;
                int2 neighbor = new int2(point.x + x, point.y + y);
                if (IsValidPoint(neighbor) && map[neighbor.x, neighbor.y])
                {
                    neighborCount++;
                }
            }
        }

        return neighborCount > 1 && neighborCount < 8;
    }

    private bool DrawMapCell(Rect rect, int x, int y)
    {
        if (map == null) return false;

        Color cellColor = map[x, y] ? Color.white : Color.black;

        int2 currentPoint = new int2(x, y);
        if (currentPoint.Equals(startPoint))
            cellColor = Color.blue;
        else if (currentPoint.Equals(endPoint))
            cellColor = Color.red;
        else if (path.Contains(currentPoint))
            cellColor = Color.green;
        else if (jumpPoints.Contains(currentPoint))
            cellColor = Color.yellow;

        EditorGUI.DrawRect(rect, cellColor);
        return map[x, y];
    }

    [Button("Clear Path and Jump Points", ButtonSizes.Medium), GUIColor(0.7f, 0.7f, 0.7f)]
    [EnableIf("@path.Count > 0 || jumpPoints.Count > 0")]
    private void ClearPathAndJumpPoints()
    {
        path.Clear();
        jumpPoints.Clear();
    }

    [Button("Randomize Start and End Points", ButtonSizes.Medium), GUIColor(1, 1, 0)]
    [EnableIf("@map != null")]
    private void RandomizeStartAndEndPoints()
    {
        do
        {
            startPoint = new int2(Random.Range(0, width), Random.Range(0, height));
        } while (!IsValidPoint(startPoint));

        do
        {
            endPoint = new int2(Random.Range(0, width), Random.Range(0, height));
        } while (!IsValidPoint(endPoint) || endPoint.Equals(startPoint));
    }
}