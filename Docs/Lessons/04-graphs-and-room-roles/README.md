# Lab 4: Graphs, linking rooms, and room roles

**Pre-lab:** work through the [Lecture 4 slides](https://claude.ai/artifact/M6j2crh4Mu2RPWz2jjYyCp) while filling in the
[guided-notes slides](https://claude.ai/artifact/3ozzBuNVeEKFvPo9diS2Rh). The **LAB PREVIEW** slides at the end of each unit show the
plan for each TODO. [LECTURE.md](LECTURE.md) is the written reference (§ = its sections).

**Deliverable:** press Play and get a BSP dungeon whose rooms are joined by a minimum spanning
tree plus a few loops, with every room given a role, and the graph and roles drawn as Gizmos in
the Scene view. All 33 Lesson 4 tests green, plus your 35 Lesson 3 and 18 Lesson 2 tests.

| Exercise | File | Lecture units | Checked by |
|---|---|---|---|
| A. The room graph | `Scripts/Dungeon/Graph/RoomGraph.cs` | 2, 3 | 8 tests (`A…`) |
| B. Minimum spanning tree | `Scripts/Dungeon/Graph/SpanningTree.cs` | 4, 5 | 5 tests (`B…`) |
| C. Breadth-first search | `Scripts/Dungeon/Graph/Bfs.cs` | 7 | 7 tests (`C…`) |
| D. Loops and roles | `Scripts/Dungeon/Graph/Loops.cs`, `RoomRoles.cs` | 6, 8 | 10 tests (`D…`) |
| E. In the game | `Generation/DungeonGenerator.cs`, `MapRenderer.cs` | 9 | 3 tests (`E1…`) + Play mode |

Do them **in order**: B and D use A, D uses C, and E uses everything.

One test starts green: `A1_NegativeRoomCount_Throws`. It tests a guard clause that's already
written for you. Everything else starts red. **Your Lesson 3 tests must stay green throughout.**

**Before you start:** pull the latest code (`git pull`) and let Unity import it. Check the
Console has no red errors, and the Test Runner lists a new `GraphTests` group.

---

## Exercise A: The room graph
**Concepts:** vertices, edges, degree (§2), adjacency list (§3.1–3.2), `List<List<int>>`, `IReadOnlyList` (§3.3–3.4).

**Pre-lab questions**
1. A tree has 6 rooms. How many edges? How many entries in total across all the adjacency lists?
2. Why does `AddEdge(2, 5)` put something in **two** lists?
3. Why does `Neighbours` return `IReadOnlyList<int>` instead of `List<int>`?

**Tasks:** TODO A1 (constructor), A2 (`HasEdge`), A3 (`AddEdge`), A4 (`Neighbours`, `Degree`).

**Done when:** all 8 `A…` tests are green.

---

## Exercise B: Minimum spanning tree
**Concepts:** Manhattan distance (§4.1), spanning trees (§5), Prim's algorithm (§5.1–5.3), arrays and `int.MaxValue` (§3.3).

**Pre-lab questions**
1. Distance between (2, 9) and (7, 3)?
2. Run Prim's by hand on the lecture's 5 rooms (§5.3) without looking. Which edge is added at each step?
3. What are `bestCost` and `bestFrom` for? Why does `bestCost[0]` start at 0?
4. Why use `<` and not `<=` when picking the cheapest room?

**Tasks:** TODO B1 (`Distance`), then B2 (set up), B3 (pick), B4 (add and update) in `Prim`.

**Tip:** if `B4_Prim_LectureExample` fails, print the edges (`Debug.Log` each pair from
`tree.Edges()`) and compare with the table in §5.3, step by step.

**Done when:** all 5 `B…` tests are green.

---

## Exercise C: Breadth-first search
**Concepts:** BFS (§7.1–7.2), `Queue<T>` (§3.3), farthest room (§7.4).

**Pre-lab questions**
1. Do the BFS trace in §7.3 yourself, on paper, with the queue written out at each step.
2. Why does `distance = −1` work as the "visited" marker?
3. What are the BFS distances from room 0 in the loop 0–1–2–3–4–0?

**Tasks:** TODO C1 (set up), C2 (the loop), C3 (`Farthest`).

**Done when:** all 7 `C…` tests are green.

---

## Exercise D: Loops and roles
**Concepts:** loops (§6), `rng.NextDouble()` (§6), room roles (§8), parallel arrays (§8.2).

**Pre-lab questions**
1. With `maxDistance = 9` and `chance = 1.0`, which extra edges are added to the lecture's MST? (§5.3 has all the lengths.)
2. Why loop with `b` starting at `a + 1`?
3. In the tree from §7.3 with spawn 0, which room is the boss? Which are treasure? Which could be the shop?
4. Why must the Treasure step skip rooms that already have a role?

**Tasks:** TODO D1 in `Loops.cs`; TODO D2 (spawn and boss), D3 (treasure), D4 (shop) in `RoomRoles.cs`.

**Done when:** all 10 `D…` tests are green.

---

## Exercise E: In the game
**Concepts:** the new pipeline (§9.1), Gizmos (§9.2).

### E1: The generator
In `DungeonGenerator.cs`, `DungeonResult` already has the new `Graph` and `Roles` fields, and
`GenerateBsp` has two new optional parameters (`loopMaxDistance`, `loopChance`). Follow **TODO E1**
in `GenerateBsp`: delete your Lesson 3 chain loop (D3) and replace it with the graph version.

**Done when:** the 3 `E1…` tests are green **and all 7 Lesson 3 `D…` tests are still green.**

### E2: Gizmos
There's no stub for this one: **you** add the code to your own `MapRenderer.cs`. The LAB PREVIEW
slides in Unit 9 show the syntax.

1. **A field** next to `Grid`: `private DungeonResult lastBsp;` It remembers the last BSP dungeon.
2. **Set it in `Build()`.** Right after the null check, set `lastBsp = null;` (so other generators
   don't draw an old dungeon). In the **Bsp** case of your switch, after `GenerateBsp`, set
   `lastBsp = result;`
3. **`ColorFor(RoomType role)`**: a `private static Color` method with a `switch` that returns a
   colour per role: Spawn green, Boss red, Treasure yellow, Shop cyan, anything else white.
   (`Color.green`, `Color.red`, …)
4. **`OnDrawGizmos()`**: a `private void` method. Return early if `lastBsp`, its `Graph` or its
   `Roles` is null. Then:
   - **Edges:** set `Gizmos.color = Color.white`, and for each `(a, b)` in `lastBsp.Graph.Edges()`,
     `Gizmos.DrawLine` between the world centres of room a and room b.
     World centre of a cell: `floorTilemap.GetCellCenterWorld(new Vector3Int(c.x, c.y, 0))`.
   - **Rooms:** for each room i, set `Gizmos.color = ColorFor(lastBsp.Roles[i])` and draw its outline:
     ```csharp
     Vector3 corner = floorTilemap.CellToWorld(new Vector3Int(room.xMin, room.yMin, 0));
     Vector3 size = new Vector3(room.width * floorTilemap.cellSize.x, room.height * floorTilemap.cellSize.y, 0f);
     Gizmos.DrawWireCube(corner + size / 2f, size);
     ```
     (`DrawWireCube` takes the box's **centre**, so add half the size to the corner.)

**Editor check**
1. Select **Map**, set Generator = Bsp, and use **⋮ > Build Map**. In the **Scene** view you should
   see white lines joining room centres and coloured room outlines.
2. In the Game view, turn on the **Gizmos** button (top right) to see them while playing too.
3. Try a few seeds. The white lines should be short, joining neighbouring rooms, with an
   occasional loop.

**Done when:**
- No corridor crosses the whole map any more.
- The green room is where you spawn; the red room is far away.
- Yellow rooms are dead ends, and there's one cyan room.
- Typing a seed back in gives the same dungeon **and** the same colours.
- All tests green. Commit and push. 🎉

**Reflection** (a sentence or two in your notes)
- Set `loopChance` to 0, then to 1. How does the dungeon feel different to walk around?
- The boss is the farthest room **in corridors**, not in tiles. When could those two disagree? Which one matters more to a player?

---

## Challenges (optional, no solutions)
1. **Corridors that avoid rooms.** Before carving an L corridor, check whether either L shape passes through a third room's `RectInt` (use `room.Contains(cell)`). Use the shape that doesn't; if both do, carve it anyway. (Generate and test, from Lecture 3 Unit 1.)
2. **Kruskal's algorithm.** Sort all pairs by distance and add each unless it makes a cycle. Detect cycles with a union-find (look it up: two arrays, `Find` and `Union`). Check it gives the same total as Prim.
3. **Critical path.** Make BFS also record `parent[n] = room` when it reaches `n`. Walk back from the boss to the spawn, and draw that path in a different Gizmo colour.
4. **Locked boss door.** Put a key in a Treasure room that's **not** on the critical path, so the player has to explore a side branch.
5. **Distance labels.** Use `UnityEditor.Handles.Label(position, text)` inside `#if UNITY_EDITOR` to print each room's BFS distance in the Scene view.
6. **Fewer dead ends.** Make `Loops.AddExtra` prefer edges that touch a dead end, so there are fewer of them but the remaining ones are special.

---

**Stuck?** Try [HINTS.md](HINTS.md) first. Full answers are in `Solutions/`.
