# Lessons

This folder is your course notes. The **code** you write lives in the real game folders
(`Assets/_Project/Scripts/...`), because every exercise builds a piece of the actual game.

## How each lesson works

1. **Read the lesson's `README.md`.** It explains the concepts before you touch code.
2. **Do the editor setup** described there (creating objects, tiles, etc. in Unity).
3. **Find the exercises.** Each one is a `TODO` comment with an ID like `TODO B3`.
   In VS Code: `Cmd+Shift+F` and search `TODO B` to see every step of exercise B.
4. **Write the code yourself.** The comments tell you *what* to do, not *how*.
5. **Check your work:**
   - **Tests** (when an exercise has them): in Unity, open
     `Window > General > Test Runner`, choose the **EditMode** tab, click **Run All**.
     Red ❌ = not done yet or a bug; green ✅ = correct. Click a failing test to read why.
   - **Play mode**: press Play and see whether it behaves the way the lesson says.
6. **Stuck?** Open `HINTS.md` first. Each hint gives a nudge, not the answer.
7. **Really stuck?** `Solutions/` has a full answer. Try for at least 15 minutes first:
   struggling is where the learning happens.
8. **Commit** when it works (`git add -A`, `git commit -m "..."`, `git push`).
9. **Challenges** at the end of each lesson are optional extras with no solutions.
   They're the best practice of all, because you have to design the code yourself.

## Playing around

`Assets/_Project/Sandbox/` is your scratch space. Put throwaway scripts and test
scenes there to try ideas without breaking the real game.

## Lessons

| # | Topic | Status |
|---|---|---|
| 1 | Player movement: MonoBehaviour lifecycle, physics, vectors | ✅ (`Scripts/Player/PlayerMovement.cs`) |
| 2 | [Camera, grids, and Tilemaps](02-grid-and-tilemap/README.md) | 👉 current |
| 3 | Dungeon generation: seeded randomness, random walk, BSP | |
| 4 | Linking rooms: graphs, minimum spanning trees, BFS | |
| 5 | Enemies: state machines, A* pathfinding, priority queues | |
| 6 | Combat: events, delegates, interfaces | |
| 7 | Data-driven design: ScriptableObjects | |
| 8 | Loot: weighted random tables, stat modifiers | |
| 9 | Performance: object pooling | |
| 10 | Game loop and saving: state manager, JSON | |
