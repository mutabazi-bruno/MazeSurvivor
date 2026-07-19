# Maze Survivor

A top-down maze game built in Unity for my OOP, Design Patterns & Algorithms assignment. You spawn at one end of a procedurally generated maze, and you need to reach the exit while dealing with enemies that patrol, chase, and attack if they spot you.

Every maze is different — it's generated fresh each time you play, and enemies actually navigate through it (they don't cheat or walk through walls).

## How to play

- **WASD / Arrow Keys** — move
- **Space** — shoot
- Reach the green exit tile to win. Run out of health and it's game over.
- Both screens give you a Restart button, no need to relaunch the game.

## What's actually going on under the hood

This project was built around three things: solid OOP structure, three real design patterns, and two algorithms that directly affect gameplay (not just decoration).

**OOP principles**
- `Character` is the shared base class for both `Player` and `Enemy` — health, damage, death all live here once instead of being duplicated
- Health is encapsulated behind `TakeDamage()`, nothing outside the class can just set your HP directly
- `Move()` and `Die()` are both `virtual`, and `Player`/`Enemy` each override them differently — same method call, different behavior depending on which one you're actually talking to

**Design patterns**
- **State** — enemy AI runs on `PatrolState` → `ChaseState` → `AttackState`, all implementing one shared `IEnemyState` interface. No giant if/else block, each state decides on its own when to hand off to the next one.
- **Singleton** — `GameManager` and `MazeGenerator` both guarantee only one instance exists, so anything in the game can reach them (`GameManager.Instance`) without needing a manually dragged reference.
- **Observer** — `Character` fires `OnDeath` and `OnHealthChanged` events. `GameManager` and the health bar UI both listen for these independently. `Character` has zero idea either of them exist.

**Algorithms**
- **Recursive Backtracking** builds the maze itself — guarantees a "perfect maze" (exactly one path between any two points, no loops, everything reachable). Lives in `MazeGenerator.CarvePath()`.
- **BFS (Breadth-First Search)** handles enemy pathfinding, so enemies actually navigate the maze's corridors instead of walking in a straight line at you. Lives in `MazePathfinder.FindPath()`. Also reused (in `FindFarthestCell()`) to place the exit as far from spawn as possible.

## Project structure

```
Assets/
  Scripts/
    Core/       — Character (base class), GameManager (Singleton)
    Player/     — Player, Bullet
    Enemy/      — Enemy, IEnemyState + the 3 state classes, EnemySpawner
    Maze/       — MazeGenerator, MazeCell, MazePathfinder, ExitTile
    UI/         — HealthBarUI, MainMenuManager
  Prefabs/
  Scenes/       — MainMenu, MainScene
  Sprites/
```

## Setup

1. Clone or download this repo
2. Open the project folder in Unity (built on Unity 6)
3. Open `Scenes/MainMenu`, hit Play, click Play in-game to load into `MainScene`

## Credits

Character and enemy sprites are from a free asset pack (not made by me). Background music sourced from Pixabay (royalty-free). All code, maze generation, pathfinding, and game logic is my own work.