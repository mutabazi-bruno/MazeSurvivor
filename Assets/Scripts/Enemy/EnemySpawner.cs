using UnityEngine;

// spawns a set number of enemy prefabs at random cells in the maze -
// safe because every cell in a perfect maze is reachable, so no enemy can ever spawn "trapped"
public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private int enemyCount = 5;

    private void Start()
    {
        // waits a frame isn't needed here since MazeGenerator.Instance is set in ITS Awake,
        // and this Start() runs after all Awakes - same safe timing trick Enemy uses for patrol points
        for (int i = 0; i < enemyCount; i++)
        {
            Vector2Int cell = MazeGenerator.Instance.GetRandomCell();
            Vector3 spawnPosition = MazeGenerator.Instance.CellToWorld(cell);
            Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
        }
    }
}