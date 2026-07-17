using UnityEngine;

// sits on the exit cell as a trigger collider - the moment the Player touches it, you win
public class ExitTile : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            GameManager.Instance.PlayerWon();
        }
    }
}