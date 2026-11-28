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

            // TODO D1: Create result.Grid (width x height, starts as all Wall) and ONE
            //          System.Random from the seed. Split the area inside the 1-tile border,
            //          new RectInt(1, 1, width - 2, height - 2), into leaves with Bsp.Split.
            result.Grid = new MapGrid(width, height);
            System.Random rng = new System.Random(seed);
            List<RectInt> leaves = Bsp.Split(new RectInt(1, 1, width - 2, height - 2), minLeafSize, rng);

            // TODO D2: For each leaf: place a room in it (Bsp.PlaceRoom), carve the room
            //          as Floor, and add it to result.Rooms.
            for(int i = 0; i < leaves.Count; i++)
            {
                RectInt room = Bsp.PlaceRoom(leaves[i], margin, minRoomSize, rng);
                result.Grid.FillRect(room, TileType.Floor);
                result.Rooms.Add(room);
            }

            // TODO D3: Join each room to the NEXT one in the list (room 0 → 1, 1 → 2, ...)
            //          with Corridors.CarveL between their centres. Pick horizontalFirst
            //          randomly (rng.Next(2) == 0) and pass corridorThickness.
            //          Careful with the loop bound: the last room has no "next" room.
            for(int i = 0; i < result.Rooms.Count - 1; i++)
            {
                Vector2Int from = Center(result.Rooms[i]);
                Vector2Int to = Center(result.Rooms[i + 1]);
                bool horizontalFirst = rng.Next(2) == 0;
                Corridors.CarveL(result.Grid, from, to, horizontalFirst, corridorThickness);
            }
            // TODO D4: The player spawns in the centre of the first room.
            //          Set result.Spawn.
            result.Spawn = Center(result.Rooms[0]);

            return result;
        }

        // A cave: one random walk from the middle of the map.
        public static DungeonResult GenerateCave(int width, int height, int seed, int steps)
        {
            var result = new DungeonResult();

            // TODO D5: Create result.Grid and a System.Random from the seed. Start in the
            //          middle of the map, run RandomWalk.Carve, and spawn the player at
            //          the start cell.
            result.Grid = new MapGrid(width, height);
            System.Random rng = new System.Random(seed);
            Vector2Int start = new Vector2Int(width / 2, height / 2);
            RandomWalk.Carve(result.Grid, rng, start, steps);
            result.Spawn = start;

            return result;
        }
    }
}
