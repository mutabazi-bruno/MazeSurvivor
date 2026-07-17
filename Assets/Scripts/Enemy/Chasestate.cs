using UnityEngine;

// once the enemy has spotted the player, it moves toward them by following the pathfinding route
public class ChaseState : IEnemyState
{
    public void Tick(Enemy enemy)
    {
        enemy.MoveTowardPlayer();

        // lost sight of the player? go back to patrolling
        if (!enemy.CanSeePlayer())
        {
            enemy.SetState(new PatrolState());
            return;
        }

        // close enough AND a clear line of sight (not blocked by a wall) - only then attack
        if (enemy.DistanceToPlayer() < enemy.AttackRange && enemy.CanSeePlayer())
        {
            enemy.SetState(new AttackState());
        }
    }
}