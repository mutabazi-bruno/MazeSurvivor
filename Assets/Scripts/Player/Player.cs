using UnityEngine;

// player inherits health/damage stuff from Character for free
// only adds what's actually player-specific: reading input, shooting
public class Player : Character
{
    private Rigidbody2D rb;

    protected override void Awake()
    {
        base.Awake(); // still runs Character's Awake (sets currentHealth) before adding our own stuff
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        // grab raw keyboard/joystick input, feed it into Move()
        Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        Move(input);
    }

    // this is where polymorphism kicks in - Character's Move() was empty,
    // Player actually implements it using physics movement
    public override void Move(Vector2 direction)
    {
        rb.linearVelocity = direction.normalized * moveSpeed;
    }

    // player-only behavior, doesn't exist on the base Character
    protected override void Die()
    {
        base.Die(); // still marks IsDead, logs, and fires OnDeath - GameManager hears about it through that event, not a direct call
    }
}