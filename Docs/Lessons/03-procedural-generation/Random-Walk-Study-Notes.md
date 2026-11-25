# Random Walk Cave Generation

## What the algorithm does

Start with a grid of rock. A walker carves its starting tile, then repeatedly picks a direction, tries to move one tile, and carves the tile it ends up on. Over many moves, the carved tiles form an irregular cave.

This is procedural generation in miniature. Random choices vary the cave’s shape. Clamping preserves the outer wall. The cave stays connected because every newly carved tile shares an edge with a tile the walker already visited. With the same starting conditions and random seed, you can replay the walk to debug it.

## Key terms

- **start** is the requested starting tile. **pos** is the walker’s current tile and updates after each move.
- **p** is the input point inside ClampToInterior. On the first call it is start. On later calls it is the proposed neighbor, pos + direction.
- **Vector2Int** stores two whole-number coordinates. For this grid, x is the column and y is the row.
- **MapGrid.Directions4** holds four one-tile moves. For instance, (1, 0) moves right. rng.Next(4) returns a random index from 0 through 3 because its upper bound is exclusive.

## A1 — Keep the walker inside the walls

On a 10 × 8 grid, columns run from 0 through 9 and rows from 0 through 7. The first and last index on each axis belong to the wall. So the interior is x = 1 through 8 and y = 1 through 6. The last index is size − 1. The last interior index is one earlier, size − 2.

    int x = Mathf.Clamp(p.x, 1, grid.Width - 2);
    int y = Mathf.Clamp(p.y, 1, grid.Height - 2);
    return new Vector2Int(x, y);

Mathf.Clamp(value, min, max) returns the nearest endpoint for an out-of-range value and returns the original value if it’s already in range. The variables x and y save those results. The last line packages them into one coordinate. The caller uses that point to decide which tile to carve.

## A2 — Carve a random walk

    pos = ClampToInterior(grid, start)
    carve pos

    repeat steps times:
        index = rng.Next(4)
        direction = MapGrid.Directions4[index]
        proposed = pos + direction
        pos = ClampToInterior(grid, proposed)
        carve pos

Carve the start before the loop so it becomes floor even when steps is zero. Inside the loop, add the direction to the current position to propose a neighbor. Store the clamped result back in pos so the next move begins where the walker actually landed. In C#, grid.Set(pos.x, pos.y, TileType.Floor) sets that tile to floor.

## Worked example on the 10 × 8 grid

1. The requested start is (4, 7). Row 7 is the border, so clamp to (4, 6) and carve it.
2. The first direction is (0, 1). Adding it proposes (4, 7), so clamping brings the walker back to (4, 6). The walker stays put, and this still counts as a step.
3. The next direction is (1, 0). The proposed tile is (5, 6), which is inside the wall. Update pos to (5, 6) and carve that new tile.

## Where we got stuck

The name p was easy to confuse with the safe point the method returns. Think of p as the tile the walker is asking to use. The returned point is the tile it can actually carve. We also worked through (0, 7), which clamps to (1, 6) because x is below its own range and y is above its own range.

Common mistakes include discarding the values returned by Mathf.Clamp, using Width for both axes, picking the direction once outside the loop, and carving the proposed tile before clamping.

## Self-check

1. On a 10 × 8 grid, what does (4, 7) clamp to?
2. Why is the highest legal x coordinate Width − 2?
3. What indices can rng.Next(4) return?
4. On an all-rock grid, how many tiles get carved when steps is zero?
5. Why can you reach every carved tile from the start?

## Answers

1. (4, 6). The x stays 4 and y comes down to 6.
2. The last column has index Width − 1 and belongs to the wall. The previous column is the last interior column.
3. 0, 1, 2, or 3.
4. One tile, at the clamped starting point.
5. Every new tile is reached by a one-tile move from an already carved tile. A move blocked by the wall stays on the current floor tile.
