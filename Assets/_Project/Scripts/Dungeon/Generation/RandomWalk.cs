using UnityEngine;

namespace Roguelite.Dungeon
{
    // LESSON 3 - EXERCISE A: The random walk ("drunkard's walk").
    //
    // Imagine someone stumbling around a block of solid rock with a pickaxe. Each step they
    // pick a random direction (up, right, down or left), move one cell, and dig. After a
    // few thousand steps the dug-out cells form a twisty, organic CAVE.
    //
    // "static class" = a class you never create with `new`. It's just a home for related
    // methods, like a C++ namespace full of free functions. You call them on the class
    // name itself: RandomWalk.Carve(...).
    //
    // Check your work: Window > General > Test Runner > EditMode > Run All.
    // The tests for this exercise start with "A" in GenerationTests.cs.
    public static class RandomWalk
    {
        // Returns the nearest cell to p that is INSIDE the 1-tile border, so x stays in
        // 1 .. Width - 2 and y stays in 1 .. Height - 2. The border ring is never dug.
        public static Vector2Int ClampToInterior(MapGrid grid, Vector2Int p)
        {
            // p is the proposed tile. Index 0 and the last index (size - 1) are walls,
            // so clamp each coordinate to 1 through size - 2 along its own axis.
            // Return the adjusted x and y together as the tile the walker can carve.
            int x = Mathf.Clamp(p.x, 1, grid.Width - 2);
            int y = Mathf.Clamp(p.y, 1, grid.Height - 2);

            return new Vector2Int(x, y);
        }

        // Digs a cave into `grid`: starts at `start` and takes `steps` random steps,
        // turning every cell it visits into Floor.
        //
        // The random numbers come from `rng`, which the CALLER passes in. That's what makes
        // the cave repeatable: the same seed gives the same sequence of numbers, so the
        // same steps, so the same cave. (Lecture Unit 2.)
        //
        // Why "System.Random" in full? Unity ALSO has a class called Random (UnityEngine.Random).
        // Writing the namespace makes it clear which one we mean.
        public static void Carve(MapGrid grid, System.Random rng, Vector2Int start, int steps)
        {
            // Guard clause (done for you): the interior must be at least 1 x 1, so the grid
            // needs to be at least 3 x 3. Otherwise ClampToInterior has nowhere to put us.
            if (grid.Width < 3 || grid.Height < 3)
                throw new System.ArgumentException($"Random walk needs a grid of at least 3x3, got {grid.Width}x{grid.Height}");

            // TODO A2: (1) Start at ClampToInterior(grid, start) and make that cell Floor.
            //          (2) Repeat `steps` times:
            //              - pick a random direction from MapGrid.Directions4
            //                (rng.Next(n) returns a random int from 0 to n - 1)
            //              - move one cell that way, then clamp back inside the border
            //              - make the new cell Floor
            //          Vector2Int supports +, so `pos + dir` is the neighbouring cell.
        }
    }
}
