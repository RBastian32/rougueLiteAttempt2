using System.Collections.Generic;
using UnityEngine;

namespace Roguelite.Dungeon
{
    // LESSON 4 - EXERCISE B: the MINIMUM SPANNING TREE (MST), with PRIM'S ALGORITHM.
    //
    // Imagine a corridor could join ANY two rooms, and its cost is its length. A SPANNING TREE
    // picks just enough corridors to connect every room (n rooms → n - 1 corridors, no loops).
    // The MINIMUM spanning tree is the one with the smallest total length: the cheapest way to
    // connect everything. Short corridors between close rooms; no corridors across the map.
    //
    // PRIM'S ALGORITHM grows the tree one room at a time:
    //   start with room 0 in the tree
    //   repeat until every room is in the tree:
    //       find the room NOT in the tree that is CLOSEST to some room IN the tree
    //       add it, plus the edge that reaches it
    //
    // To find "closest" quickly, each room outside the tree remembers two numbers:
    //   bestCost[u] = the length of the shortest edge from the tree to u found so far
    //   bestFrom[u] = which tree room that edge comes from (-1 = none yet)
    // Every time a room v joins the tree, it may offer a shorter edge to the rooms still outside,
    // so we update their bestCost/bestFrom.
    //
    // Check your work: the tests start with "B" in GraphTests.cs.
    public static class SpanningTree
    {
        // MANHATTAN DISTANCE: how far apart two cells are if you can only move along x and y
        // (like our L-shaped corridors). |dx| + |dy|.
        public static int Distance(Vector2Int a, Vector2Int b)
        {
            // TODO B1: Return |a.x - b.x| + |a.y - b.y|. Mathf.Abs(n) gives the absolute value.
            //          Then delete the line below.
            throw new System.NotImplementedException("TODO B1");
        }

        // Returns the minimum spanning tree of the rooms at `centres`, as a RoomGraph.
        // centres[i] is the centre of room i, so vertex i in the graph is room i.
        public static RoomGraph Prim(IReadOnlyList<Vector2Int> centres)
        {
            int n = centres.Count;
            var tree = new RoomGraph(n);   // n rooms, no edges yet
            if (n == 0)
                return tree;

            // TODO B2: SET UP three arrays of size n. (C# arrays: int[] a = new int[n];
            //          they start full of 0 / false, and you index them like C++ arrays.)
            //          - bool[] inTree       all false: nothing is in the tree yet
            //          - int[]  bestCost     every entry int.MaxValue ("infinitely far")
            //          - int[]  bestFrom     every entry -1 ("no edge yet")
            //          Then set bestCost[0] = 0, so room 0 is picked first.

            // The main loop: each pass adds ONE room to the tree, so n passes add them all.
            for (int step = 0; step < n; step++)
            {
                // TODO B3: PICK the room to add: the index v with the SMALLEST bestCost among
                //          the rooms that are NOT inTree yet.
                //          Start with int v = -1, loop over every i, and take i if it's not in the
                //          tree and (v == -1 || bestCost[i] < bestCost[v]).
                //          Use "<", not "<=", so ties go to the LOWER index (the tests expect it).

                // TODO B4: ADD v to the tree, then UPDATE the rooms still outside.
                //          (1) inTree[v] = true;
                //          (2) If bestFrom[v] != -1, add the edge (bestFrom[v], v) to `tree`.
                //              (Room 0 has no bestFrom: it's the root, so it adds no edge.)
                //          (3) For every room u NOT in the tree: work out d = Distance(centres[v], centres[u]).
                //              If d < bestCost[u], v offers a shorter way to reach u:
                //              set bestCost[u] = d and bestFrom[u] = v.
            }

            return tree;
        }
    }
}
