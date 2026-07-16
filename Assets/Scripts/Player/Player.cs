using UnityEngine;

// player inherits health/damage stuff from Character for free
// only adds what's actually player-specific: reading input, shooting
public class Player : Character
{
    private Rigidbody2D rb;
    private Vector2 facingDirection = Vector2.down; // default facing, updates as you move

    [Header("Shooting")]
    [SerializeField] private int shootDamage = 20;
    [SerializeField] private float shootRange = 10f;
    [SerializeField] private LayerMask enemyLayer; // set this to whatever layer your enemies are on

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

        if (Input.GetKeyDown(KeyCode.Space))
        {
            Shoot();
        }
    }

    // this is where polymorphism kicks in - Character's Move() was empty,
    // Player actually implements it using physics movement
    public override void Move(Vector2 direction)
    {
        rb.linearVelocity = direction.normalized * moveSpeed;

        // remember which way we were last actually moving, so Shoot() knows which direction to fire
        if (direction.sqrMagnitude > 0.01f)
        {
            facingDirection = direction.normalized;
        }
    }

    // fires an instant raycast in whichever direction the player is facing -
    // reuses the exact same TakeDamage() every Character already has, no new damage system needed
    private void Shoot()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, facingDirection, shootRange, enemyLayer);

        if (hit.collider != null)
        {
            Character target = hit.collider.GetComponent<Character>();
            if (target != null)
            {
                target.TakeDamage(shootDamage);
            }
        }
    }

    // player-only behavior, doesn't exist on the base Character
    protected override void Die()
    {
        base.Die(); // still marks IsDead, logs, and fires OnDeath - GameManager hears about it through that event, not a direct call
    }
}