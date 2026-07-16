using UnityEngine;

// once the enemy has spotted the player, it moves straight toward them
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

        // close enough now? switch to attacking (built in the next step)
        if (enemy.DistanceToPlayer() < enemy.AttackRange)
        {
            enemy.SetState(new AttackState());
        }
    }
}