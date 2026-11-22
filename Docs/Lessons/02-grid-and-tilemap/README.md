# Lesson 2: Camera, grids, and Tilemaps

**Goal:** by the end you'll walk your player around a room with solid walls, and the
camera will glide along behind you. More importantly, the room will be built from **data**
(a 2D grid) by code, which is the foundation every later lesson (dungeon generation,
pathfinding, enemies) builds on.

**You'll write:**

| Exercise | File | Checked by |
|---|---|---|
| A. Camera follow | `Assets/_Project/Scripts/Core/CameraFollow.cs` | Play mode |
| B. The map grid | `Assets/_Project/Scripts/Dungeon/MapGrid.cs` | 18 unit tests |
| C. Drawing the map | `Assets/_Project/Scripts/Dungeon/MapRenderer.cs` | Play mode |

Do them in order: **C uses B**, so the map can't appear until B passes its tests.

---

## Part 0: What changed in the project (read once)

Since lesson 1, I've added a few pieces of project structure that real Unity projects use:

- **Namespaces.** Every script is now inside `namespace Roguelite.Something { ... }`.
  This is like a surname for your classes. It stops your `MapGrid` clashing with some other
  package's `MapGrid`.
- **An assembly definition** (`Scripts/Roguelite.asmdef`). By default Unity compiles every
  script into one big blob called `Assembly-CSharp`. An `.asmdef` file says "all scripts in
  this folder form their own unit." Two benefits: faster recompiles as the project grows, and
  the tests (below) can reference your game code by name.
- **A tests assembly** (`Tests/EditMode/`). This holds unit tests: code that checks your code.
  It only exists in the editor and never ships with the game.
- **`Sandbox/`**, a scratch folder for experiments.

After Unity reloads, check the **Console** (`Window > General > Console`). It should have no
red errors. Your Player should still move as before.

---

## Part 1: Camera follow (Exercise A)

### Concepts

**The update order within a frame.** Each frame, Unity calls `Update()` on *every* script,
then `LateUpdate()` on every script, then draws. If the camera moved in `Update`, it might
run *before* the player moved that frame, and the camera would lag one frame behind, which
looks like jitter. `LateUpdate` guarantees the target has already moved.

**Smoothing.** Snapping the camera exactly to the player is fine, but feels stiff.
`Vector3.SmoothDamp` moves a value *toward* a goal a bit each frame, fast when far away
and slowing as it gets close, like a spring with a damper. It needs to remember its current
speed between frames, which is why you pass it a `ref velocity` field.

**`ref`** means "pass this variable itself, not a copy," so the method can change it.

**The camera's z.** In 2D, everything is at z = 0 and the camera sits at z = -10 looking
forward. If you copied the player's whole position (z = 0), the camera would be *inside* the
sprites and show nothing. Keep the camera's own z.

### Setup
1. Select **Main Camera** and use **Add Component** to add **CameraFollow**.
2. Drag the **Player** from the Hierarchy into its **Target** field.
3. Select **Player**, and in its **Rigidbody2D** set **Interpolate** to **Interpolate**.
   (Physics moves the player 50 times a second, but the screen may draw 60 to 144 times.
   Interpolation fills in the gaps so the smooth camera doesn't reveal stutter.)

### Write it
Open `CameraFollow.cs` and complete **TODO A1 to A3**.

### Done when
Press Play and walk around. The camera follows smoothly, and the player stays visible.
Try `smoothTime` at 0, 0.15 and 0.5 while playing, and notice how the feel changes.

---

## Part 2: The map as data (Exercise B)

This is the most important part of the lesson.

### Concepts

**Why a grid?** Roguelite maps are made of tiles, so the natural way to store one is a
table of rows and columns: a **2D array**. In C# that's

```csharp
TileType[,] tiles = new TileType[width, height];
tiles[3, 5] = TileType.Floor;   // column 3, row 5
```

Looking up any cell is instant (no searching), and its neighbours are just `x ± 1`, `y ± 1`.
Almost every algorithm you'll write later (cave generation, flood fill, A* pathfinding,
field of view) works by walking around a grid like this.

**Other ways to store a grid** (good to recognise when you see them in tutorials):
- *Jagged array* `TileType[][]`: an array of arrays. Each row is separate, and rows can even
  be different lengths.
- *Flat array* `TileType[width * height]`, with index `y * width + x`. It's fast and simple to
  save to a file, and you'll see it in performance-focused code.
- *Dictionary* `Dictionary<Vector2Int, TileType>`: only stores cells that exist. It's great
  for huge or infinite maps where most space is empty.

We use `[,]` because it reads most clearly.

**`[x, y]` vs `[row, col]`.** Maths and spreadsheets often say "row then column," which is
`[y, x]`. Games usually say `[x, y]` so the code matches Unity's coordinates. Pick one and be
consistent: mixing them up is *the* classic grid bug. There's a test specifically for it.

**Data vs view.** `MapGrid` is a plain C# class, not a MonoBehaviour. It doesn't know sprites
exist. This separation means:
- you can **test** it without opening a scene,
- the generator, pathfinding, minimap and save system can all share one map,
- you could swap the visuals (3D, ASCII, whatever) without touching the logic.

**Bounds and sentinels.** Any time you compute `x + 1`, you might step off the edge of the
array, and C# will crash with `IndexOutOfRangeException`. Two defences:
1. `InBounds(x, y)`, a single place that knows what "inside" means.
2. A **sentinel**: `Get` returns `Wall` for anything outside. Now "count my floor neighbours"
   just works at the edges, because the outside world behaves like solid rock.

**4 vs 8 neighbours.** The 4 cardinal neighbours (up, down, left, right) are called the
*von Neumann* neighbourhood. Adding the diagonals gives the 8-cell *Moore* neighbourhood.
Movement often uses 4, while things like cave smoothing use 8. Storing the offsets in an
array (`Directions4`, `Directions8`) lets one loop handle either.

**Half-open ranges.** `RectInt(x: 2, y: 0, width: 3, height: 1)` covers x = 2, 3, 4.
Its `xMax` is 5, which is *one past the end*. Loops are written `x < xMax`, the same as
`i < array.Length`. This convention means width = xMax − xMin, with no ±1 fiddling.

**Guard clauses** (`throw new ArgumentException(...)` at the top of a method) reject bad
input immediately, with a clear message, instead of letting it cause a weird bug later.

### Write it
Open `MapGrid.cs` and complete **TODO B1 to B6**, in order.

**Test as you go.** Open `Window > General > Test Runner`, pick **EditMode**, and click
**Run All**. Tests are named after their TODO (`B1_...`, `B2_...`), so after finishing B2,
every `B1_` and `B2_` test should be green. Click a red test to see what it expected.

> **What is a NotImplementedException?** Until you finish B1, the constructor deliberately
> throws one, so *every* test fails. That's normal. This is *test-driven* learning: red first,
> then make it green.

### Done when
All 18 tests are green ✅.

---

## Part 3: Drawing the map (Exercise C)

### Concepts

**Unity's Tilemap system** has four pieces:
- **Grid**: a GameObject that defines cell size. It's the parent of your Tilemaps.
- **Tilemap**: a layer of tiles (like a layer in Photoshop). We'll have two, *Floor* and *Walls*.
- **TilemapRenderer**: draws a Tilemap.
- **Tile asset**: what goes *in* a cell (a sprite, a colour, collision settings).

**Grid coordinates vs world coordinates.** Cell `(3, 5)` is *which square*. World position
`(3.5, 5.5, 0)` is *where in the scene*. They only line up by coincidence. If you move or
scale the Grid object, they don't. Always convert with `tilemap.GetCellCenterWorld(cell)`
(grid → world) or `tilemap.WorldToCell(position)` (world → grid).

**Colliders on Tilemaps.** A `TilemapCollider2D` makes one collider box per tile, and your
player can snag on the seams between them. Adding a `CompositeCollider2D` merges them all
into one smooth outline. The composite needs a `Rigidbody2D` set to **Static**, which tells
physics "this never moves," and that's also cheaper for the physics engine.

**Sorting.** When sprites overlap, `Order in Layer` decides who's on top: higher values
are drawn later, so they appear in front.

### Setup in the editor (take your time, because this is real Unity skill)

**Make the tiles**
1. In the Project window, go to `Assets/_Project/Art`. Right-click and choose
   `Create > 2D > Sprites > Square`, then name it `TileSquare`.
2. Open `Window > 2D > Tile Palette`. Click **Create New Palette** and name it `Dungeon`.
   Save it in `Assets/_Project/Art`.
3. Drag `TileSquare` from the Project window into the palette. When asked where to save
   the tile, choose `Assets/_Project/Art` and name it `FloorTile`.
4. In the Project window, select `FloorTile` and duplicate it (`Cmd+D`). Rename the copy `WallTile`.
5. Select each tile and set its **Color** in the Inspector, for example dark grey for floor
   and a lighter or brown colour for walls.

**Make the Tilemaps**
6. In the Hierarchy, right-click and choose `2D Object > Tilemap > Rectangular`. This creates
   `Grid` with a child `Tilemap`. Rename the child `Floor`.
7. Right-click `Grid` and choose `2D Object > Tilemap > Rectangular` again. Rename it `Walls`.
8. Select `Walls`:
   - set **TilemapRenderer → Order in Layer** = `1`
   - **Add Component** `TilemapCollider2D`, and tick **Used By Composite**
   - **Add Component** `CompositeCollider2D`. This auto-adds a Rigidbody2D, so set
     its **Body Type** to **Static**.
9. Select **Player**, and on its **SpriteRenderer** set **Order in Layer** = `10` so it draws
   above the floor.

**Connect the script**
10. In the Hierarchy, right-click and choose **Create Empty**. Name it `Map`, then **Add Component** `MapRenderer`.
11. Fill in its fields: drag `Floor` and `Walls` from the Hierarchy, `FloorTile` and `WallTile`
    from the Project window, and `Player` from the Hierarchy.
12. Select `Main Camera` and set **Size** to about `7` so the whole room fits.

### Write it
Open `MapRenderer.cs` and complete **TODO C1 to C3**.

Tip: you don't need to press Play to test. Click the **⋮** on the MapRenderer component
and choose **Build Map**. The map appears in the Scene view immediately. The Console also
prints the map as text (`#` and `.`) using the `ToString()` you saw in `MapGrid`.

### Done when
Press Play. You start in the middle of a walled room, can walk to every wall and can't
walk through any of them, and the camera follows you smoothly. 🎉 Commit and push!

---

## Challenges (optional, no solutions; design them yourself)

1. **Pillars.** In `CreateTestRoom`, add a few 2×2 wall pillars inside the room.
2. **Two rooms and a corridor.** Carve two rooms and connect them with a 1-tile-wide
   hallway. (This is a preview of lesson 3.)
3. **Neighbour debugger.** In the Sandbox, write a script that, when you click the mouse,
   converts the mouse position to a grid cell (`Camera.main.ScreenToWorldPoint`, then
   `wallTilemap.WorldToCell`) and `Debug.Log`s the cell, its type, and how many wall
   neighbours it has.
4. **Wall-only-where-needed.** Right now every non-floor cell gets a wall tile, even ones deep
   in solid rock that the player can never see. Change `Draw` to only place a wall tile if
   it has at least one floor neighbour (8-way). Use `CountNeighbors`!
5. **Camera bounds.** Stop the camera from showing past the edges of the map. (Hint:
   `Mathf.Clamp`, and the camera's `orthographicSize` and `aspect`.)

---

## Key terms from this lesson

| Term | Meaning |
|---|---|
| `LateUpdate` | Runs after every `Update` each frame. Use it for cameras. |
| `ref` | Pass a variable itself so the method can change it. |
| 2D array `T[,]` | A table of values indexed by `[x, y]`. |
| Bounds check | Making sure an index is inside the array before using it. |
| Sentinel value | A safe default returned for "outside," to avoid special cases. |
| Neighbourhood (4/8) | The cells touching a cell, without or with diagonals. |
| Half-open range | Includes the start, excludes the end: `[min, max)`. |
| Guard clause | An early check that rejects bad input. |
| Data/view separation | Keep game state in plain classes; draw it from separate components. |
| Unit test | Code that automatically checks other code. |
| Assembly definition | Groups scripts into their own compiled unit. |
