using UnityEngine;

// classic Singleton pattern - guarantees only one GameManager ever exists,
// and gives every other script easy access to it via GameManager.Instance
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public bool IsGameOver { get; private set; }

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
        Player player = FindAnyObjectByType<Player>();
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
        // later: show a game over UI screen here
    }

    public void PlayerWon()
    {
        if (IsGameOver) return;

        IsGameOver = true;
        Debug.Log("YOU WIN - reached the exit");
        // later: show a victory UI screen here
    }
}