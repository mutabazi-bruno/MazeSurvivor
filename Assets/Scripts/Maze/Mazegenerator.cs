using UnityEngine;

public class MazeGenerator : MonoBehaviour
{
    [SerializeField] private int width = 20;
    [SerializeField] private int height = 20;
    [SerializeField] private float cellSize = 2f;

    [Header("Wall prefab - thin rectangle, assigned in Inspector")]
    [SerializeField] private GameObject wallPrefab;

    [Header("Exit prefab - assigned in Inspector")]
    [SerializeField] private GameObject exitPrefab;
    [SerializeField] private Vector2Int spawnCell = Vector2Int.zero; 

    private MazeCell[,] grid;

    public int Width => width;
    public int Height => height;
    public float CellSize => cellSize;

    public static MazeGenerator Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        GenerateMaze();
        BuildWalls();
        SpawnExit();
    }

    private void SpawnExit()
    {
        Vector2Int exitCell = MazePathfinder.FindFarthestCell(spawnCell, this);
        Vector3 exitWorldPos = CellToWorld(exitCell);
        Instantiate(exitPrefab, exitWorldPos, Quaternion.identity);
    }

    private void GenerateMaze()
    {
        grid = new MazeCell[width, height];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                grid[x, y] = new MazeCell();
            }
        }

        // this is the recursive call that does all the actual work starts carving from (0,0)

        CarvePath(0, 0);
    }

    private void CarvePath(int x, int y)
    {
        grid[x, y].visited = true;

        // check all 4 directions in random order, so the maze doesn't look predictable

        foreach (Vector2Int direction in GetShuffledDirections())
        {
            int neighborX = x + direction.x;
            int neighborY = y + direction.y;

            // skip if this neighbor is outside the grid, or already visited

            if (!IsInBounds(neighborX, neighborY)) continue;
            if (grid[neighborX, neighborY].visited) continue;

            RemoveWallBetween(x, y, neighborX, neighborY);

            CarvePath(neighborX, neighborY);
        }
    }

    private Vector2Int[] GetShuffledDirections()
    {
        Vector2Int[] directions = { Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left };

        // simple shuffle  swap each slot with a random other slot

        for (int i = directions.Length - 1; i > 0; i--)
        {
            int swapIndex = Random.Range(0, i + 1);
            (directions[i], directions[swapIndex]) = (directions[swapIndex], directions[i]);
        }

        return directions;
    }

    private bool IsInBounds(int x, int y)
    {
        return x >= 0 && x < width && y >= 0 && y < height;
    }

    private void RemoveWallBetween(int x1, int y1, int x2, int y2)
    {
        if (x2 == x1 + 1) { grid[x1, y1].hasRightWall = false; grid[x2, y2].hasLeftWall = false; }
        else if (x2 == x1 - 1) { grid[x1, y1].hasLeftWall = false; grid[x2, y2].hasRightWall = false; }
        else if (y2 == y1 + 1) { grid[x1, y1].hasTopWall = false; grid[x2, y2].hasBottomWall = false; }
        else if (y2 == y1 - 1) { grid[x1, y1].hasBottomWall = false; grid[x2, y2].hasTopWall = false; }
    }

    private void BuildWalls()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3 cellWorldPos = new Vector3(x * cellSize, y * cellSize, 0);

                if (grid[x, y].hasTopWall)
                {
                    PlaceWall(cellWorldPos + new Vector3(cellSize / 2f, cellSize, 0), horizontal: true);
                }
                if (grid[x, y].hasLeftWall)
                {
                    PlaceWall(cellWorldPos + new Vector3(0, cellSize / 2f, 0), horizontal: false);
                }
                if (x == width - 1 && grid[x, y].hasRightWall)
                {
                    PlaceWall(cellWorldPos + new Vector3(cellSize, cellSize / 2f, 0), horizontal: false);
                }
                if (y == 0 && grid[x, y].hasBottomWall)
                {
                    PlaceWall(cellWorldPos + new Vector3(cellSize / 2f, 0, 0), horizontal: true);
                }
            }
        }
    }

    private void PlaceWall(Vector3 position, bool horizontal)
    {
        GameObject wall = Instantiate(wallPrefab, position, Quaternion.identity, transform);
        // thin in one direction, full cellSize in the other horizontal wall is wide+short, vertical is tall+thin

        wall.transform.localScale = horizontal
            ? new Vector3(cellSize, 0.1f, 1f)
            : new Vector3(0.1f, cellSize, 1f);
    }


    public Vector2Int WorldToCell(Vector3 worldPosition)
    {
        int x = Mathf.FloorToInt(worldPosition.x / cellSize);
        int y = Mathf.FloorToInt(worldPosition.y / cellSize);
        return new Vector2Int(x, y);
    }

    public Vector3 CellToWorld(Vector2Int cell)
    {
        return new Vector3(cell.x * cellSize + cellSize / 2f, cell.y * cellSize + cellSize / 2f, 0);
    }

    // can you walk directly from one cell to an adjacent cell? (false if a wall blocks that specific direction)

    public bool CanMoveBetween(Vector2Int from, Vector2Int to)
    {
        if (!IsInBounds(from.x, from.y) || !IsInBounds(to.x, to.y)) return false;

        MazeCell fromCell = grid[from.x, from.y];

        if (to.x == from.x + 1) return !fromCell.hasRightWall;
        if (to.x == from.x - 1) return !fromCell.hasLeftWall;
        if (to.y == from.y + 1) return !fromCell.hasTopWall;
        if (to.y == from.y - 1) return !fromCell.hasBottomWall;

        return false; // not actually adjacent
    }

    public Vector2Int GetRandomCell()
    {
        return new Vector2Int(Random.Range(0, width), Random.Range(0, height));
    }
}