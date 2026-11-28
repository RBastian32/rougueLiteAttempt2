using System.Collections.Generic;

namespace Roguelite.Dungeon
{
    // LESSON 4 - EXERCISE C: BREADTH-FIRST SEARCH (BFS) on the room graph.
    //
    // BFS answers "how many corridors do I walk through to get from the start to each room?"
    // It explores in LAYERS: first the start (distance 0), then all its neighbours (distance 1),
    // then THEIR new neighbours (distance 2), and so on. A QUEUE (first in, first out) keeps the
    // order right: rooms found earlier are explored earlier, so the nearer layers always finish
    // before the farther ones start.
    //
    //   distance[start] = 0, put start in the queue
    //   while the queue isn't empty:
    //       room = take the front of the queue
    //       for each neighbour of room:
    //           if neighbour hasn't been reached yet:
    //               distance[neighbour] = distance[room] + 1
    //               put neighbour at the back of the queue
    //
    // Queue<T> is C#'s queue, like std::queue<T>: Enqueue(x) adds to the back, Dequeue() removes
    // and returns the front, .Count is how many are waiting.
    //
    // Check your work: the tests start with "C" in GraphTests.cs.
    public static class Bfs
    {
        // distance[i] = the fewest edges from `start` to room i, or -1 if room i can't be reached.
        public static int[] Distances(RoomGraph graph, int start)
        {
            // Guard clause (done for you).
            if (start < 0 || start >= graph.RoomCount)
                throw new System.ArgumentOutOfRangeException(nameof(start), $"room {start} doesn't exist");

            // TODO C1: SET UP.
            //          (1) int[] distance with one entry per room, every entry -1 ("not reached yet").
            //              -1 also works as the "visited" check: a room is visited once its
            //              distance isn't -1, so you don't need a separate visited array.
            //          (2) distance[start] = 0
            //          (3) var queue = new Queue<int>(); and Enqueue the start.

            // TODO C2: THE LOOP. Follow the pseudocode above. graph.Neighbours(room) gives the
            //          rooms to look at. Then return distance, and delete the line below.
            throw new System.NotImplementedException("TODO C1/C2");
        }

        // The room with the LARGEST distance from `start` (the end of the longest route).
        // Ties go to the lowest room id. With only one room, that's `start` itself.
        public static int Farthest(RoomGraph graph, int start)
        {
            int[] distance = Distances(graph, start);

            // TODO C3: Find the index with the biggest distance. Start with best = start, and
            //          take i whenever distance[i] > distance[best] (">" keeps the lowest id on a
            //          tie). Unreachable rooms are -1, so they never win.
            //          Then delete the line below.
            throw new System.NotImplementedException("TODO C3");
        }
    }
}
