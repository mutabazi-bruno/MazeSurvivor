using System.Collections.Generic;
using UnityEngine;

// finds the shortest path between two cells using BFS (Breadth-First Search):
// expand outward one "ring" of cells at a time from the start, stop the instant
// we reach the target, then trace backward through how we got there.
// guaranteed shortest path here because every move costs the same (one cell to an adjacent cell).
public static class MazePathfinder
{
    public static List<Vector2Int> FindPath(Vector2Int start, Vector2Int target, MazeGenerator maze)
    {
        // tracks which cells we've already checked, so we never process the same cell twice
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();

        // the "ripple" - cells waiting to be expanded, in the order they were discovered
        Queue<Vector2Int> frontier = new Queue<Vector2Int>();

        // remembers "I reached this cell FROM this other cell" - how we trace the path backward at the end
        Dictionary<Vector2Int, Vector2Int> cameFrom = new Dictionary<Vector2Int, Vector2Int>();

        frontier.Enqueue(start);
        visited.Add(start);

        Vector2Int[] directions = { Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left };

        while (frontier.Count > 0)
        {
            Vector2Int current = frontier.Dequeue();

            if (current == target)
            {
                return ReconstructPath(cameFrom, start, target);
            }

            foreach (Vector2Int direction in directions)
            {
                Vector2Int neighbor = current + direction;

                if (visited.Contains(neighbor)) continue;
                if (!maze.CanMoveBetween(current, neighbor)) continue; // wall blocks this direction

                visited.Add(neighbor);
                cameFrom[neighbor] = current;
                frontier.Enqueue(neighbor);
            }
        }

        return null; // no path exists (shouldn't happen in our maze, since every cell is reachable)
    }

    // reuses the same BFS spread, but instead of stopping at a target, it explores the WHOLE maze
    // from the start point and remembers whichever cell was hardest to reach (took the most steps) -
    // perfect for picking an exit location that's genuinely far from the spawn point
    public static Vector2Int FindFarthestCell(Vector2Int start, MazeGenerator maze)
    {
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();
        Queue<Vector2Int> frontier = new Queue<Vector2Int>();
        Vector2Int[] directions = { Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left };

        frontier.Enqueue(start);
        visited.Add(start);

        Vector2Int farthestCell = start;

        while (frontier.Count > 0)
        {
            Vector2Int current = frontier.Dequeue();
            farthestCell = current; // BFS visits in increasing distance order, so the last one dequeued is farthest

            foreach (Vector2Int direction in directions)
            {
                Vector2Int neighbor = current + direction;
                if (visited.Contains(neighbor)) continue;
                if (!maze.CanMoveBetween(current, neighbor)) continue;

                visited.Add(neighbor);
                frontier.Enqueue(neighbor);
            }
        }

        return farthestCell;
    }

    // walks backward from target to start using the cameFrom trail, then reverses it
    // so the returned list reads start -> ... -> target
    private static List<Vector2Int> ReconstructPath(Dictionary<Vector2Int, Vector2Int> cameFrom, Vector2Int start, Vector2Int target)
    {
        List<Vector2Int> path = new List<Vector2Int>();
        Vector2Int current = target;

        while (current != start)
        {
            path.Add(current);
            current = cameFrom[current];
        }

        path.Reverse();
        return path;
    }
}