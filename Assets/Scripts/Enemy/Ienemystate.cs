// the contract every enemy state has to follow
// this is what makes the State pattern work - Enemy doesn't need to know
// what kind of state it's holding, just that it can Tick()
public interface IEnemyState
{
    void Tick(Enemy enemy);
}