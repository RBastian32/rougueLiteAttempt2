# Lesson 4 plan: Graphs, linking rooms, and room roles

*Draft for review. Nothing here is built yet. Once the plan is agreed, it becomes the lecture
slides, the guided-notes slides, `LECTURE.md`, the lab handout (`README.md`), `HINTS.md`, the
`Solutions/` folder, and TODO stubs with EditMode tests, in the same format as Lesson 3.*

## Why this lesson, in our game

At the end of Lab 3 the BSP dungeon works, but:
- **Corridors cross the whole map and cut through rooms.** Rooms are joined in *list order*, so
  where the left half's leaves end and the right half's begin, room *i* and room *i + 1* can be far apart.
- **Every room is the same.** There's no spawn room, boss room, shop or treasure room, and no way
  to say "the boss should be **far** from the spawn".
- **There's only one route.** A chain means no loops, no shortcuts and no side branches.

All three come down to one question: *which rooms are connected to which?* The tool for that
question is a **graph**. This lesson treats the dungeon as a graph of rooms, connects it with a
**minimum spanning tree**, measures it with **breadth-first search**, and uses those measurements
to give rooms **roles**.

## Learning objectives

By the end you should be able to:
1. Define graph, vertex, edge, weight, degree, path, cycle, connected, tree.
2. Store a graph as an adjacency list, and explain when an adjacency matrix is better.
3. Use `Dictionary<K,V>`, `Queue<T>`, `HashSet<T>`, arrays and tuples in C#.
4. Explain and implement Prim's algorithm for a minimum spanning tree, and say why it never makes a cycle.
5. Add a few extra edges back to make loops, and explain the design trade-off (dead ends vs alternate routes).
6. Implement breadth-first search, and use it for distances, the farthest room and a connectivity check.
7. Assign room roles from graph measurements (distance, degree), reproducibly from the seed.
8. Visualise data in the Unity editor with Gizmos.

## Lecture units

| Unit | Topic | Key content | Used in lab |
|---|---|---|---|
| 1 | The problem | Screenshots of the Lab 3 chain; "which rooms touch?" as a question; dungeons as graphs in many roguelites | all |
| 2 | Graph vocabulary | vertex, edge, undirected/directed, weighted, degree, path, cycle, connected, tree (n − 1 edges, no cycles). *Tree leaf ≠ BSP leaf* | all |
| 3 | Representing a graph | adjacency matrix vs adjacency list (memory, speed, when to use each); an `Edge` struct; C# `List<List<int>>`, `Dictionary`, tuples `(int a, int b)` | A |
| 4 | Which rooms *could* connect? | the complete graph of room centres; Manhattan distance as the weight (why not Euclidean on a tile grid) | B |
| 5 | Minimum spanning trees | definition; why it's the "cheapest way to connect everything"; **Prim's algorithm** step by step on a 5-room example; the simple O(n²) version with arrays (priority queues come in Lesson 5); Kruskal's mentioned for comparison | B |
| 6 | Loops on purpose | why a pure tree feels like a maze of dead ends; adding back the shortest unused edges with some probability; the design trade-off | D |
| 7 | Breadth-first search | the queue, `visited`, distance layers; tracing by hand; O(V + E); BFS on the room graph *and* on the tile grid (flood fill, which your Lab 3 tests already used) | C |
| 8 | Room roles | `enum RoomType { Normal, Spawn, Boss, Treasure, Shop }`; spawn = room 0, boss = the room farthest from spawn (BFS), treasure = dead ends (degree 1), shop = a mid-distance room; staying reproducible from one `rng` | D |
| 9 | Seeing it in Unity | `OnDrawGizmos`: draw the graph's edges and colour room outlines by role in the Scene view; editor-only debug visuals | E |

Every unit gets the Lesson 3 treatment: a dark opener slide (the problem in our game, what you'll
learn, why it matters beyond the lab), then concept slides (plain words → definition → example/code →
game use → common mistake), then **LAB PREVIEW** slides with the steps, signatures and a skeleton
for each TODO, stopping short of the full solution.

## Lab exercises (TODO stubs + EditMode tests)

New files in `Assets/_Project/Scripts/Dungeon/Graph/`:

| Exercise | You write | Tests check |
|---|---|---|
| **A: RoomGraph** | `RoomGraph` class: an adjacency list over room ids. `AddEdge(a, b)` (undirected, no duplicates, no self-loops), `Neighbours(id)`, `Degree(id)`, `HasEdge(a, b)`, `EdgeCount` | both directions stored; duplicates ignored; self-loop throws |
| **B: Minimum spanning tree** | `Mst.Prim(List<Vector2Int> centres)` → a list of edges, using Manhattan distance | n − 1 edges; everything connected; a hand-made 5-point case gives the known minimum total; 1 room → 0 edges |
| **C: Breadth-first search** | `Bfs.Distances(graph, start)` → `int[]` (−1 = unreachable); `Bfs.Farthest(graph, start)` | distances on a hand-drawn graph; unreachable = −1; farthest on a chain and on a tree |
| **D: Loops and roles** | `Loops.AddExtra(graph, centres, chance, rng)`; `RoomRoles.Assign(graph, rng)` → `RoomType[]` | the graph is still connected; spawn ≠ boss; boss is at the maximum BFS distance; treasure rooms have degree 1; same seed → same roles |
| **E: In the game** | `GenerateBsp` uses MST + loops instead of the chain, and `DungeonResult` gains `Graph` and `Roles`; `MapRenderer` draws Gizmos; the player spawns in the Spawn room | the D tests still pass; every floor is reachable; play-test |

**Challenges (optional):**
1. **Corridors that avoid rooms.** Before carving an L corridor, check whether it passes through a third room; if so, try the other L shape (generate and test, from Lecture 3 Unit 1).
2. **Kruskal's algorithm.** Implement it with a union-find, and check it gives the same total weight as Prim's.
3. **Locked boss door.** Put a key in a room that's *not* on the BFS path from spawn to boss.
4. **Critical path.** Store BFS parents and highlight the shortest path from spawn to boss in the Gizmos.

## What's deliberately *not* in this lesson

- **Hand-made room layouts (prefabs/templates) stamped into rooms.** Doing this well needs
  ScriptableObjects, which is **Lesson 7 (data-driven design)**. Lesson 4 gives every room a
  *role*, and Lesson 7 can then choose a *layout* per role. (Alternative: a short "4.5" mini-lab
  that stamps layouts from text strings, if you'd rather see it sooner.)
- **Priority queues and A\***: Lesson 5 (enemy AI). The Prim's here uses the simple array
  version on purpose, so Lesson 5 can show how a priority queue speeds it up.

## Materials to build (once the plan is approved)

1. Lecture slide deck (Slides artifact), about 9 units.
2. Guided-notes deck: a copy with key words blanked out, answers in the speaker notes.
3. `LECTURE.md` reference notes, a glossary, and review problems.
4. `README.md` lab handout (pre-lab questions, editor setup, exercises A–E, challenges, reflection).
5. `HINTS.md` and `Solutions/`.
6. Stubs in `Scripts/Dungeon/Graph/` and `Tests/EditMode/GraphTests.cs`.
7. A link row in `Docs/Lessons/README.md`.
