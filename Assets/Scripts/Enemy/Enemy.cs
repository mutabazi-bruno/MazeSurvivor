using System.Collections.Generic;
using UnityEngine;

// enemy inherits health/damage from Character, same as Player does
// behavior itself lives in separate state classes (Patrol/Chase/Attack) this is the State pattern

public class Enemy : Character
{
    [Header("Detection")]
    [SerializeField] private float detectionRange = 5f;
    [SerializeField] private LayerMask wallLayer; 

    [Header("Combat")]
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private int attackDamage = 10;
    [SerializeField] private float fireRate = 1f; 
    private float lastAttackTime;
    public float AttackRange => attackRange;

    private Transform player;
    private Rigidbody2D rb;

    // this is the whole trick  enemy just holds "whatever state I'm in right now"
    // and asks it to Tick() every frame, without caring what's actually inside

    private IEnemyState currentState;
    private Vector2Int patrolPointA;
    private Vector2Int patrolPointB;
    private Vector2Int currentPatrolTarget;

    [Header("Pathfinding")]
    [SerializeField] private float pathRecalculateInterval = 0.5f; 
    private List<Vector2Int> currentPath;
    private int pathIndex;
    private float lastPathTime;
    private Vector2Int currentPathTargetCell;
    public int CurrentPathCount => currentPath?.Count ?? -1;

    protected override void Awake()
    {
        base.Awake(); 
        player = GameObject.FindGameObjectWithTag("Player").transform;
        currentState = new PatrolState(); 
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        
        patrolPointA = MazeGenerator.Instance.GetRandomCell();
        patrolPointB = MazeGenerator.Instance.GetRandomCell();
        currentPatrolTarget = patrolPointB;
    }

    private void Update()
    {
        currentState.Tick(this);
    }

    public void SetState(IEnemyState newState)
    {
        currentState = newState;
    }

    public override void Move(Vector2 direction)
    {
        rb.linearVelocity = direction.normalized * moveSpeed;
    }

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

    public void AttackPlayer()
    {
        if (Time.time - lastAttackTime < fireRate) return; // still on cooldown, do nothing

        lastAttackTime = Time.time;

        // this only works because Player also inherits from Character 
        // Enemy doesn't need to know it's specifically a "Player", just that it's damageable

        Character playerCharacter = player.GetComponent<Character>();
        playerCharacter.TakeDamage(attackDamage);
    }

    // cheap distance check first, then a raycast to confirm no wall is blocking the view

    public bool CanSeePlayer()
    {
        float distance = Vector2.Distance(transform.position, player.position);
        if (distance > detectionRange) return false;

        Vector2 directionToPlayer = (player.position - transform.position).normalized;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, directionToPlayer, distance, wallLayer);

        // if the ray hit a wall before reaching the player, hit.collider won't be null - vision is blocked

        return hit.collider == null;
    }

    protected override void Die()
    {
        base.Die(); 
        Destroy(gameObject);
    }
}