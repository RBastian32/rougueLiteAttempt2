# Lab 3: Procedural generation

**Pre-lab:** work through the [Lecture 3 slides](https://claude.ai/artifact/7yhR4sMjhn72CYKUPDYw9k) while filling in the
[guided-notes slides](https://claude.ai/artifact/FVufEG56hJnzX9TRcf75UY). The **LAB PREVIEW** slides at the end of each unit
show the plan for each TODO. [LECTURE.md](LECTURE.md) is the written reference (§ = its sections).

**Deliverable:** press Play and get a new, fully connected dungeon every run (BSP rooms or a
cave), with the seed logged. All 35 Lesson 3 tests green, plus your 18 Lesson 2 tests.

| Exercise | File | Lecture units | Checked by |
|---|---|---|---|
| A. Random walk | `Scripts/Dungeon/Generation/RandomWalk.cs` | 2, 3, 4 | 9 tests (`A…`) |
| B. Corridors | `Scripts/Dungeon/Generation/Corridors.cs` | 3, 5 | 9 tests (`B…`) |
| C. BSP | `Scripts/Dungeon/Generation/Bsp.cs` | 2, 3, 6, 7 | 10 tests (`C…`) |
| D. The generator | `Scripts/Dungeon/Generation/DungeonGenerator.cs` | 8 | 7 tests (`D…`) |
| E. In the game | `Scripts/Dungeon/MapRenderer.cs` | 3.6, 9 | Play mode |

Do them **in order**: D uses A, B and C, and E uses D. You can run all the tests at any time;
each TODO's tests turn green as you finish it.

Two tests start green: `A2_Carve_TinyGrid_Throws` and `C1_Split_MinLeafSizeBelowOne_Throws`.
They test guard clauses that are already written for you. Everything else starts red.

---

## Exercise A: The random walk
**Concepts:** seeds and `System.Random` (§2), static classes (§3.1), `Mathf.Clamp` (§3.5), random walk (§4).

**Pre-lab questions**
1. What does `rng.Next(4)` return? Could it ever return 4?
2. On a 10 × 8 grid, what's the allowed range of x for the walker? Of y?
3. Why is every cave the walker digs connected?

**Tasks:** TODO A1 (`ClampToInterior`), then A2 (`Carve`).

**Done when:** all 9 `A…` tests are green.

---

## Exercise B: Corridors
**Concepts:** inclusive points vs half-open rectangles (§5.1), `Mathf.Min/Max` (§3.5), optional parameters (§3.3).

**Pre-lab questions**
1. A corridor from x = 9 to x = 4: how many cells? What `RectInt` (row 2, thickness 1)?
2. Thickness 3 on row 5: which rows are carved?
3. Horizontal-first L from (2, 2) to (8, 9): where's the corner?

**Tasks:** TODO B1, B2, B3. **B3 should only call B1 and B2.** No new carving code.

**Done when:** all 9 `B…` tests are green.

---

## Exercise C: Binary space partitioning
**Concepts:** recursion (§6), BSP (§7), `List<T>` (§3.2), `Next(min, max)` (§2.2).

**Pre-lab questions**
1. With `minLeafSize = 8`, can a 15 × 30 area be cut vertically? Horizontally?
2. A 30-wide area, `minLeafSize = 8`. What's the smallest and largest `cut`? What do you pass to `rng.Next`?
3. What is `Split`'s base case? What's its recursive case?
4. Leaf (10, 20, 12, 9), margin 1, room width 5. What range of x is allowed?

**Tasks:** TODO C1 to C4 in `Split`, then C5 in `PlaceRoom`.

**Tip:** the `C3_` test runs 20 seeds. If it fails, the message names the seed and the bad leaf.
Work out by hand what your code does for that leaf.

**Done when:** all 10 `C…` tests are green.

---

## Exercise D: The generator
**Concepts:** the pipeline, one generator passed in (§2.4, §8.1), testing random code (§8.2).

**Pre-lab questions**
1. Why must there be exactly **one** `System.Random` per generated dungeon?
2. With 6 rooms, how many corridors do you carve? What's the loop bound?
3. Why does the chain of corridors guarantee everything is connected?

**Tasks:** TODO D1 to D4 in `GenerateBsp`, then D5 in `GenerateCave`.
**Read the guard clauses first.** They're done for you, and they tell you which settings can't work.

**Done when:** all 7 `D…` tests are green. **All 35 Lesson 3 tests should now be green.**

---

## Exercise E: In the game
**Concepts:** enums as dropdowns, `switch` (§3.6), seeds from the clock (§2.5), spawning (§9).

There's no stub file for this one. **You** add the code to your own `MapRenderer.cs`, following
these steps. The LAB PREVIEW slides in Unit 9 show the syntax.

1. **E1 – The enum.** Above `public class MapRenderer`, inside the namespace, declare
   `public enum GeneratorType { TestRoom, TwoRooms, Cave, Bsp }`.
2. **E2 – The fields.** Add a `[Header("Generation")]` section with three `[SerializeField] private` fields:
   - `GeneratorType generator`, defaulting to `Bsp`
   - `int seed`, defaulting to `0`, with a `[Tooltip]` saying 0 means random
   - `int caveSteps`, defaulting to `1500`
3. **E3 – The seed.** At the start of `Build()` (after the null check), work out `usedSeed`: the
   `seed` field if it isn't 0, otherwise `System.Environment.TickCount`. Log it with `Debug.Log`.
4. **E4 – The switch.** Replace the `Grid = CreateTwoRooms();` line with a `switch (generator)` that
   sets `Grid` and a local `Vector2Int spawn` for each of the four cases:
   - BSP and Cave: call `DungeonGenerator`, then copy `.Grid` and `.Spawn` from the result
   - TestRoom and TwoRooms: use your old methods, with the spawn at the middle of the map
5. **E5 – The spawn.** Change `PlacePlayer()` to `PlacePlayer(Vector2Int spawn)`, use `spawn` instead
   of the middle cell, and pass `spawn` from `Build()`.

**Editor setup**
1. On **Map**, set **Width = 60, Height = 40**. BSP needs room for several leaves. Press **Cmd+S**.
2. On **Player**'s BoxCollider2D, set **Size** to `0.8, 0.8`. BSP corridors are 2 tiles wide;
   a slightly smaller collider stops the player snagging on corners.

**Done when:**
- Each Play gives a new dungeon, and the Console says which seed it used.
- Typing that seed into **Seed** gives the same dungeon again.
- **Generator = Cave** gives a cave; TestRoom and TwoRooms still work.
- You spawn on floor every time, and can walk to every room.
- Commit and push. 🎉

**Reflection** (a sentence or two in your notes)
- What would happen if `GenerateBsp` created a `new System.Random()` inside the loop in D2?
- The BSP corridors sometimes cut straight through a room on the way to the next. Is that a bug or a feature?

---

## Challenges (optional, no solutions)
1. **Regenerate key.** Press **R** in Play mode to build a new dungeon (new random seed). Hint: `Input.GetKeyDown(KeyCode.R)` in an `Update`.
2. **Fill target.** Change the cave generator to keep walking until a target percentage of the map is floor (e.g. 40%), instead of a fixed number of steps.
3. **Several walkers.** Start three walkers from random cells inside the first one's cave. How does the shape change?
4. **Varied leaves.** Stop splitting a leaf early with some probability (e.g. 25% once it's under 3 × minLeafSize), so room sizes vary more.
5. **Rooms + caves.** After BSP, run a short random walk from each room's centre to roughen the edges.
6. **Seed from text.** Let the designer type a word ("dragon") as the seed. Hint: `"dragon".GetHashCode()` gives an int. Is that stable between runs? Look it up.

---

**Stuck?** Try [HINTS.md](HINTS.md) first. Full answers are in `Solutions/`.
