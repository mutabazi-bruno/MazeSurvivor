using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private int enemyCount = 5;

    private void Start()
    {
       
        for (int i = 0; i < enemyCount; i++)
        {
            Vector2Int cell = MazeGenerator.Instance.GetRandomCell();
            Vector3 spawnPosition = MazeGenerator.Instance.CellToWorld(cell);
            Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
        }
    }
}