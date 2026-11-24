# Lecture 3: Procedural Generation

**Companion slides:** [lecture slides](https://claude.ai/artifact/7yhR4sMjhn72CYKUPDYw9k) · [guided notes](https://claude.ai/artifact/FVufEG56hJnzX9TRcf75UY).
This file is the written reference: the same material, for looking things up.
**Lab:** [README.md](README.md). Section numbers (§) here match the lab handout.

## Learning objectives
By the end of this lecture you should be able to:
1. Explain what procedural generation is, and why roguelites depend on it.
2. Define a pseudo-random number generator and a seed, and explain why a seeded generator is repeatable.
3. Use `System.Random` correctly, including its half-open `Next(min, max)`.
4. Use the C# tools generators need: static classes, `List<T>`, optional parameters, `switch`.
5. Implement a random walk, and explain why its caves are always connected.
6. Carve straight and L-shaped corridors between two points (inclusive ends).
7. Explain recursion: base case, recursive case, and the call stack.
8. Implement binary space partitioning, and place non-overlapping rooms in its leaves.
9. Assemble a generator from small parts, and test it for determinism and connectivity.

## Lecture map
| Unit | Topic | Used in lab |
|---|---|---|
| 1 | What procedural generation is | all |
| 2 | Randomness and seeds | A2, C, D, E |
| 3 | The C# toolkit for generators | all |
| 4 | The random walk | A1, A2 |
| 5 | Corridors | B1–B3 |
| 6 | Recursion | C1–C4 |
| 7 | Binary space partitioning | C1–C5 |
| 8 | Assembling and testing a generator | D1–D5 |
| 9 | Hooking it into Unity | E |

---

# Unit 1: What procedural generation is

> **Definition: Procedural content generation (PCG).** Creating game content (levels, items,
> enemies, textures) with an **algorithm**, instead of building every piece by hand.

In Lesson 2 you placed every room by hand with `FillRect`. That's fine for one level, but a
roguelite needs a **different dungeon every run**: the surprise of a new layout is the genre's
core appeal. Nobody can hand-design thousands of levels, so we write code that designs them.

**What makes a generated level *good*?**
| Goal | Meaning | How we'll guarantee it |
|---|---|---|
| Variety | Runs feel different | Randomness (Unit 2) |
| Playability | The player can reach everything | Connectivity: corridors, and tests (Unit 8) |
| Structure | It reads as a place, not noise | Algorithms with rules: rooms, caves (Units 4, 7) |
| Control | Designers can tune it | Parameters: sizes, steps, seeds |

**Two families of algorithm:**
- **Constructive:** build the level in one pass by following rules, guaranteeing correctness as you go. *Random walk and BSP are constructive.*
- **Generate and test:** produce a candidate, check it, and throw it away if it fails. Simple, but can waste time.

---

# Unit 2: Randomness and seeds

### 2.1 Computers can't roll dice
A computer follows instructions exactly, so it can't produce true randomness on its own. Instead
it uses a formula that produces numbers that *look* random.

> **Definition: Pseudo-random number generator (PRNG).** An algorithm that produces a long
> sequence of numbers that look random, where each number is computed from internal state.
> The sequence is completely determined by its starting state.

> **Definition: Seed.** The number that sets a PRNG's starting state. **The same seed always
> produces the same sequence.**

```csharp
var a = new System.Random(42);
var b = new System.Random(42);
a.Next(100);   // 66 (for example)
b.Next(100);   // 66 again: same seed, same sequence
```

This is a feature, not a weakness:
- **Replays and sharing:** "seed 9137 is a great dungeon" → anyone can play it.
- **Daily challenges:** everyone gets today's seed.
- **Debugging:** a bug in seed 512 can be reproduced exactly, every time.
- **Testing:** your tests can check "same seed, same dungeon".

### 2.2 The `System.Random` API
| Call | Returns |
|---|---|
| `new System.Random(seed)` | a generator with that seed |
| `rng.Next(n)` | an int from **0 to n − 1** |
| `rng.Next(min, max)` | an int from **min to max − 1** (half-open, like `RectInt`) |
| `rng.NextDouble()` | a double in [0, 1) |
| `rng.Next(2) == 0` | a coin flip: true or false |

⚠️ **The top value is excluded.** To get a number from 4 to 10 *inclusive*, write `rng.Next(4, 11)`.
This is the same half-open convention as `RectInt.xMax` and `for (i = 0; i < n; i++)`.

### 2.3 Two classes called `Random`
Unity has its own `UnityEngine.Random`. If a file has both `using System;` and `using UnityEngine;`,
the name `Random` is **ambiguous** and won't compile. We write `System.Random` in full.

| | `UnityEngine.Random` | `System.Random` |
|---|---|---|
| State | One global generator for the whole game | Each object has its own |
| Seeding | `Random.InitState(seed)` affects everything | Per object |
| Risk | Any other script's call shifts your sequence | Isolated |
| Use for | Quick effects (particle jitter, hit sparks) | **Generation that must repeat** |

### 2.4 One generator, passed in
Create **one** `System.Random` from the seed at the start of generation, and **pass it as a
parameter** to every step. If each step made its own `new System.Random()`, they'd be seeded from
the clock and the result would never repeat. Passing a dependency in like this is called
**dependency injection**.

### 2.5 Picking a seed
- A fixed seed (`seed = 1234`): the same dungeon every time. Great for designing and debugging.
- "Surprise me": `System.Environment.TickCount` (milliseconds since the computer started) is
  different every run. **Always log the seed you used**, so you can replay it.

---

# Unit 3: The C# toolkit for generators

### 3.1 Static classes
```csharp
public static class RandomWalk
{
    public static void Carve(...) { ... }
}
RandomWalk.Carve(grid, rng, start, 500);   // called on the class itself
```
A `static class` can't be created with `new`. It only holds `static` methods: functions that don't
belong to any object. It's the C# equivalent of a C++ namespace full of free functions. Use one when
a method only needs its parameters (no fields).

### 3.2 `List<T>`
C#'s growable array, like C++ `std::vector<T>`. Needs `using System.Collections.Generic;`.
```csharp
var rooms = new List<RectInt>();          // empty
rooms.Add(new RectInt(1, 1, 4, 4));       // push_back
int n = rooms.Count;                      // size() (arrays use .Length; lists use .Count)
RectInt first = rooms[0];                 // indexing
rooms.AddRange(otherList);                // append a whole list
var one = new List<RectInt> { area };     // initialiser: a list with one item
foreach (RectInt r in rooms) { ... }      // visit each item
```
`List<T>` is a class (reference type): passing it to a method passes a reference, not a copy.

### 3.3 Optional parameters
```csharp
public static void CarveHorizontal(MapGrid grid, int x1, int x2, int y, int thickness = 1)
CarveHorizontal(grid, 2, 6, 3);      // thickness = 1
CarveHorizontal(grid, 2, 6, 3, 3);   // thickness = 3
```
Optional parameters must come after the required ones. Same as C++ default arguments.

### 3.4 A class as a bundle of results
A method returns one value. To return several (a grid, a list of rooms, a spawn cell), bundle them:
```csharp
public class DungeonResult
{
    public MapGrid Grid;
    public List<RectInt> Rooms = new List<RectInt>();
    public Vector2Int Spawn;
}
```

### 3.5 Integer helpers
`Mathf.Min(a, b)`, `Mathf.Max(a, b)` and `Mathf.Clamp(v, lo, hi)` all have **int** versions.
`Vector2Int` supports `+` and `-`: `pos + Vector2Int.up` is the cell above.

### 3.6 `switch`
```csharp
switch (generator)
{
    case GeneratorType.Bsp:
        ...
        break;          // required: C# never "falls through" to the next case
    default:
        ...
        break;
}
```
Like C's `switch`, but C# forbids accidental fall-through: every case must end with `break` (or `return`).

---

# Unit 4: The random walk

> **Definition: Random walk (drunkard's walk).** Start at a cell. Repeatedly step one cell in a
> random direction, turning every visited cell into floor.

```
pos = start (clamped inside the border)
dig pos
repeat steps times:
    dir = random one of up / right / down / left
    pos = clamp(pos + dir)         // never step onto the border ring
    dig pos
```

**Why clamp to 1 .. Width − 2?** The outer ring stays wall, so the cave is always enclosed and
the player can never walk off the map.

**Key property: always connected.** The walker never teleports. Every dug cell is next to the
previous one, so **every floor is reachable from the start**. No corridors needed.

| Parameter | Effect |
|---|---|
| More steps | Bigger, more open caves |
| Fewer steps | Thin, twisty tunnels |
| Map size | More room to wander |

**Cost:** O(steps) work. **Weakness:** the walker revisits cells a lot, and can bunch up in one
area. Variants: stop when a target % of the map is floor; run several walkers; bias the direction.

---

# Unit 5: Corridors

### 5.1 Points are inclusive; rectangles are half-open
A corridor from x = 2 to x = 6 must include **both ends**, or it won't touch the rooms.
That's 6 − 2 + 1 = **5** cells. A `RectInt` from xMin 2 with width 5 covers exactly those.

> **Rule:** cells from *a* to *b* inclusive → `x = a`, `width = b − a + 1`.

### 5.2 Straight corridors with `FillRect`
```
left  = min(x1, x2)          // so the caller can give the ends in either order
right = max(x1, x2)
FillRect( RectInt(left, y - thickness / 2, right - left + 1, thickness) )
```
`y - thickness / 2` centres a thick corridor on row `y` (thickness 3 → rows y−1, y, y+1).

### 5.3 L-shaped corridors
Two straight pieces meeting at a corner. Two choices of corner:
- **Horizontal first:** across on `from.y`, then up/down on `to.x`. Corner at `(to.x, from.y)`.
- **Vertical first:** up/down on `from.x`, then across on `to.y`. Corner at `(from.x, to.y)`.

Picking the order randomly makes corridors look less uniform.

---

# Unit 6: Recursion

> **Definition: Recursion.** A method that solves a problem by calling **itself** on a smaller
> version of the same problem.

Every correct recursive method has:
1. a **base case**: a version small enough to answer directly, with no further calls;
2. a **recursive case**: break the problem into smaller pieces and call itself on them.

```csharp
static int CountDown(int n)
{
    if (n == 0) return 0;          // base case
    return 1 + CountDown(n - 1);   // recursive case: a smaller problem
}
```

**The call stack.** Each call gets its own copy of its parameters and local variables, stacked on
top of the caller's. When a call returns, its frame is removed and the caller continues. The same
as C and C++.

**Infinite recursion.** Forget the base case (or make a "smaller" problem that isn't actually
smaller) and the stack grows until the program crashes with a `StackOverflowException`. That's
why `Bsp.Split` rejects `minLeafSize < 1`.

**When to use it:** when a problem naturally splits into smaller copies of itself: trees,
splitting regions, flood fills, sorting.

---

# Unit 7: Binary space partitioning (BSP)

> **Definition: Binary space partitioning.** Recursively cutting a region in two, then cutting
> each part in two, until the parts are small enough. The parts form a **binary tree**; the
> uncut parts at the bottom are the **leaves**.

```
             whole map
            /         \
        left half    right half
        /     \        /     \
     leaf    leaf    leaf    leaf
```

**Why BSP for dungeons:** the leaves **never overlap** and together cover the whole map. Put one
room inside each leaf, with a margin, and the rooms can never collide. They're also spread evenly.

### 7.1 Split
```
Split(area):
    canCutV = area.width  >= 2 * minLeafSize
    canCutH = area.height >= 2 * minLeafSize
    if neither: return [area]                         // base case: a leaf
    choose direction (longer side; square → coin flip)
    cut = random in minLeafSize .. size - minLeafSize   // both halves stay big enough
    return Split(first half) + Split(second half)     // recursive case
```
**Why `2 * minLeafSize`?** Both halves must be at least `minLeafSize`.
**Why cut the longer side?** It keeps leaves roughly square, so rooms aren't long slivers.
**`rng.Next(minLeafSize, size - minLeafSize + 1)`:** the `+ 1` because Next excludes its max.

### 7.2 Placing a room in a leaf
```
maxW = leaf.width  - 2 * margin
w    = random in minRoomSize .. maxW  (inclusive)
x    = random in leaf.xMin + margin .. leaf.xMax - margin - w  (inclusive)
```
(and the same for height and y). The `margin` keeps rock between neighbouring rooms.

---

# Unit 8: Assembling and testing a generator

### 8.1 The pipeline
```
seed → System.Random
     → Split the inner area into leaves      (Bsp.Split)
     → one room per leaf                      (Bsp.PlaceRoom + FillRect)
     → corridor from room i to room i + 1     (Corridors.CarveL)
     → spawn = centre of room 0
```
**Why is it connected?** Room 0 is joined to 1, 1 to 2, and so on: a **chain**. You can walk
along the chain from any room to any other. (Lesson 4 covers smarter ways to connect rooms.)

### 8.2 Testing random code
You can't test "the output is exactly this", but you can test **properties that must always hold**:
- **Determinism:** same seed → identical map.
- **Variety:** different seeds → different maps.
- **Invariants:** the border is always wall; rooms are floor and don't overlap.
- **Connectivity:** a **flood fill** from the spawn reaches every floor cell.

> **Definition: Flood fill.** Starting from one cell, repeatedly visit every unvisited
> neighbour that's floor, until there are none left. It finds everything reachable. (Lesson 4
> explains how it works; the tests use it already.)

Test many seeds in a loop: a bug might only show up for one seed in twenty.

---

# Unit 9: Hooking it into Unity

- An `enum GeneratorType { TestRoom, TwoRooms, Cave, Bsp }` becomes a **dropdown** in the Inspector.
- A `seed` field where **0 means random**: `int usedSeed = seed != 0 ? seed : System.Environment.TickCount;`
- **Log the seed** every time.
- `switch` on the generator type to fill `Grid` and a `spawn` cell.
- `PlacePlayer(spawn)` instead of always the map's middle (the middle might be rock).
- A 60 × 40 map gives BSP enough room for 6 to 15 rooms.

---

## Glossary
| Term | Meaning |
|---|---|
| PCG | Generating content with an algorithm |
| PRNG | An algorithm producing random-looking numbers from a starting state |
| Seed | The starting state; same seed → same sequence |
| Deterministic | Same inputs always give the same output |
| Random walk | Repeated random single steps, digging as you go |
| Base case | The part of a recursive method that doesn't recurse |
| Call stack | The stack of active method calls and their local variables |
| BSP | Recursively cutting a region in two |
| Leaf | A region BSP doesn't cut any further |
| Invariant | A property that must hold for every output |
| Flood fill | Visiting everything reachable from a start cell |

## Review problems
1. Two `System.Random(7)` objects each call `Next(10)` three times. What can you say about the results?
2. Write a call that returns a random int from 3 to 8 **inclusive**.
3. Why would `new System.Random()` inside `RandomWalk.Carve` break the "same seed, same cave" test?
4. Why is every random-walk cave connected?
5. How many cells does a corridor from x = 9 down to x = 4 cover? What `RectInt` carves it (row 2, thickness 1)?
6. A 1-thick L corridor goes from (2, 2) to (8, 9), horizontal first. Where is the corner? How many cells?
7. What are the two parts of every recursive method? What happens if one is missing?
8. With `minLeafSize = 8`, can a 15 × 30 area be cut vertically? Horizontally?
9. Leaf (10, 20, 12, 9), margin 1, room width 5. What range of x is allowed?
10. Why does joining room i to room i + 1 guarantee every room is reachable?
11. Name three properties you can test about a random generator.
12. Why log the seed?

<details><summary>Answers</summary>

1. Both produce exactly the same three numbers.
2. `rng.Next(3, 9)`.
3. It would seed itself from the clock, ignoring the seed you passed, so two runs differ.
4. Each new cell is next to the previous one; the walker never jumps.
5. 6 cells (4, 5, 6, 7, 8, 9). `new RectInt(4, 2, 6, 1)`.
6. (8, 2). 7 + 8 − 1 = 14 cells.
7. A base case and a recursive case. Without a base case it never stops (stack overflow).
8. Vertically: no, 15 < 16. Horizontally: yes, 30 ≥ 16.
9. From 10 + 1 = 11 to 22 − 1 − 5 = 16 inclusive.
10. The rooms form a chain; you can walk along it from any room to any other.
11. Determinism, variety, invariants (border is wall, rooms don't overlap), connectivity.
12. To reproduce a level you liked, or a bug, by typing the seed back in.
</details>
