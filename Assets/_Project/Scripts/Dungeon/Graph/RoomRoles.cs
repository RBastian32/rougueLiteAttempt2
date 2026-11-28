using System.Collections.Generic;

namespace Roguelite.Dungeon
{
    // What a room is for. (Done for you.) The first value is the default: a new RoomType[]
    // array starts with every entry = Normal, just like a new int[] starts full of 0.
    public enum RoomType { Normal, Spawn, Boss, Treasure, Shop }

    // LESSON 4 - EXERCISES D2-D4: give every room a ROLE from what the graph tells us.
    //
    // The rules, in this order (each step only changes rooms that are still Normal):
    //   D2  Spawn    = the room the player starts in (passed in; the generator uses room 0)
    //       Boss     = the room FARTHEST from the spawn, measured by BFS (Exercise C)
    //   D3  Treasure = every DEAD END (degree 1): a reward for exploring a side branch
    //   D4  Shop     = ONE of the rooms that are still Normal, picked at random
    //
    // Check your work: the tests start with "D2", "D3" and "D4" in GraphTests.cs.
    public static class RoomRoles
    {
        // roles[i] is the role of room i.
        public static RoomType[] Assign(RoomGraph graph, int spawn, System.Random rng)
        {
            // Guard clause (done for you).
            if (spawn < 0 || spawn >= graph.RoomCount)
                throw new System.ArgumentOutOfRangeException(nameof(spawn), $"room {spawn} doesn't exist");

            var roles = new RoomType[graph.RoomCount];   // all Normal

            // TODO D2: SPAWN AND BOSS.
            //          (1) roles[spawn] = RoomType.Spawn;
            //          (2) int boss = Bfs.Farthest(graph, spawn);
            //          (3) Only if boss != spawn, set roles[boss] = RoomType.Boss.
            //              (With a single room, Farthest returns the spawn itself, and the
            //              spawn must stay the spawn.)

            // TODO D3: TREASURE. Loop over every room i. If roles[i] is still Normal AND
            //          graph.Degree(i) == 1, make it Treasure.

            // TODO D4: SHOP. (1) Collect the ids of every room that is still Normal into a List<int>.
            //          (2) If the list isn't empty, pick one at random: list[rng.Next(list.Count)]
            //              and make that room the Shop.
            //          Only call rng.Next when the list isn't empty (rng.Next(0) would throw).

            return roles;
        }
    }
}
