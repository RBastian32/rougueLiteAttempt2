# Lab 3 hints

Each TODO has up to three hints. Read one, try again, and only read the next if you're still stuck.

---

## A1: `ClampToInterior`
1. The interior is everything except the outer ring. The smallest x allowed is 1. The largest is the last column *before* the border: `grid.Width - 2`.
2. `Mathf.Clamp(p.x, 1, grid.Width - 2)` gives the clamped x. Do the same for y with `Height`.
3. Return `new Vector2Int(x, y)`. (Changing `p.x` and `p.y` and returning `p` also works: `p` is a parameter, so it's already your own copy of the struct.)

## A2: `Carve`
1. You need one variable that remembers where the walker is: `Vector2Int pos`. Start it at `ClampToInterior(grid, start)` and dig it with `grid.Set(pos.x, pos.y, TileType.Floor)`.
2. Inside a `for` loop that runs `steps` times: `MapGrid.Directions4[rng.Next(MapGrid.Directions4.Length)]` is a random direction.
3. `pos = ClampToInterior(grid, pos + dir);` then dig. If the "same seed" test fails, check you're using the `rng` parameter, not creating a new `System.Random`.

## B1: `CarveHorizontal`
1. `int left = Mathf.Min(x1, x2);` and `int right = Mathf.Max(x1, x2);`.
2. Inclusive count: `right - left + 1`. The bottom row of a thick corridor: `y - thickness / 2`.
3. `grid.FillRect(new RectInt(left, y - thickness / 2, right - left + 1, thickness), TileType.Floor);`

## B2: `CarveVertical`
1. Copy your B1 and swap the axes: `bottom`/`top` from `y1`/`y2`.
2. The RectInt now starts at `x - thickness / 2` across, and `bottom` up. Its width is `thickness` and its height is the inclusive count.

## B3: `CarveL`
1. It's an `if (horizontalFirst) { ... } else { ... }` with two calls in each branch.
2. Horizontal first: `CarveHorizontal(grid, from.x, to.x, from.y, thickness)` then `CarveVertical(grid, from.y, to.y, to.x, thickness)`.
3. If the corner test fails, check which y the horizontal piece uses and which x the vertical piece uses. The corner must be (to.x, from.y) when horizontal comes first.

## C1: base case
1. `if (!canCutVertically && !canCutHorizontally) return new List<RectInt> { area };`

## C2: direction
1. Declare `bool cutVertically;` first, then assign it in each branch.
2. If only one is possible: `cutVertically = canCutVertically;` covers both one-way cases (think about why).
3. If both are possible: `width > height` → true, `height > width` → false, otherwise `rng.Next(2) == 0`.

## C3: where to cut
1. Declare `RectInt first, second;` before the `if`, so both branches can assign them.
2. Vertical: `int cut = rng.Next(minLeafSize, area.width - minLeafSize + 1);`
3. The second half starts where the first ends: `area.xMin + cut`, with width `area.width - cut`. For a horizontal cut, change y and height instead.

## C4: recurse
1. `List<RectInt> leaves = Split(first, minLeafSize, rng);`
2. `leaves.AddRange(Split(second, minLeafSize, rng));` then `return leaves;`. Delete the old `return new List<RectInt>();`.
3. Stack overflow? Your base case isn't being reached: print `area` at the top of `Split` and see whether it's actually getting smaller.

## C5: `PlaceRoom`
1. Guard: `if (maxWidth < minRoomSize || maxHeight < minRoomSize) throw new System.ArgumentException("...");`
2. Width: `rng.Next(minRoomSize, maxWidth + 1)` (the `+ 1` makes maxWidth possible).
3. x: `rng.Next(leaf.xMin + margin, leaf.xMax - margin - w + 1)`. Same pattern for y with `yMin`, `yMax` and `h`.

## D1: setup
1. `result.Grid = new MapGrid(width, height);` and `var rng = new System.Random(seed);`
2. `List<RectInt> leaves = Bsp.Split(new RectInt(1, 1, width - 2, height - 2), minLeafSize, rng);`

## D2: rooms
1. `foreach (RectInt leaf in leaves) { ... }` with three lines inside: PlaceRoom, FillRect, Rooms.Add.

## D3: corridors
1. `for (int i = 0; i < result.Rooms.Count - 1; i++)`: the `- 1` because room i joins room i + 1.
2. `Corridors.CarveL(result.Grid, Center(result.Rooms[i]), Center(result.Rooms[i + 1]), rng.Next(2) == 0, corridorThickness);`
3. If the connectivity test fails for some seeds, check the loop bound and that you join i to **i + 1**, not i to i.

## D4: spawn
1. `result.Spawn = Center(result.Rooms[0]);`

## D5: cave
1. Same first two lines as D1. The start is `new Vector2Int(width / 2, height / 2)`.
2. `RandomWalk.Carve(result.Grid, rng, start, steps);` then `result.Spawn = start;`

## E: MapRenderer
1. The enum goes **outside** the class but **inside** the namespace, so other scripts can use `GeneratorType` too.
2. The ternary from Lesson 2: `int usedSeed = seed != 0 ? seed : System.Environment.TickCount;`
3. Declare `Vector2Int spawn;` **before** the `switch`, and assign it in every case. The compiler refuses to let you use it afterwards if any case could skip it ("use of unassigned local variable").
4. A `DungeonResult` is a class: `DungeonResult r = DungeonGenerator.GenerateBsp(width, height, usedSeed);` then `Grid = r.Grid; spawn = r.Spawn;`.
5. Error "map is too small for minLeafSize"? Set the Map's Width and Height to 60 and 40.
