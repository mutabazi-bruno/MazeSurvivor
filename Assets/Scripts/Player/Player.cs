using UnityEngine;

// player inherits health/damage stuff from Character for free only adds what's actually player-specific: reading input, shooting

public class Player : Character
{
    private Rigidbody2D rb;
    private Vector2 facingDirection = Vector2.down;

    [Header("Shooting")]
    [SerializeField] private int shootDamage = 20;
    [SerializeField] private GameObject bulletPrefab;
    private Collider2D myCollider;

    protected override void Awake()
    {
        base.Awake(); 
        rb = GetComponent<Rigidbody2D>();
        myCollider = GetComponent<Collider2D>();
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

    // this is where polymorphism kicks in - Character's Move() was empty, Player actually implements it using physics movement
  
    public override void Move(Vector2 direction)
    {
        rb.linearVelocity = direction.normalized * moveSpeed;

        if (direction.sqrMagnitude > 0.01f)
        {
            facingDirection = direction.normalized;
            RotateTowardFacing();
        }
    }

    // points the sprite toward whichever direction I'M currently moving
    private void RotateTowardFacing()
    {
        float angle = Mathf.Atan2(facingDirection.y, facingDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle - 90f);
    }

    // spawns a real bullet that flies off in the direction I'M facing
    private void Shoot()
    {
        GameObject bulletObject = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
        Bullet bullet = bulletObject.GetComponent<Bullet>();
        bullet.Init(facingDirection, shootDamage, myCollider);
    }

    // player-only behavior, doesn't exist on the base Character
    protected override void Die()
    {
        base.Die(); 
    }
}