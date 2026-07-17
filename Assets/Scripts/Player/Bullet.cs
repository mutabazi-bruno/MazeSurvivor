using UnityEngine;

// a real flying projectile, instead of an instant invisible raycast -
// naturally respects walls too, since it just physically stops when it hits one
public class Bullet : MonoBehaviour
{
    [SerializeField] private float speed = 12f;
    [SerializeField] private float lifetime = 1.5f; // safety cleanup in case it never hits anything

    private int damage;
    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        Destroy(gameObject, lifetime);
    }

    // called right after Instantiate - sets the bullet flying and tells it how much damage to deal
    public void Init(Vector2 direction, int damageAmount, Collider2D ownerCollider)
    {
        damage = damageAmount;
        rb.linearVelocity = direction.normalized * speed;

        // without this, the bullet would instantly "hit" whoever fired it, since it spawns overlapping them
        Physics2D.IgnoreCollision(GetComponent<Collider2D>(), ownerCollider);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"bullet hit: {other.gameObject.name}, layer: {LayerMask.LayerToName(other.gameObject.layer)}"); // TEMPORARY debug

        Character target = other.GetComponent<Character>();
        if (target != null)
        {
            target.TakeDamage(damage);
            Destroy(gameObject);
            return;
        }

        // hit something that isn't a Character (a wall) - just stop here, no damage
        if (other.gameObject.layer == LayerMask.NameToLayer("Wall"))
        {
            Destroy(gameObject);
        }
    }
}