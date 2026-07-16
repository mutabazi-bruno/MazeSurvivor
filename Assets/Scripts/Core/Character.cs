using UnityEngine;

// base class for anything that moves and can take damage (player + enemies)
public class Character : MonoBehaviour
{
    [SerializeField] protected int maxHealth = 100;
    protected int currentHealth;

    [SerializeField] protected float moveSpeed = 3f;

    // basically the same thing you built before with HealthManager
    // keeping health private-ish (only touched through methods) is what encapsulation means here
    public int CurrentHealth => currentHealth;
    public bool IsDead { get; protected set; }

    protected virtual void Awake()
    {
        currentHealth = maxHealth;
    }

    // any character can take damage this way, no matter if it's player or enemy
    public virtual void TakeDamage(int amount)
    {
        if (IsDead) return;

        currentHealth -= amount;

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
        }
    }

    // player and enemy can override this later if they need custom death behavior
    // (player might trigger a game over screen, enemy might just disappear)
    protected virtual void Die()
    {
        IsDead = true;
        Debug.Log($"{gameObject.name} died");
    }

    // left empty on purpose - player and enemy move very differently
    // (player reads input, enemy follows a path) so they'll each write their own version
    public virtual void Move(Vector2 direction)
    {
        // overridden by subclasses
    }
}