using System;
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

    // Observer pattern - Character just announces death, doesn't know or care who's listening
    // (GameManager will subscribe to this without Character ever needing a reference to it)
    public event Action<Character> OnDeath;

    protected virtual void Awake()
    {
        currentHealth = maxHealth;
    }

    // any character can take damage this way, no matter if it's player or enemy
    public virtual void TakeDamage(int amount)
    {
        if (IsDead) return;

        currentHealth -= amount;
        Debug.Log($"{gameObject.name} took {amount} damage, health now {currentHealth}"); // TEMPORARY - remove once you have a health UI

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
        OnDeath?.Invoke(this); // announce death - anyone subscribed will hear about it
    }

    // left empty on purpose - player and enemy move very differently
    // (player reads input, enemy follows a path) so they'll each write their own version
    public virtual void Move(Vector2 direction)
    {
        // overridden by subclasses
    }
}