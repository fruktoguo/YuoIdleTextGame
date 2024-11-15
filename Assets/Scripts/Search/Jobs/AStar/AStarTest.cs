using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEditor;
using YuoTools;
using Random = UnityEngine.Random;

public class AStarTest : MonoBehaviour
{
    [Required] [InlineEditor] [SerializeField]
    private JobifiedAStar aStarAlgorithm;

    [TitleGroup("Map Settings")] [MinValue(1)] [SerializeField]
    private int width = 20;

    [TitleGroup("Map Settings")] [MinValue(1)] [SerializeField]
    private int height = 20;

    [TitleGroup("Map Settings")] [Range(0, 1)] [SerializeField]
    private float obstaclePercentage = 0.3f;

    [TitleGroup("Path Finding")] [ValidateInput("IsValidPoint", "Start point is not valid")] [SerializeField]
    private Vector2Int startPoint;

    [TitleGroup("Path Finding")] [ValidateInput("IsValidPoint", "End point is not valid")] [SerializeField]
    private Vector2Int endPoint;

    [TitleGroup("Map Visualization")]
    [TableMatrix(HorizontalTitle = "X", VerticalTitle = "Y", DrawElementMethod = nameof(DrawMapCell),
        ResizableColumns = false, SquareCells = true)]
    [ShowInInspector]
    private bool[,] map;

    [ReadOnly] [ShowInInspector] private List<Vector2Int> openSet = new();

    [ReadOnly] [ShowInInspector] private List<Vector2Int> closedSet = new();

    [ReadOnly] [ShowInInspector] private List<Vector2Int> path = new();

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

        // Update A* grid
        aStarAlgorithm.SetGrid(map);

        openSet.Clear();
        closedSet.Clear();
        path.Clear();
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

        float3 startPos = new(startPoint.x, 0, startPoint.y);
        float3 endPos = new(endPoint.x, 0, endPoint.y);
        var result = aStarAlgorithm.RequestPaths(new List<JobifiedAStar.PathRequest>()
        {
            new()
            {
                startPos = startPos,
                endPos = endPos
            }
        });
        result[0].LogAll();
        if (path == null || path.Count == 0)
        {
            Debug.Log("No path found!");
            path = new();
        }
        else
        {
            Debug.Log($"Path found with {path.Count} steps.");
        }
    }

    private bool IsValidPoint(Vector2Int point)
    {
        return point.x >= 0 && point.x < width && point.y >= 0 && point.y < height &&
               (map == null || map[point.x, point.y]);
    }

    private bool DrawMapCell(Rect rect, int x, int y)
    {
        if (map == null) return false;

        Color cellColor = map[x, y] ? Color.white : Color.black;

        Vector2Int currentPoint = new Vector2Int(x, y);
        if (currentPoint == startPoint)
            cellColor = Color.blue;
        else if (currentPoint == endPoint)
            cellColor = Color.red;
        else if (path.Contains(currentPoint))
            cellColor = Color.green;
        else if (openSet.Contains(currentPoint))
            cellColor = Color.yellow;
        else if (closedSet.Contains(currentPoint))
            cellColor = new Color(1f, 0.5f, 0f); // Orange

        EditorGUI.DrawRect(rect, cellColor);
        return map[x, y];
    }

    [Button("Clear Path and Sets", ButtonSizes.Medium), GUIColor(0.7f, 0.7f, 0.7f)]
    [EnableIf("@path.Count > 0 || openSet.Count > 0 || closedSet.Count > 0")]
    private void ClearPathAndSets()
    {
        path.Clear();
        openSet.Clear();
        closedSet.Clear();
    }

    [Button("Randomize Start and End Points", ButtonSizes.Medium), GUIColor(1, 1, 0)]
    [EnableIf("@map != null")]
    private void RandomizeStartAndEndPoints()
    {
        do
        {
            startPoint = new Vector2Int(Random.Range(0, width), Random.Range(0, height));
        } while (!IsValidPoint(startPoint));

        do
        {
            endPoint = new Vector2Int(Random.Range(0, width), Random.Range(0, height));
        } while (!IsValidPoint(endPoint) || endPoint == startPoint);
    }
}