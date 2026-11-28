using System.Collections.Generic;
using UnityEngine;

namespace Roguelite.Dungeon
{
    // LESSON 4 - EXERCISE D1: add a few extra edges to the spanning tree, so it has LOOPS.  (SOLUTION)
    public static class Loops
    {
        public static int AddExtra(RoomGraph graph, IReadOnlyList<Vector2Int> centres,
            int maxDistance, double chance, System.Random rng)
        {
            if (centres.Count != graph.RoomCount)
                throw new System.ArgumentException($"{centres.Count} centres for {graph.RoomCount} rooms");

            // SOLUTION D1
            int added = 0;
            for (int a = 0; a < graph.RoomCount; a++)
            {
                for (int b = a + 1; b < graph.RoomCount; b++)
                {
                    if (graph.HasEdge(a, b))
                        continue;
                    if (SpanningTree.Distance(centres[a], centres[b]) > maxDistance)
                        continue;
                    if (rng.NextDouble() < chance)
                    {
                        graph.AddEdge(a, b);
                        added++;
                    }
                }
            }
            return added;
        }
    }
}
