using System.Collections.Generic;
using UnityEngine;

public static class MazePathfinder
{
    public static List<Vector2Int> FindPath(Vector2Int start, Vector2Int target, MazeGenerator maze)
    {
        // tracks which cells we've already checked, so we never process the same cell twice

        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();

        // the "ripple" cells waiting to be expanded, in the order they were discovered

        Queue<Vector2Int> frontier = new Queue<Vector2Int>();

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
                if (!maze.CanMoveBetween(current, neighbor)) continue; 

                visited.Add(neighbor);
                cameFrom[neighbor] = current;
                frontier.Enqueue(neighbor);
            }
        }

        return null; 
    }

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
            farthestCell = current; 

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