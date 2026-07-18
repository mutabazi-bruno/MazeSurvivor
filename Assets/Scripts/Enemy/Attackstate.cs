using UnityEngine;

// enemy is in range and attacking calls Enemy's AttackPlayer, which handles its own cooldown
public class AttackState : IEnemyState
{
    public void Tick(Enemy enemy)
    {
        enemy.AttackPlayer();

        // if the player gets away, OR a wall ends up between , go back to chasing

        if (enemy.DistanceToPlayer() > enemy.AttackRange || !enemy.CanSeePlayer())
        {
            enemy.SetState(new ChaseState());
        }
    }
}