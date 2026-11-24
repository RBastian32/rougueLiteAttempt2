using UnityEngine;

namespace Roguelite.Dungeon
{
    // LESSON 3 - EXERCISE B: Corridors.
    //
    // A corridor joins two POINTS (for example two room centres). The simplest corridor that
    // always works is an "L": go straight across, then straight up or down (or the other way
    // round). Two straight lines, one turn.
    //
    // POINTS ARE INCLUSIVE. A corridor from x = 2 to x = 6 covers 2, 3, 4, 5 AND 6, because
    // both ends must be dug or the corridor won't reach the room. (Compare RectInt, which is
    // half-open: xMax is NOT included. Points vs rectangles is a classic source of off-by-one bugs.)
    //
    // Check your work: the tests start with "B" in GenerationTests.cs.
    public static class Corridors
    {
        // Carves a horizontal line of Floor on row y, from x1 to x2 INCLUSIVE.
        // x1 may be bigger than x2 (the corridor might run right-to-left).
        //
        // "int thickness = 1" is an OPTIONAL PARAMETER: callers may leave it out, and then
        // it's 1. CarveHorizontal(grid, 2, 6, 3) and CarveHorizontal(grid, 2, 6, 3, 1) are the same.
        public static void CarveHorizontal(MapGrid grid, int x1, int x2, int y, int thickness = 1)
        {
            // SOLUTION B1
            int left = Mathf.Min(x1, x2);
            int right = Mathf.Max(x1, x2);
            grid.FillRect(new RectInt(left, y - thickness / 2, right - left + 1, thickness), TileType.Floor);
        }

        // The same, but vertical: column x, from y1 to y2 inclusive.
        public static void CarveVertical(MapGrid grid, int y1, int y2, int x, int thickness = 1)
        {
            // SOLUTION B2
            int bottom = Mathf.Min(y1, y2);
            int top = Mathf.Max(y1, y2);
            grid.FillRect(new RectInt(x - thickness / 2, bottom, thickness, top - bottom + 1), TileType.Floor);
        }

        // Carves an L-shaped corridor from `from` to `to`.
        //
        //   horizontalFirst = true:            horizontalFirst = false:
        //
        //             to                                  ┌──── to
        //             │                                   │
        //   from ─────┘  (turns at to.x, from.y)      from    (turns at from.x, to.y)
        public static void CarveL(MapGrid grid, Vector2Int from, Vector2Int to, bool horizontalFirst, int thickness = 1)
        {
            // SOLUTION B3
            if (horizontalFirst)
            {
                CarveHorizontal(grid, from.x, to.x, from.y, thickness);
                CarveVertical(grid, from.y, to.y, to.x, thickness);
            }
            else
            {
                CarveVertical(grid, from.y, to.y, from.x, thickness);
                CarveHorizontal(grid, from.x, to.x, to.y, thickness);
            }
        }
    }
}
