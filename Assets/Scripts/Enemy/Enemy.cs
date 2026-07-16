using UnityEngine;

// enemy inherits health/damage from Character, same as Player does
// behavior itself now lives in separate state classes (Patrol/Chase/Attack) - this is the State pattern
public class Enemy : Character
{
    // drag two empty GameObjects in here in the Inspector - the enemy will walk back and forth between them
    [SerializeField] private Transform pointA;
    [SerializeField] private Transform pointB;
    private Transform currentTarget;

    [Header("Detection")]
    [SerializeField] private float detectionRange = 5f;
    [SerializeField] private LayerMask wallLayer; // set this to whatever layer your maze walls are on

    [Header("Combat")]
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private int attackDamage = 10;
    [SerializeField] private float fireRate = 1f; // seconds between hits
    private float lastAttackTime;
    public float AttackRange => attackRange;

    private Transform player;

    // this is the whole trick - enemy just holds "whatever state I'm in right now"
    // and asks it to Tick() every frame, without caring what's actually inside
    private IEnemyState currentState;

    protected override void Awake()
    {
        base.Awake(); // still sets up currentHealth from Character
        currentTarget = pointB; // start by walking toward point B
        player = GameObject.FindGameObjectWithTag("Player").transform;
        currentState = new PatrolState(); // every enemy starts out patrolling
    }

    private void Update()
    {
        currentState.Tick(this);
    }

    // states call this to switch which behavior is currently active
    public void SetState(IEnemyState newState)
    {
        currentState = newState;
    }

    // enemy's version of Move looks different from Player's -
    // instead of reading input, it's told where to go by whichever state is active
    public override void Move(Vector2 direction)
    {
        transform.Translate(direction * moveSpeed * Time.deltaTime);
    }

    // --- helpers the states call, so the states don't need to touch Enemy's private fields directly ---

    public void MoveTowardCurrentPatrolPoint()
    {
        Vector2 direction = (currentTarget.position - transform.position).normalized;
        Move(direction);
    }

    public void CheckPatrolPointSwitch()
    {
        float distance = Vector2.Distance(transform.position, currentTarget.position);
        if (distance < 0.1f)
        {
            currentTarget = currentTarget == pointA ? pointB : pointA;
        }
    }

    public void MoveTowardPlayer()
    {
        Vector2 direction = (player.position - transform.position).normalized;
        Move(direction);
    }

    public float DistanceToPlayer()
    {
        return Vector2.Distance(transform.position, player.position);
    }

    // called by AttackState - only actually deals damage if the cooldown has passed
    public void AttackPlayer()
    {
        if (Time.time - lastAttackTime < fireRate) return; // still on cooldown, do nothing

        lastAttackTime = Time.time;

        // this only works because Player also inherits from Character -
        // Enemy doesn't need to know it's specifically a "Player", just that it's damageable
        Character playerCharacter = player.GetComponent<Character>();
        playerCharacter.TakeDamage(attackDamage);
    }

    // cheap distance check first, then a raycast to confirm no wall is blocking the view
    // this is the "search" style logic we'll talk about for the algorithms section later
    public bool CanSeePlayer()
    {
        float distance = Vector2.Distance(transform.position, player.position);
        if (distance > detectionRange) return false;

        Vector2 directionToPlayer = (player.position - transform.position).normalized;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, directionToPlayer, distance, wallLayer);

        // if the ray hit a wall before reaching the player, hit.collider won't be null - vision is blocked
        return hit.collider == null;
    }
}