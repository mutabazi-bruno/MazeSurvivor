using System.Collections.Generic;
using UnityEngine;

// enemy inherits health/damage from Character, same as Player does
// behavior itself lives in separate state classes (Patrol/Chase/Attack) - this is the State pattern
public class Enemy : Character
{
    [Header("Detection")]
    [SerializeField] private float detectionRange = 5f;
    [SerializeField] private LayerMask wallLayer; // set this to whatever layer your maze walls are on

    [Header("Combat")]
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private int attackDamage = 10;
    [SerializeField] private float fireRate = 1f; // seconds between hits
    private float lastAttackTime;
    public float AttackRange => attackRange;

    private Transform player;
    private Rigidbody2D rb;

    // this is the whole trick - enemy just holds "whatever state I'm in right now"
    // and asks it to Tick() every frame, without caring what's actually inside
    private IEnemyState currentState;

    // --- patrol points are now picked automatically as random maze cells, ---
    // --- not manually placed Transforms - guaranteed reachable since it's a perfect maze ---
    private Vector2Int patrolPointA;
    private Vector2Int patrolPointB;
    private Vector2Int currentPatrolTarget;

    // --- shared pathfinding state, used by BOTH patrol and chase now ---
    [Header("Pathfinding")]
    [SerializeField] private float pathRecalculateInterval = 0.5f; // don't recompute every single frame, that's wasteful
    private List<Vector2Int> currentPath;
    private int pathIndex;
    private float lastPathTime;
    private Vector2Int currentPathTargetCell;
    public int CurrentPathCount => currentPath?.Count ?? -1;

    protected override void Awake()
    {
        base.Awake(); // still sets up currentHealth from Character
        player = GameObject.FindGameObjectWithTag("Player").transform;
        currentState = new PatrolState(); // every enemy starts out patrolling
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        // done in Start (not Awake) so we're guaranteed MazeGenerator has already set its Instance
        patrolPointA = MazeGenerator.Instance.GetRandomCell();
        patrolPointB = MazeGenerator.Instance.GetRandomCell();
        currentPatrolTarget = patrolPointB;
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
    // uses physics (like Player does) so it actually respects wall colliders
    public override void Move(Vector2 direction)
    {
        rb.linearVelocity = direction.normalized * moveSpeed;
    }

    // --- helpers the states call ---

    // patrol now walks a REAL path to a random cell, and once there, swaps to the other patrol cell
    public void MoveTowardPatrolTarget()
    {
        FollowPathTo(currentPatrolTarget);

        Vector2Int myCell = MazeGenerator.Instance.WorldToCell(transform.position);
        if (myCell == currentPatrolTarget)
        {
            currentPatrolTarget = currentPatrolTarget == patrolPointA ? patrolPointB : patrolPointA;
        }
    }

    // chase walks a real path straight to whatever cell the player is currently in
    public void MoveTowardPlayer()
    {
        Vector2Int playerCell = MazeGenerator.Instance.WorldToCell(player.position);
        FollowPathTo(playerCell);
    }

    // shared logic - both patrol and chase funnel through here now, so there's only one
    // path-following implementation to maintain instead of two separate ones
    private void FollowPathTo(Vector2Int targetCell)
    {
        bool needsNewPath = currentPath == null
            || targetCell != currentPathTargetCell
            || Time.time - lastPathTime > pathRecalculateInterval;

        if (needsNewPath)
        {
            currentPathTargetCell = targetCell;
            RecalculatePath(targetCell);
        }

        if (currentPath == null || currentPath.Count == 0) return;

        Vector3 targetWorldPos = MazeGenerator.Instance.CellToWorld(currentPath[pathIndex]);
        Vector2 direction = (targetWorldPos - transform.position).normalized;
        Move(direction);

        // close enough to this waypoint? move on to the next one
        // (slightly generous threshold - too tight and a wall corner can pin the enemy in place)
        if (Vector2.Distance(transform.position, targetWorldPos) < 0.35f && pathIndex < currentPath.Count - 1)
        {
            pathIndex++;
        }
    }

    private void RecalculatePath(Vector2Int targetCell)
    {
        lastPathTime = Time.time;
        Vector2Int myCell = MazeGenerator.Instance.WorldToCell(transform.position);
        currentPath = MazePathfinder.FindPath(myCell, targetCell, MazeGenerator.Instance);
        pathIndex = 0;
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

    // enemy-specific death behavior - disappears from the scene entirely
    // (Player overrides this differently - it stays visible for the game over screen instead)
    protected override void Die()
    {
        base.Die(); // still marks IsDead, logs, fires OnDeath
        Destroy(gameObject);
    }
}