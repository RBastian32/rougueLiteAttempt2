using System.Collections.Generic;

namespace Roguelite.Dungeon
{
    public enum RoomType { Normal, Spawn, Boss, Treasure, Shop }

    // LESSON 4 - EXERCISE D2-D4: give every room a ROLE from what the graph tells us.  (SOLUTION)
    public static class RoomRoles
    {
        public static RoomType[] Assign(RoomGraph graph, int spawn, System.Random rng)
        {
            if (spawn < 0 || spawn >= graph.RoomCount)
                throw new System.ArgumentOutOfRangeException(nameof(spawn), $"room {spawn} doesn't exist");

            var roles = new RoomType[graph.RoomCount];

            // SOLUTION D2
            roles[spawn] = RoomType.Spawn;
            int boss = Bfs.Farthest(graph, spawn);
            if (boss != spawn)
                roles[boss] = RoomType.Boss;

            // SOLUTION D3
            for (int i = 0; i < graph.RoomCount; i++)
                if (roles[i] == RoomType.Normal && graph.Degree(i) == 1)
                    roles[i] = RoomType.Treasure;

            // SOLUTION D4
            var candidates = new List<int>();
            for (int i = 0; i < graph.RoomCount; i++)
                if (roles[i] == RoomType.Normal)
                    candidates.Add(i);
            if (candidates.Count > 0)
                roles[candidates[rng.Next(candidates.Count)]] = RoomType.Shop;

            return roles;
        }
    }
}
