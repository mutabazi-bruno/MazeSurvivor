using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private float speed = 12f;
    [SerializeField] private float lifetime = 1.5f; 

    private int damage;
    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        Destroy(gameObject, lifetime);
    }

   
    public void Init(Vector2 direction, int damageAmount, Collider2D ownerCollider)
    {
        damage = damageAmount;
        rb.linearVelocity = direction.normalized * speed;

        Physics2D.IgnoreCollision(GetComponent<Collider2D>(), ownerCollider);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Character target = other.GetComponent<Character>();
        if (target != null)
        {
            target.TakeDamage(damage);
            Destroy(gameObject);
            return;
        }

        if (other.gameObject.layer == LayerMask.NameToLayer("Wall"))
        {
            Destroy(gameObject);
        }
    }
}