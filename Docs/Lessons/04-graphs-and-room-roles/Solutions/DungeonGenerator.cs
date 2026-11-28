using System.Collections.Generic;
using UnityEngine;

namespace Roguelite.Dungeon
{
    // Everything a generator produces, bundled together so a method can return it all at once.
    public class DungeonResult
    {
        public MapGrid Grid;
        public List<RectInt> Rooms = new List<RectInt>();   // empty for caves
        public Vector2Int Spawn;                             // the cell the player starts on

        // Lesson 4: which rooms are joined by a corridor, and what each room is for.
        public RoomGraph Graph;                              // null for caves
        public RoomType[] Roles;                             // Roles[i] is the role of Rooms[i]; null for caves
    }

    // LESSON 4 SOLUTION: GenerateBsp connects rooms with a minimum spanning tree plus a few
    // loops, instead of the Lesson 3 chain, and gives every room a role.
    public static class DungeonGenerator
    {
        public static Vector2Int Center(RectInt room) =>
            new Vector2Int(room.xMin + room.width / 2, room.yMin + room.height / 2);

        public static DungeonResult GenerateBsp(int width, int height, int seed,
            int minLeafSize = 10, int margin = 1, int minRoomSize = 4, int corridorThickness = 2,
            int loopMaxDistance = 20, double loopChance = 0.25)
        {
            if (minLeafSize < minRoomSize + 2 * margin)
                throw new System.ArgumentException("minLeafSize must be at least minRoomSize + 2 * margin, or a leaf could be too small for a room");
            if (width - 2 < minLeafSize || height - 2 < minLeafSize)
                throw new System.ArgumentException($"A {width}x{height} map is too small for minLeafSize {minLeafSize}");

            var result = new DungeonResult();

            // Lesson 3 D1
            result.Grid = new MapGrid(width, height);
            var rng = new System.Random(seed);
            List<RectInt> leaves = Bsp.Split(new RectInt(1, 1, width - 2, height - 2), minLeafSize, rng);

            // Lesson 3 D2
            foreach (RectInt leaf in leaves)
            {
                RectInt room = Bsp.PlaceRoom(leaf, margin, minRoomSize, rng);
                result.Grid.FillRect(room, TileType.Floor);
                result.Rooms.Add(room);
            }

            // SOLUTION E1 (replaces the Lesson 3 D3 chain)
            // (1) The centre of every room, in the same order as Rooms, so index i means room i everywhere.
            var centres = new List<Vector2Int>();
            foreach (RectInt room in result.Rooms)
                centres.Add(Center(room));

            // (2) The cheapest set of corridors that still connects every room, plus a few loops.
            result.Graph = SpanningTree.Prim(centres);
            Loops.AddExtra(result.Graph, centres, loopMaxDistance, loopChance, rng);

            // (3) Carve one L-shaped corridor per edge.
            foreach ((int a, int b) in result.Graph.Edges())
            {
                bool horizontalFirst = rng.Next(2) == 0;
                Corridors.CarveL(result.Grid, centres[a], centres[b], horizontalFirst, corridorThickness);
            }

            // (4) Roles: room 0 is the spawn; the rest follow from the graph.
            result.Roles = RoomRoles.Assign(result.Graph, 0, rng);

            // Lesson 3 D4
            result.Spawn = Center(result.Rooms[0]);

            return result;
        }

        public static DungeonResult GenerateCave(int width, int height, int seed, int steps)
        {
            var result = new DungeonResult();
            result.Grid = new MapGrid(width, height);
            var rng = new System.Random(seed);
            var start = new Vector2Int(width / 2, height / 2);
            RandomWalk.Carve(result.Grid, rng, start, steps);
            result.Spawn = start;
            return result;
        }
    }
}
