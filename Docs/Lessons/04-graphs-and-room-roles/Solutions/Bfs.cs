using System.Collections.Generic;

namespace Roguelite.Dungeon
{
    // LESSON 4 - EXERCISE C: BREADTH-FIRST SEARCH on the room graph.  (SOLUTION)
    public static class Bfs
    {
        public static int[] Distances(RoomGraph graph, int start)
        {
            if (start < 0 || start >= graph.RoomCount)
                throw new System.ArgumentOutOfRangeException(nameof(start), $"room {start} doesn't exist");

            // SOLUTION C1
            int[] distance = new int[graph.RoomCount];
            for (int i = 0; i < distance.Length; i++)
                distance[i] = -1;
            distance[start] = 0;
            var queue = new Queue<int>();
            queue.Enqueue(start);

            // SOLUTION C2
            while (queue.Count > 0)
            {
                int room = queue.Dequeue();
                foreach (int next in graph.Neighbours(room))
                {
                    if (distance[next] == -1)
                    {
                        distance[next] = distance[room] + 1;
                        queue.Enqueue(next);
                    }
                }
            }

            return distance;
        }

        public static int Farthest(RoomGraph graph, int start)
        {
            int[] distance = Distances(graph, start);

            // SOLUTION C3
            int best = start;
            for (int i = 0; i < distance.Length; i++)
                if (distance[i] > distance[best])
                    best = i;
            return best;
        }
    }
}
