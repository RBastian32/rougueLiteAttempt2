using System.Collections.Generic;
using UnityEngine;

namespace Roguelite.Dungeon
{
    // LESSON 4 - EXERCISE D1: add a few extra edges to the spanning tree, so it has LOOPS.
    //
    // A tree has exactly one route between any two rooms. That's tidy, but it plays like a maze:
    // lots of dead ends and backtracking. Adding back a FEW short edges creates loops (CYCLES):
    // alternate routes, shortcuts, and a way round a dangerous room. Only SHORT edges, though:
    // a long one would cut across the map, which is the Lesson 3 problem we just fixed.
    //
    // Check your work: the tests start with "D1" in GraphTests.cs.
    public static class Loops
    {
        // For every PAIR of rooms that (1) isn't already joined and (2) is at most maxDistance
        // apart, add the edge with probability `chance` (0.0 = never, 1.0 = always).
        // Returns how many edges were added.
        public static int AddExtra(RoomGraph graph, IReadOnlyList<Vector2Int> centres,
            int maxDistance, double chance, System.Random rng)
        {
            // Guard clause (done for you).
            if (centres.Count != graph.RoomCount)
                throw new System.ArgumentException($"{centres.Count} centres for {graph.RoomCount} rooms");

            // TODO D1: Visit every pair ONCE with two nested loops:
            //              for a from 0 to RoomCount - 1, for b from a + 1 to RoomCount - 1
            //          (starting b at a + 1 skips a == b and skips the reversed pair b, a).
            //          For each pair:
            //            - skip it if graph.HasEdge(a, b)
            //            - skip it if SpanningTree.Distance(centres[a], centres[b]) > maxDistance
            //            - otherwise roll the dice: rng.NextDouble() returns a random double in
            //              [0.0, 1.0), so "rng.NextDouble() < chance" is true with probability `chance`.
            //              If it's true, add the edge and count it.
            //          IMPORTANT for "same seed, same dungeon": only call rng.NextDouble() for pairs
            //          that passed both checks, in this exact loop order.
            //          Return the count, and delete the line below.
            throw new System.NotImplementedException("TODO D1");
        }
    }
}
