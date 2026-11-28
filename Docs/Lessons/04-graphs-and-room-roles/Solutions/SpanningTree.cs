using System.Collections.Generic;
using UnityEngine;

namespace Roguelite.Dungeon
{
    // LESSON 4 - EXERCISE B: the MINIMUM SPANNING TREE, with Prim's algorithm.  (SOLUTION)
    public static class SpanningTree
    {
        public static int Distance(Vector2Int a, Vector2Int b)
        {
            // SOLUTION B1
            return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
        }

        public static RoomGraph Prim(IReadOnlyList<Vector2Int> centres)
        {
            int n = centres.Count;
            var tree = new RoomGraph(n);
            if (n == 0)
                return tree;

            // SOLUTION B2
            bool[] inTree = new bool[n];
            int[] bestCost = new int[n];
            int[] bestFrom = new int[n];
            for (int i = 0; i < n; i++)
            {
                bestCost[i] = int.MaxValue;
                bestFrom[i] = -1;
            }
            bestCost[0] = 0;

            for (int step = 0; step < n; step++)
            {
                // SOLUTION B3
                int v = -1;
                for (int i = 0; i < n; i++)
                    if (!inTree[i] && (v == -1 || bestCost[i] < bestCost[v]))
                        v = i;

                // SOLUTION B4
                inTree[v] = true;
                if (bestFrom[v] != -1)
                    tree.AddEdge(bestFrom[v], v);

                for (int u = 0; u < n; u++)
                {
                    if (inTree[u])
                        continue;
                    int d = Distance(centres[v], centres[u]);
                    if (d < bestCost[u])
                    {
                        bestCost[u] = d;
                        bestFrom[u] = v;
                    }
                }
            }

            return tree;
        }
    }
}
