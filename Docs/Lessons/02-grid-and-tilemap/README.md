# Lab 2: Camera, grids, and Tilemaps

**Pre-lab:** work through the [Lecture 2 slides](https://claude.ai/artifact/4WEHL1yUd5v2KUy2oSiKQP)
while filling in the [guided-notes slides](https://claude.ai/artifact/FwxucXJn12wV3n6R9VXWej).
[LECTURE.md](LECTURE.md) is the written reference. Section numbers below (§) point into it.
**Deliverable:** a playable walled room with a smoothly following camera, all 18 tests green,
committed and pushed.

| Exercise | File | Lecture units | Checked by |
|---|---|---|---|
| A. Camera follow | `Assets/_Project/Scripts/Core/CameraFollow.cs` | 1, 2, 3 | Play mode |
| B. The map grid | `Assets/_Project/Scripts/Dungeon/MapGrid.cs` | 2, 4, 6, 7, 8, 9 | 18 unit tests |
| C. Drawing the map | `Assets/_Project/Scripts/Dungeon/MapRenderer.cs` | 5, 10 | Play mode |

Do them in order: **C uses B**, so the map can't appear until B passes its tests.

---

## Part 0: Project changes since Lab 1

These were added for you. Each is explained further in the lecture.

- **Namespaces.** Every script is now inside `namespace Roguelite.Something { ... }`. This
  is like a surname for your classes, so `Roguelite.Dungeon.MapGrid` can't clash with
  another package's `MapGrid`.
- **Assembly definition** (`Scripts/Roguelite.asmdef`). This compiles the game scripts as their
  own unit, instead of in Unity's catch-all `Assembly-CSharp`. Recompiles are faster, and the
  test assembly can reference the game code by name.
- **Test assembly** (`Tests/EditMode/`). This holds NUnit tests (Lecture Unit 9). It exists
  only in the editor and never ships.
- **`Sandbox/`**, a scratch folder for experiments.

---

## Exercise A: Camera follow ✅ (completed)

**Concepts:** frame order and `LateUpdate` (§1.3), interpolation (§1.4), `ref` (§2.2),
`=` vs `==` (§2.3), `SmoothDamp` (§3.3).

**Pre-lab questions**
1. Why must the camera's z stay at −10?
2. What is the *first* argument to `SmoothDamp`?

**Setup**
1. Add **CameraFollow** to **Main Camera**, and drag **Player** into **Target**.
2. On Player's **Rigidbody2D**, set **Interpolate** to **Interpolate**.

**Tasks:** TODO A1 to A3 in `CameraFollow.cs`.

**Done when:** the camera follows smoothly with no stutter. Try `smoothTime` at 0, 0.15
and 0.5.

**Lab notes, debugging log.** Record each bug you hit, its cause, and its fix. Yours so far:

| Symptom | Cause | Fix | Lecture § |
|---|---|---|---|
| Target resets to None on Play | `if (target = null)` assigns instead of comparing | Use `==` | 2.3 |
| Camera doesn't smooth | First SmoothDamp argument was `target.position` | Use `transform.position` | 3.3 |
| Movement stutters | Physics at 50 Hz, rendering at 60–120 Hz | Rigidbody2D Interpolate | 1.4 |

---

## Exercise B: The map grid

**Concepts:** enums and default values (§2.4), arrays and 2D grids (§4.1–4.3), Big-O (§4.4),
bounds checks, guard clauses and sentinels (§6), neighbourhoods and direction arrays (§7),
encapsulation (§8), unit tests (§9).

**Pre-lab questions** (answer before you code)
1. If you allocate `new TileType[width, height]`, in which order must you index it?
2. What should `InBounds(Width, 0)` return, and why?
3. What does `Get` return for a cell outside the grid, and what kind of value is that called?
4. How many cells does `new RectInt(1, 1, 3, 2)` cover? List their coordinates.
5. For the corner cell (0, 0) of a 3 × 3 all-floor grid, how many 8-way *wall* neighbours are there?

**Tasks:** TODO B1 to B6 in `MapGrid.cs`, in order.

**Procedure**
1. Open **Window → General → Test Runner → EditMode** and click **Run All**. All 18 tests
   start red. That's expected (§9.3, red → green).
2. Complete one TODO, save, and re-run. Tests are prefixed with their TODO ID (`B1_`, `B2_`, ...).
3. When a test fails, click it and read *Expected* vs *But was*.
4. Read the test code for the TODO you're on. It's the exact specification.

**Done when:** all 18 tests are green ✅.

**Reflection** (write a sentence or two in your notes)
- Which Big-O class is each of your methods (§4.4)?
- `Set` ignores out-of-bounds writes. Do you agree with that policy (§6.5)?

---

## Exercise C: Drawing the map

**Concepts:** coordinate systems and conversion (§5), Tilemap components (§10.1),
painting from code (§10.2), tilemap collision (§10.3), serialized references (§10.4).

**Pre-lab questions**
1. Name the four pieces of the Tilemap system and what each does.
2. Why do the walls need both a `TilemapCollider2D` and a `CompositeCollider2D`?
3. Which method converts a grid cell to a world position?

### Setup in the editor

**Tiles**
1. In `Assets/_Project/Art`, right-click and choose **Create → 2D → Sprites → Square**. Name it `TileSquare`.
2. Open **Window → 2D → Tile Palette**. Click **Create New Palette**, name it `Dungeon`, and save it in `Assets/_Project/Art`.
3. Drag `TileSquare` into the palette, and save the tile as `FloorTile` in `Assets/_Project/Art`.
4. Select `FloorTile` and duplicate it (`Cmd+D`). Rename the copy `WallTile`.
5. Set each tile's **Color** in the Inspector, for example dark grey for floor and a lighter colour for walls.

**Tilemaps**

6. In the Hierarchy, choose **2D Object → Tilemap → Rectangular**. This creates `Grid`, with a child that you rename `Floor`.
7. Right-click `Grid`, choose **2D Object → Tilemap → Rectangular** again, and rename it `Walls`.
8. On `Walls`:
   - **TilemapRenderer → Order in Layer** = `1`
   - add `TilemapCollider2D`, and tick **Used By Composite**
   - add `CompositeCollider2D`, and set the auto-added Rigidbody2D's **Body Type** to **Static**
9. On Player's **SpriteRenderer**, set **Order in Layer** = `10`.

**Script**

10. Create an empty GameObject named `Map`, and add **MapRenderer**.
11. Assign `Floor` and `Walls` (from the Hierarchy), `FloorTile` and `WallTile` (from the Project window), and `Player`.
12. On **Main Camera**, set **Size** to about `7`.

**Tasks:** TODO C1 to C3 in `MapRenderer.cs`.

**Tip:** use the component's **⋮ → Build Map** to build in Edit mode, without pressing Play.
The Console also prints the map as text (`#` and `.`).

**Done when:** you start in the middle of a walled room, can't pass through any wall,
and the camera follows. Commit and push. 🎉

---

## Challenges (optional, no solutions)

1. **Pillars.** Add several 2 × 2 wall pillars inside the room.
2. **Two rooms and a corridor.** Connect them with a 1-tile hallway (a preview of Lecture 3).
3. **Neighbour debugger.** In `Sandbox/`, on mouse click convert screen → world → cell (§5),
   then log the cell, its type and its wall-neighbour count.
4. **Hide buried walls.** Only place a wall tile if it has at least one floor neighbour (8-way).
5. **Camera bounds.** Clamp the camera so it never shows past the map edges
   (`Mathf.Clamp`, `orthographicSize`, `aspect`).
6. **Flat array refactor.** Re-implement `MapGrid` internally with a 1D array and
   `index = y * Width + x` (§4.2). If the tests still pass, you've proven your refactor is
   correct. That's exactly what tests are for.

---

**Stuck?** Try [HINTS.md](HINTS.md) first. Full answers are in `Solutions/`.
