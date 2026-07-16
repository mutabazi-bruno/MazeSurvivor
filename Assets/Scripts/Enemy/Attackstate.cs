using UnityEngine;

// enemy is in range and attacking - calls Enemy's AttackPlayer, which handles its own cooldown
public class AttackState : IEnemyState
{
    public void Tick(Enemy enemy)
    {
        enemy.AttackPlayer();

        // if the player somehow gets away, go back to chasing
        if (enemy.DistanceToPlayer() > enemy.AttackRange)
        {
            enemy.SetState(new ChaseState());
        }
    }
}