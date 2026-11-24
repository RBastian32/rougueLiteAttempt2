using System.Collections.Generic;
using UnityEngine;

namespace Roguelite.Dungeon
{
    // Everything a generator produces, bundled together so a method can return it all at once.
    // It's a class with public FIELDS (no logic): a "plain data" object, like a C struct.
    public class DungeonResult
    {
        public MapGrid Grid;
        public List<RectInt> Rooms = new List<RectInt>();   // empty for caves
        public Vector2Int Spawn;                             // the cell the player starts on
    }

    // LESSON 3 - EXERCISE D: Putting the pieces together.
    //
    // A generator is a PURE function of its inputs: the same width, height, seed and settings
    // always give exactly the same dungeon. All the randomness comes from ONE System.Random,
    // created from the seed, and passed to every step that needs random numbers.
    //
    // Check your work: the tests start with "D" in GenerationTests.cs.
    public static class DungeonGenerator
    {
        // The middle cell of a room. (Done for you.)
        // "=>" makes this an expression-bodied method: its whole body is the expression.
        public static Vector2Int Center(RectInt room) =>
            new Vector2Int(room.xMin + room.width / 2, room.yMin + room.height / 2);

        // A BSP dungeon: rooms in a binary tree of leaves, joined in order by L-shaped corridors.
        public static DungeonResult GenerateBsp(int width, int height, int seed,
            int minLeafSize = 10, int margin = 1, int minRoomSize = 4, int corridorThickness = 2)
        {
            // Guard clauses (done for you). Read them: they say which settings can't work.
            if (minLeafSize < minRoomSize + 2 * margin)
                throw new System.ArgumentException("minLeafSize must be at least minRoomSize + 2 * margin, or a leaf could be too small for a room");
            if (width - 2 < minLeafSize || height - 2 < minLeafSize)
                throw new System.ArgumentException($"A {width}x{height} map is too small for minLeafSize {minLeafSize}");

            var result = new DungeonResult();

            // SOLUTION D1
            result.Grid = new MapGrid(width, height);
            var rng = new System.Random(seed);
            List<RectInt> leaves = Bsp.Split(new RectInt(1, 1, width - 2, height - 2), minLeafSize, rng);

            // SOLUTION D2
            foreach (RectInt leaf in leaves)
            {
                RectInt room = Bsp.PlaceRoom(leaf, margin, minRoomSize, rng);
                result.Grid.FillRect(room, TileType.Floor);
                result.Rooms.Add(room);
            }

            // SOLUTION D3
            for (int i = 0; i < result.Rooms.Count - 1; i++)
            {
                bool horizontalFirst = rng.Next(2) == 0;
                Corridors.CarveL(result.Grid, Center(result.Rooms[i]), Center(result.Rooms[i + 1]),
                    horizontalFirst, corridorThickness);
            }

            // SOLUTION D4
            result.Spawn = Center(result.Rooms[0]);

            return result;
        }

        // A cave: one random walk from the middle of the map.
        public static DungeonResult GenerateCave(int width, int height, int seed, int steps)
        {
            var result = new DungeonResult();

            // SOLUTION D5
            result.Grid = new MapGrid(width, height);
            var rng = new System.Random(seed);
            var start = new Vector2Int(width / 2, height / 2);
            RandomWalk.Carve(result.Grid, rng, start, steps);
            result.Spawn = start;

            return result;
        }
    }
}
