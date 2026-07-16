using UnityEngine;

// same patrol logic that used to live in Enemy.Update() - just moved here
// so it can be swapped out for ChaseState/AttackState later
public class PatrolState : IEnemyState
{
    public void Tick(Enemy enemy)
    {
        enemy.MoveTowardCurrentPatrolPoint();
        enemy.CheckPatrolPointSwitch();

        // this is the actual state transition - if we spot the player, hand control to ChaseState
        if (enemy.CanSeePlayer())
        {
            enemy.SetState(new ChaseState());
        }
    }
}