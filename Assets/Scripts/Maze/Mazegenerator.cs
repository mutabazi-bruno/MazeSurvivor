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

    private MazeCell[,] grid;

    private void Start()
    {
        GenerateMaze();
        BuildWalls();
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
}