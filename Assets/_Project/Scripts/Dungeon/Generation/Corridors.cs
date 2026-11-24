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
            // TODO B1: (1) Work out the left end and right end with Mathf.Min / Mathf.Max,
            //              so the order of x1 and x2 doesn't matter.
            //          (2) Carve with grid.FillRect(...) and one RectInt:
            //              - it starts at the left end
            //              - its width is the number of cells from left to right INCLUSIVE
            //                (careful: last - first + 1)
            //              - it's `thickness` rows tall, centred on y: the bottom row is
            //                y - thickness / 2 (same trick as the Lesson 2 corridor).
        }

        // The same, but vertical: column x, from y1 to y2 inclusive.
        public static void CarveVertical(MapGrid grid, int y1, int y2, int x, int thickness = 1)
        {
            // TODO B2: The mirror image of B1. Swap the roles of x and y.
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
            // TODO B3: If horizontalFirst:
            //              - horizontal on row from.y, from from.x to to.x
            //              - then vertical on column to.x, from from.y to to.y
            //          Otherwise:
            //              - vertical on column from.x, from from.y to to.y
            //              - then horizontal on row to.y, from from.x to to.x
            //          Pass `thickness` through to both calls.
        }
    }
}
