# Lecture 4: Graphs, Linking Rooms, and Room Roles

**Companion slides:** [lecture slides](https://claude.ai/artifact/M6j2crh4Mu2RPWz2jjYyCp) · [guided notes](https://claude.ai/artifact/3ozzBuNVeEKFvPo9diS2Rh).
This file is the written reference: the same material, for looking things up.
**Lab:** [README.md](README.md). Section numbers (§) here match the lab handout.

## Learning objectives
By the end of this lecture you should be able to:
1. Define graph, vertex, edge, weight, degree, path, cycle, connected and tree.
2. Store a graph as an adjacency list, and explain when an adjacency matrix is better.
3. Use the C# tools for graphs: `List<List<int>>`, arrays, `Queue<T>`, tuples, `IReadOnlyList<T>`.
4. Explain what a minimum spanning tree is, and implement Prim's algorithm.
5. Add a few extra edges to make loops, and explain the design trade-off.
6. Implement breadth-first search, and use it to find distances and the farthest room.
7. Give rooms roles from graph measurements, reproducibly from one seed.
8. Draw debug information in the Unity editor with Gizmos.

## Lecture map
| Unit | Topic | Used in lab |
|---|---|---|
| 1 | The problem: which rooms connect? | all |
| 2 | Graph vocabulary | all |
| 3 | Representing a graph in code | A1–A4 |
| 4 | Which rooms *could* connect? | B1 |
| 5 | Minimum spanning trees and Prim's algorithm | B2–B4 |
| 6 | Loops on purpose | D1 |
| 7 | Breadth-first search | C1–C3 |
| 8 | Room roles | D2–D4 |
| 9 | Putting it in the game, and Gizmos | E1–E2 |

---

# Unit 1: The problem

At the end of Lab 3, `GenerateBsp` joins room 0 to room 1, room 1 to room 2, and so on: a
**chain**. It always connects everything, but you saw three problems when you played it:

1. **Long corridors cut through rooms.** The room list comes out of the BSP recursion as *all the
   leaves of the left half, then all the leaves of the right half*. Neighbours in the list are
   usually neighbours on the map, except where the two halves meet: there, room *i* and room
   *i + 1* can be on opposite sides of the map.
2. **There's only one route.** No loops, no shortcuts, no side branches.
3. **Every room is the same.** You can't say "put the boss far from the spawn", because nothing
   measures "far".

All three are questions about **which rooms are connected to which**. That structure has a name,
a **graph**, and computer science has a toolbox of algorithms for it. This lesson uses three:

| Question | Tool |
|---|---|
| What's the cheapest set of corridors that still connects every room? | Minimum spanning tree (Unit 5) |
| How many corridors from the spawn to each room? Which room is farthest? | Breadth-first search (Unit 7) |
| Which rooms are dead ends? | Degree (Unit 2) |

Graphs show up all over games: dungeon layouts, road networks, skill trees, dialogue trees,
quest dependencies, navigation meshes for AI (Lesson 5), and social networks between NPCs.

---

# Unit 2: Graph vocabulary

> **Definition: Graph.** A set of **vertices** (also called nodes) and a set of **edges**, where
> each edge joins two vertices. Written G = (V, E).

In our dungeon, **vertices are rooms** (numbered 0, 1, 2, …) and **edges are corridors**.

| Term | Meaning | Dungeon example |
|---|---|---|
| **Undirected** | an edge works both ways: a–b is the same as b–a | a corridor can be walked either way |
| **Directed** | an edge has a direction: a → b doesn't imply b → a | a one-way drop down a ledge |
| **Weight** | a number on an edge, usually a cost or length | the corridor's length |
| **Neighbours** (adjacent) | two vertices joined by an edge | rooms with a corridor between them |
| **Degree** | how many edges touch a vertex | how many doors a room has |
| **Path** | a sequence of vertices, each joined to the next | a route through the dungeon |
| **Cycle** | a path that comes back to where it started without reusing an edge | a loop you can walk round |
| **Connected** | there's a path between every pair of vertices | every room is reachable |
| **Tree** | a connected graph with **no cycles** | exactly one route between any two rooms |

### 2.1 Facts about trees
- A tree with *n* vertices has exactly **n − 1 edges**. Fewer, and it can't be connected. More,
  and it must have a cycle.
- In a tree there's **exactly one path** between any two vertices.
- Adding **any** edge to a tree creates exactly one cycle.

### 2.2 Two meanings of "leaf"
- In a **BSP** (Lesson 3), a leaf is a region that isn't split any further.
- In a **graph tree**, a leaf is a vertex with **degree 1**. In a dungeon, that's a **dead end**.

They're different things. In this lesson, "leaf" means a dead-end room.

---

# Unit 3: Representing a graph in code

### 3.1 Two classic representations

**Adjacency matrix:** an *n* × *n* table of bools, where `m[a, b]` is true if a and b are joined.

**Adjacency list:** for every vertex, a list of its neighbours.

```
    0 - 1          matrix:   0 1 2 3        list:  0: [1, 2]
    |                     0  . X X .               1: [0]
    2 - 3                 1  X . . .               2: [0, 3]
                          2  X . . X               3: [2]
                          3  . . X .
```

| | Matrix | List |
|---|---|---|
| Memory | n² cells, always | one entry per edge end (2·E) |
| "Are a and b joined?" | instant: one lookup | scan a's list |
| "Who are a's neighbours?" | scan a whole row (n cells) | read the list directly |
| Best for | **dense** graphs (most pairs joined) | **sparse** graphs (few edges each) |

A dungeon is **sparse**: each room has 1 to 4 corridors, not 15. And BFS (Unit 7) asks "who are
my neighbours?" over and over. So we use an **adjacency list**.

### 3.2 Undirected means stored twice
For an undirected graph, the edge a–b goes in **both** lists: b in a's list, and a in b's. That
is the rule our `RoomGraph` must never break. It's why `AddEdge` adds to both lists, and why
`Neighbours` hands out a **read-only** view (§3.4), so outside code can't add to just one.

### 3.3 The C# toolkit

| C# | Meaning | C++ equivalent |
|---|---|---|
| `List<List<int>>` | a list of lists | `std::vector<std::vector<int>>` |
| `int[] a = new int[n];` | a fixed-size array, starts all 0 | `std::vector<int> a(n);` |
| `bool[]` | starts all `false` | |
| `int.MaxValue` | the largest int (2 147 483 647), used as "infinity" | `INT_MAX` |
| `list.Contains(x)` | true if x is in the list (scans it) | `std::find(...) != end` |
| `(int a, int b)` | a **tuple**: a small bundle of named values | `std::pair<int,int>` with names |
| `foreach ((int a, int b) in edges)` | unpack each tuple into two variables | structured bindings `auto [a, b]` |
| `Queue<int>` | first in, first out: `Enqueue`, `Dequeue`, `Count` | `std::queue<int>` |
| `readonly` field | can only be assigned in the constructor | a `const` member (roughly) |
| `nameof(id)` | the string `"id"`, checked by the compiler | (none) |

A note on `Dictionary<K, V>`: if rooms had names or ids that weren't 0 to n − 1, you'd store the
adjacency list as `Dictionary<int, List<int>>` (like `std::unordered_map`). Because our rooms are
numbered 0, 1, 2, …, a plain list indexed by room id is simpler and faster.

### 3.4 `IReadOnlyList<T>`: handing out a view
```csharp
public IReadOnlyList<int> Neighbours(int id) => neighbours[id];
```
`IReadOnlyList<int>` is an **interface**: a promise of what you can do with something (index it
with `[i]`, read `.Count`, loop over it) without saying what it really is. A `List<int>` keeps
that promise, so you can return it directly. The caller can **read** the list but can't call
`Add` or `Remove`. It's like returning `const std::vector<int>&` in C++.

### 3.5 The `RoomGraph` class (Lab A)
```csharp
var g = new RoomGraph(4);   // rooms 0..3, no edges
g.AddEdge(0, 1);            // true: a new edge
g.AddEdge(1, 0);            // false: same edge (undirected)
g.HasEdge(1, 0);            // true
g.Degree(0);                // 1
g.Neighbours(1);            // [0]
g.Edges();                  // [(0, 1)]: each edge once, with a < b
```
`AddEdge(2, 2)` throws: a corridor from a room to itself (a **self-loop**) makes no sense here.

---

# Unit 4: Which rooms *could* connect?

Before choosing corridors, decide what a corridor between any two rooms would **cost**. Imagine
every room joined to every other: the **complete graph**. With *n* rooms that's n(n − 1)/2 possible
edges (for 12 rooms: 66). Each one gets a **weight**: the distance between the two room centres.

### 4.1 Manhattan distance
> **Definition: Manhattan distance.** |x₁ − x₂| + |y₁ − y₂|: the distance if you can only move
> along the grid's axes, like a taxi on city blocks.

Our corridors are L-shaped: one horizontal leg and one vertical leg. An L corridor from (1, 2) to
(4, 6) is 3 cells across and 4 up, so **its length is exactly the Manhattan distance**, 7.
(The straight-line Euclidean distance, √(3² + 4²) = 5, would underestimate it.)

```csharp
int d = Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
```

---

# Unit 5: Minimum spanning trees and Prim's algorithm

> **Definition: Spanning tree.** A tree that uses edges of a graph to connect **all** its vertices.
> **Minimum spanning tree (MST).** The spanning tree with the smallest total edge weight.

For a dungeon, the MST is **the shortest set of corridors that still connects every room**.
Short corridors join nearby rooms; nothing crosses the whole map unless it has to.

Your Lab 3 chain was a spanning tree too (n − 1 edges, connected, no cycles), just not the
minimum one. So the MST is never longer than the chain. One of the tests checks exactly that.

### 5.1 Prim's algorithm, the idea
Grow the tree one room at a time, always adding the **cheapest edge** that reaches a new room.
```
put room 0 in the tree
repeat until every room is in the tree:
    among all edges from a tree room to a non-tree room, take the shortest
    add that edge and its new room to the tree
```
**Why it never makes a cycle:** every added edge goes to a room that's **not yet** in the tree.
A cycle needs an edge between two rooms that are already connected, and Prim never adds one.

**Why it's minimal (intuition):** at each step the tree has to reach outside itself *somehow*,
and Prim takes the cheapest way out. Swapping that edge for any other could only cost more.
(This is called the **cut property**.)

### 5.2 Making "the shortest edge out" fast
Checking every tree room against every non-tree room on every step is slow. Instead, every room
**outside** the tree remembers:
- `bestCost[u]`: the shortest edge found so far from the tree to u (∞ at the start)
- `bestFrom[u]`: which tree room that edge comes from (−1 = none)

Each step: **pick** the non-tree room with the smallest `bestCost`, **add** it, then **update**:
the new room might offer a shorter edge to the rooms still outside.

```
inTree[*] = false;  bestCost[*] = ∞;  bestFrom[*] = −1
bestCost[0] = 0
repeat n times:
    v = the room not in the tree with the smallest bestCost      (ties → lowest index)
    inTree[v] = true
    if bestFrom[v] ≠ −1: add edge (bestFrom[v], v)
    for each room u not in the tree:
        d = Distance(v, u)
        if d < bestCost[u]: bestCost[u] = d; bestFrom[u] = v
```
Cost: *n* steps, each scanning *n* rooms → **O(n²)**. For 15 rooms that's 225 checks: nothing.
(With a **priority queue**, which comes in Lesson 5, it's O(E log V), which matters for big graphs.)

### 5.3 Worked example (the lecture's 5 rooms)
Rooms: A (0, 0), B (4, 0), C (4, 3), D (10, 3), E (0, 8).

| Edge | AB | AC | AD | AE | BC | BD | BE | CD | CE | DE |
|---|---|---|---|---|---|---|---|---|---|---|
| Length | 4 | 7 | 13 | 8 | 3 | 9 | 12 | 6 | 9 | 15 |

`bestCost` after each step (★ = picked, — = already in the tree):

| Step | Pick | Edge added | A | B | C | D | E |
|---|---|---|---|---|---|---|---|
| start | | | 0 | ∞ | ∞ | ∞ | ∞ |
| 1 | A ★ | (none: root) | — | 4 (A) | 7 (A) | 13 (A) | 8 (A) |
| 2 | B ★ | A–B (4) | — | — | **3 (B)** | **9 (B)** | 8 (A) |
| 3 | C ★ | B–C (3) | — | — | — | **6 (C)** | 8 (A) |
| 4 | D ★ | C–D (6) | — | — | — | — | 8 (A) |
| 5 | E ★ | A–E (8) | — | — | — | — | — |

**MST = A–B, B–C, C–D, A–E. Total = 21.** Bold entries got cheaper because of the room just added.

### 5.4 Kruskal's algorithm (for comparison)
Sort **all** edges by length, then go through them shortest first, adding each edge unless it
would make a cycle. Same answer (21 here: BC 3, AB 4, CD 6, skip AC because it closes a cycle,
AE 8). Detecting "would make a cycle" needs a structure called **union-find**: that's a lab
challenge. Prim suits us better: no sorting, and simple arrays.

---

# Unit 6: Loops on purpose

A tree has **exactly one route** between any two rooms. As a dungeon that means:
- lots of **dead ends** and backtracking
- no way round a dangerous room
- the map feels like a maze rather than a place

Many dungeon designs therefore start from a spanning tree (guaranteed connected, no wasted
corridors) and **add back a few extra edges**. Each extra edge creates exactly one cycle.

**Which edges?** Only **short** ones. A long extra edge would cut across the map: the Lesson 3
problem again. Our rule:

```
for every pair (a, b) with a < b:
    if they're already joined: skip
    if Distance(a, b) > maxDistance: skip
    with probability `chance`, add the edge
```

`rng.NextDouble()` returns a random `double` in [0.0, 1.0), so `rng.NextDouble() < chance` is
true with probability `chance`. `0.25` means about one in four close pairs gets a corridor.

**The trade-off** is a design choice, not a right answer:

| More loops | Fewer loops |
|---|---|
| more routes, less backtracking | more dead ends (good for secrets and treasure) |
| harder to control where the player goes | the path to the boss is more predictable |

**Reproducibility:** loop over pairs in a fixed order, and only call `rng` for pairs that passed
both checks. Then the same seed always gives the same loops.

---

# Unit 7: Breadth-first search

> **Definition: Breadth-first search (BFS).** Explore a graph from a start vertex in **layers**:
> first the start, then everything 1 edge away, then everything 2 edges away, and so on.

### 7.1 Why a queue
A **queue** is first in, first out (FIFO), like a line at a shop. BFS puts newly found rooms at
the **back** and takes the next room from the **front**. Every room at distance *k* is found
before any room at distance *k + 1* is taken out, so the layers come out in order.

(Swap the queue for a **stack**, last in first out, and you get **depth-first search**, which
dives down one branch before trying the next. It still finds everything reachable, but it doesn't
give shortest distances. The lab's tests use DFS to check connectivity independently of your BFS.)

### 7.2 The algorithm
```
distance[*] = −1                         (−1 means "not reached yet")
distance[start] = 0;  queue = [start]
while the queue isn't empty:
    room = queue.Dequeue()
    for each neighbour n of room:
        if distance[n] == −1:
            distance[n] = distance[room] + 1
            queue.Enqueue(n)
```
`distance` doubles as the **visited** marker: a room is visited exactly when its distance isn't −1.
Marking a room **when it's enqueued** (not when it's dequeued) stops it being queued twice.

**Cost:** every room is enqueued once, and every edge is looked at twice (once from each end) →
**O(V + E)**.

### 7.3 Worked example
```
  3 - 0 - 1 - 2
          |
          4 - 5 - 6
```
BFS from 0:

| Dequeued | New neighbours found | Queue after | distance |
|---|---|---|---|
| (start) | | [0] | 0:0 |
| 0 | 3, 1 → distance 1 | [3, 1] | 3:1, 1:1 |
| 3 | (none new) | [1] | |
| 1 | 2, 4 → distance 2 | [2, 4] | 2:2, 4:2 |
| 2 | (none new) | [4] | |
| 4 | 5 → distance 3 | [5] | 5:3 |
| 5 | 6 → distance 4 | [6] | 6:4 |
| 6 | (none new) | [] | |

Result: `[0, 1, 2, 1, 2, 3, 4]`. The **farthest** room from 0 is room 6 (distance 4).

### 7.4 What BFS gives us
- **Distances** from the spawn to every room (`Distances`).
- The **farthest room** (`Farthest`): the biggest distance. Ties go to the lowest id, so the
  result is reproducible.
- A **connectivity check:** if any distance is still −1, that room is unreachable.
- On a **tile grid**, the same algorithm with "neighbours = the 4 adjacent floor cells" is the
  **flood fill** your Lab 3 tests used (`CountReachableFloors`).

BFS counts **edges**, not tiles: "the boss is 4 rooms away", not "40 tiles away". For room
roles, that's exactly the measure players feel.

---

# Unit 8: Room roles

```csharp
public enum RoomType { Normal, Spawn, Boss, Treasure, Shop }
```
A new `RoomType[]` starts with every entry `Normal`, because `Normal` is the first value (0).

### 8.1 The rules, in order
Each step only changes rooms that are **still Normal**, so earlier roles are never overwritten.

| Step | Role | Rule | Why |
|---|---|---|---|
| D2 | Spawn | the room passed in (the generator uses room 0) | the player starts here |
| D2 | Boss | `Bfs.Farthest(graph, spawn)`, if it isn't the spawn itself | the whole dungeon lies between you and the boss |
| D3 | Treasure | every room with **degree 1** (a dead end) | rewards exploring side branches |
| D4 | Shop | **one** of the remaining Normal rooms, picked with `rng` | somewhere to spend loot on the way |

Edge cases:
- **One room:** the farthest room from the spawn is the spawn, so there's no boss.
- **Two rooms:** spawn and boss; no Normal room left, so no shop. Only call `rng.Next(count)`
  when `count > 0`, because `rng.Next(0)` throws.

### 8.2 Data next to data: parallel arrays
`Roles[i]` is the role of `Rooms[i]`, and graph vertex *i* is room *i*. Keeping the **same index**
across `Rooms`, `centres`, the graph and `Roles` is what lets each piece stay simple. Break that
(for example by sorting one list) and everything silently refers to the wrong room.

### 8.3 Where this is going
Roles are the hook for the rest of the course: enemies by role (Lesson 5), loot tables per role
(Lesson 8), and hand-made room layouts per role with ScriptableObjects (Lesson 7).

---

# Unit 9: Putting it in the game, and Gizmos

### 9.1 The new pipeline
```
seed → System.Random
     → Split into leaves, one room per leaf          (Lesson 3)
     → centres[i] = Center(Rooms[i])
     → Graph = Prim(centres)                          (B)
     → Loops.AddExtra(Graph, centres, ...)            (D1)
     → for each edge (a, b): CarveL(centres[a], centres[b])
     → Roles = RoomRoles.Assign(Graph, 0, rng)        (D2–D4)
     → Spawn = Center(Rooms[0])
```
Everything random still comes from the **one** `rng`, so the same seed still gives the same
dungeon, graph and roles.

### 9.2 Gizmos: drawing debug information in the editor
> **Definition: Gizmo.** A shape Unity draws in the **Scene view** to help you see what your
> code is doing. Gizmos never appear in a built game.

Unity calls a method named `OnDrawGizmos()` on every MonoBehaviour that has one, each time it
redraws the Scene view. Inside it:
```csharp
Gizmos.color = Color.red;              // the colour for what's drawn next
Gizmos.DrawLine(from, to);             // a line between two world positions
Gizmos.DrawWireCube(centre, size);     // a box outline
```
Gizmos use **world** positions. Convert grid cells with the Tilemap, as `PlacePlayer` does:
`floorTilemap.GetCellCenterWorld(new Vector3Int(x, y, 0))`.

To see them in the **Game** view too, turn on its **Gizmos** button (top right of the Game view).

Gizmos are one of the most useful debugging tools in Unity: draw your data, and bugs that would
take an hour to find in numbers become obvious in seconds.

---

## Glossary
| Term | Meaning |
|---|---|
| Graph | Vertices joined by edges |
| Vertex / node | One thing in a graph (here: a room) |
| Edge | A connection between two vertices (here: a corridor) |
| Undirected | Edges work both ways |
| Weight | A cost on an edge (here: corridor length) |
| Degree | The number of edges touching a vertex |
| Path | A sequence of vertices, each joined to the next |
| Cycle | A path that returns to its start without reusing an edge |
| Connected | Every vertex can reach every other |
| Tree | A connected graph with no cycles; n vertices, n − 1 edges |
| Leaf (graph) | A vertex of degree 1: a dead end |
| Adjacency list | Each vertex's list of neighbours |
| Adjacency matrix | An n × n table of "is there an edge?" |
| Sparse / dense | Few edges per vertex / most pairs joined |
| Complete graph | Every pair of vertices joined |
| Manhattan distance | \|dx\| + \|dy\| |
| Spanning tree | A tree connecting all vertices of a graph |
| MST | The spanning tree with the smallest total weight |
| Prim's algorithm | Grow an MST by always adding the cheapest edge to a new vertex |
| Kruskal's algorithm | Build an MST by adding edges shortest first, skipping cycles |
| BFS | Explore in layers with a queue; gives shortest edge counts |
| DFS | Explore one branch deep first, with a stack |
| Queue (FIFO) | First in, first out |
| Gizmo | A debug shape drawn in the Scene view |

## Review problems
1. A graph has 8 rooms and is a tree. How many edges does it have? What happens if you add one more?
2. Draw the adjacency list for the edges 0–2, 1–2, 2–3, 3–4.
3. Why is an adjacency list a better fit for a dungeon than a matrix?
4. What is the Manhattan distance between (2, 9) and (7, 3)? Why use it instead of Euclidean distance here?
5. Run Prim's by hand on A (0, 0), B (5, 0), C (5, 5), D (0, 6). Which edges, and what total?
6. Why can't Prim's algorithm ever create a cycle?
7. Why is the MST never longer than the Lab 3 chain?
8. In BFS, why mark a room as visited when it's **enqueued**, not when it's dequeued?
9. What changes if you replace BFS's queue with a stack?
10. Give the BFS distances from room 0 in the loop 0–1–2–3–4–5–0.
11. In a 2-room dungeon, what roles do the rooms get? Why is there no shop?
12. Why must `centres`, `Rooms`, the graph and `Roles` all use the same index for the same room?
13. Name two things you'd want a Gizmo to show while debugging a dungeon generator.

<details><summary>Answers</summary>

1. 7 edges. One more edge creates exactly one cycle.
2. 0: [2] · 1: [2] · 2: [0, 1, 3] · 3: [2, 4] · 4: [3].
3. Dungeons are sparse (1–4 corridors per room), and BFS needs "who are my neighbours?", which a list answers directly.
4. 5 + 6 = 11. Our corridors are L-shaped, so their length is exactly the Manhattan distance.
5. Lengths: AB 5, AC 10, AD 6, BC 5, BD 11, CD 6. From A: B is cheapest (5) → A–B. Via B, C drops to 5 → B–C. D's best is 6 from A; C–D is also 6, which isn't *shorter*, so it stays → A–D. Total 5 + 5 + 6 = 16.
6. Every edge it adds goes to a vertex not yet in the tree, and a cycle needs an edge between two vertices that are already connected.
7. The chain is a spanning tree too, and the MST is the smallest spanning tree.
8. Otherwise the same room can be enqueued several times (by different neighbours) before it's processed.
9. It becomes depth-first search: it still reaches everything, but the distances are no longer shortest.
10. [0, 1, 2, 3, 2, 1].
11. Spawn and Boss. Both rooms already have a role, so no Normal room is left for the shop.
12. Each piece refers to rooms only by number. If one list is in a different order, roles, corridors and outlines get attached to the wrong rooms with no error.
13. The graph's edges, and each room's role (colour), for example. Also the spawn cell, or the BFS distance of each room.
</details>
