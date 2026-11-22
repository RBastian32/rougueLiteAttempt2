# Lesson 2 hints

Read only the hint for the step you're stuck on. Each one is a nudge, not the answer.

## Exercise A: CameraFollow

- **A1:** `if (something == null) { return; }`. What's the "something"?
- **A2:** You can build a new position from pieces:
  `new Vector3(xFromHere, yFromHere, zFromThere)`. The target's position is
  `target.position`, and the camera's own position is `transform.position`.
- **A3:** `transform.position = Vector3.SmoothDamp(...)`. The first argument is where the
  camera is *now*. Don't forget the `ref` keyword before `velocity`.
- *Camera doesn't move at all?* Is the Player dragged into the Target slot? Any red errors in
  the Console?
- *Screen goes blank / solid blue?* Your z is wrong. Check A2.

## Exercise B: MapGrid

- **B1:** `if (width < 1 || height < 1) throw new ArgumentException("...");`
  Then set `Width = width;` and the same for Height. Then `tiles = new TileType[?, ?];`.
  Which size goes first if you index `tiles[x, y]`?
- **B2:** It's one `return` statement combining four comparisons with `&&`.
- **B3a:** Use the `InBounds` you just wrote. `if (!InBounds(x, y)) return ...;`
- **B3b:** It's the mirror image of B3a.
- **B4:**
  ```
  for (int x = 0; x < ???; x++)
      for (int y = 0; y < ???; y++)
          ...
  ```
- **B5:** Same shape as B4, but the loops start at `rect.xMin` / `rect.yMin` and stop before
  `rect.xMax` / `rect.yMax`. Call `Set`, not `tiles[...]`, so off-grid cells are skipped.
- **B6a:** Copy the 4 from `Directions4`, then add four more of the form
  `new Vector2Int(±1, ±1)`.
- **B6b:** `Vector2Int[] dirs = includeDiagonals ? Directions8 : Directions4;`
  (the `? :` is the *ternary operator*: a compact if/else that produces a value).
  Then `foreach (Vector2Int d in dirs) { ... }`.
- *Test B3_XAndY_AreNotSwapped fails?* Check that the array size order in B1 and the index
  order in B3 are the same: `[x, y]` both times.
- *Tests don't appear in the Test Runner?* Look for red compile errors in the Console first.
  Nothing runs if the code doesn't compile.

## Exercise C: MapRenderer

- **C1:** A 1-tile border means the floor starts at (1, 1). If the map is 20 wide and you lose
  one tile on each side, how wide is the floor?
- **C2:** `floorTilemap.ClearAllTiles();` Then a double loop like B4 over
  `grid.Width` / `grid.Height`. Use `if (grid.Get(x, y) == TileType.Floor) ... else ...`.
- **C3:** `var cell = new Vector3Int(Grid.Width / 2, Grid.Height / 2, 0);` then
  `player.position = ...`.
- *Map doesn't appear?* Check the Console for the "assign both Tilemaps..." error. Also check
  that both Tilemaps are children of the same `Grid`.
- *Player walks through walls?* The `Walls` Tilemap needs TilemapCollider2D (Used By
  Composite), CompositeCollider2D, and a Static Rigidbody2D. The Player needs a BoxCollider2D.
- *Player is invisible?* It's probably behind the floor. Raise its Order in Layer.
- *Player gets stuck on walls?* Make the player's BoxCollider2D slightly smaller than one tile
  (for example 0.9 × 0.9).
