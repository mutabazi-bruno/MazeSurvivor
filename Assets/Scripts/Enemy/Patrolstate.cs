using UnityEngine;

// patrol now walks a real pathfinding route between two randomly chosen maze cells
public class PatrolState : IEnemyState
{
    public void Tick(Enemy enemy)
    {
        enemy.MoveTowardPatrolTarget();

        // this is the actual state transition - if we spot the player, hand control to ChaseState
        if (enemy.CanSeePlayer())
        {
            enemy.SetState(new ChaseState());
        }
    }
}