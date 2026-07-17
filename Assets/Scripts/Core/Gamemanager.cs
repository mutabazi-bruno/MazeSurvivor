using UnityEngine;
using UnityEngine.SceneManagement;

// classic Singleton pattern - guarantees only one GameManager ever exists,
// and gives every other script easy access to it via GameManager.Instance
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public bool IsGameOver { get; private set; }

    [Header("UI Panels - assign in Inspector")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject winPanel;

    private void Awake()
    {
        // if one already exists, destroy this duplicate - enforces "only one ever"
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        // find the player and subscribe to their death announcement -
        // Player never needed to know GameManager exists, this is GameManager reaching out instead
        Player player = FindFirstObjectByType<Player>();
        if (player != null)
        {
            player.OnDeath += HandlePlayerDeath;
        }
    }

    // this method's signature has to match Action<Character> since that's what OnDeath is
    private void HandlePlayerDeath(Character character)
    {
        PlayerDied();
    }

    public void PlayerDied()
    {
        if (IsGameOver) return; // don't trigger this twice

        IsGameOver = true;
        Debug.Log("GAME OVER - you died");

        if (gameOverPanel != null) gameOverPanel.SetActive(true);
        Time.timeScale = 0f; // freezes all physics/movement - a clean way to "pause" without extra logic
    }

    public void PlayerWon()
    {
        if (IsGameOver) return;

        IsGameOver = true;
        Debug.Log("YOU WIN - reached the exit");

        if (winPanel != null) winPanel.SetActive(true);
        Time.timeScale = 0f;
    }

    // reloading the whole scene is the simplest possible "reset everything" -
    // maze regenerates fresh, enemies respawn, health resets, all for free
    public void RestartGame()
    {
        Time.timeScale = 1f; // un-freeze before reloading, otherwise the new scene loads still paused
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}