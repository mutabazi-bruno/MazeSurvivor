using System;
using UnityEngine;

// base class for anything that moves and can take damage (player + enemies)
public class Character : MonoBehaviour
{
    [SerializeField] protected int maxHealth = 100;
    protected int currentHealth;

    [SerializeField] protected float moveSpeed = 3f;


    public int CurrentHealth => currentHealth;
    public bool IsDead { get; protected set; }

    public event Action<Character> OnDeath;

    public event Action<int, int> OnHealthChanged; 

    protected virtual void Awake()
    {
        currentHealth = maxHealth;
    }

    // any character can take damage this way, no matter if it's player or enemy

    public virtual void TakeDamage(int amount)
    {
        if (IsDead) return;

        currentHealth -= amount;
        OnHealthChanged?.Invoke(currentHealth, maxHealth); 

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
        }
    }

    protected virtual void Die()
    {
        IsDead = true;
        Debug.Log($"{gameObject.name} died");
        OnDeath?.Invoke(this); 
    }

    public virtual void Move(Vector2 direction)
    {
        // overridden by subclasses
    }
}