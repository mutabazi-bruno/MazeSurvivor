using UnityEngine;

// builds a maze using Recursive Backtracking:
// start at a cell, randomly visit an unvisited neighbor, knock down the wall between them,
// repeat from the neighbor - if stuck (no unvisited neighbors), backtrack to the previous cell
// and try again from there. this guarantees every cell is reachable (a fully solvable maze)
// with no loops, since we only ever connect to cells we haven't visited yet.
public class MazeGenerator : MonoBehaviour
{
    [SerializeField] private int width = 20;
    [SerializeField] private int height = 20;
    [SerializeField] private float cellSize = 2f;

    [Header("Wall prefab - thin rectangle, assigned in Inspector")]
    [SerializeField] private GameObject wallPrefab;

    [Header("Exit prefab - assigned in Inspector")]
    [SerializeField] private GameObject exitPrefab;
    [SerializeField] private Vector2Int spawnCell = Vector2Int.zero; // wherever your Player actually starts

    private MazeCell[,] grid;

    // other scripts (like the pathfinder) need this data, so we expose it in a controlled way -
    // still encapsulation: nobody outside can directly edit the grid, only read through these
    public int Width => width;
    public int Height => height;
    public float CellSize => cellSize;

    // only one maze ever exists in this game, so a simple static reference is fine here -
    // simpler than a full Singleton since we don't need the "destroy duplicates" protection GameManager has
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

        // this is the recursive call that does all the actual work - starts carving from (0,0)
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

            // recursive call - carve onward from the neighbor
            // when this eventually hits a dead end and returns, we fall back here
            // and try the next direction - that "falling back" IS the backtracking
            CarvePath(neighborX, neighborY);
        }
    }

    private Vector2Int[] GetShuffledDirections()
    {
        Vector2Int[] directions = { Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left };

        // simple shuffle - swap each slot with a random other slot
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

    // turns the wall data into actual GameObjects in the scene -
    // only builds top+left walls per cell (avoids placing the same interior wall twice,
    // since a cell's top wall is the same physical wall as its neighbor's bottom wall)
    // plus the outer border on the right and bottom edges of the whole grid
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
        // thin in one direction, full cellSize in the other - horizontal wall is wide+short, vertical is tall+thin
        wall.transform.localScale = horizontal
            ? new Vector3(cellSize, 0.1f, 1f)
            : new Vector3(0.1f, cellSize, 1f);
    }

    // converts a real world position into which grid cell it falls inside
    // (Floor, not Round - matches how BuildWalls treats (x*cellSize, y*cellSize) as a cell's corner,
    // so cell x actually spans world x from x*cellSize up to (x+1)*cellSize)
    public Vector2Int WorldToCell(Vector3 worldPosition)
    {
        int x = Mathf.FloorToInt(worldPosition.x / cellSize);
        int y = Mathf.FloorToInt(worldPosition.y / cellSize);
        return new Vector2Int(x, y);
    }

    // converts a grid cell back into a world position - the CENTER of that cell, not its corner
    // (this was the actual bug: returning the corner sent enemies walking straight into wall intersections)
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

    // picks any random cell in the grid - safe to use for patrol points because
    // every cell in a perfect maze is guaranteed reachable from every other cell
    public Vector2Int GetRandomCell()
    {
        return new Vector2Int(Random.Range(0, width), Random.Range(0, height));
    }
}