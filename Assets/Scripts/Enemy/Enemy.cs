using UnityEngine;

// enemy inherits health/damage from Character, same as Player does
// right now this does basically nothing extra - we'll build up its behavior in layers
public class Enemy : Character
{
    // drag two empty GameObjects in here in the Inspector - the enemy will walk back and forth between them
    [SerializeField] private Transform pointA;
    [SerializeField] private Transform pointB;
    private Transform currentTarget;

    protected override void Awake()
    {
        base.Awake(); // still sets up currentHealth from Character
        currentTarget = pointB; // start by walking toward point B
    }

    private void Update()
    {
        // figure out which direction gets us closer to the current target point
        Vector2 direction = (currentTarget.position - transform.position).normalized;
        Move(direction);

        // close enough to the target? switch to the other point
        float distance = Vector2.Distance(transform.position, currentTarget.position);
        if (distance < 0.1f)
        {
            currentTarget = currentTarget == pointA ? pointB : pointA;
        }
    }

    // enemy's version of Move will look completely different from Player's once we're done -
    // instead of reading input, it'll be told where to go by its current state (patrol/chase/attack)
    public override void Move(Vector2 direction)
    {
        transform.Translate(direction * moveSpeed * Time.deltaTime);
    }
}