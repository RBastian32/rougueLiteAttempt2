using System.Collections.Generic;

namespace Roguelite.Dungeon
{
    // LESSON 4 - EXERCISE A: a graph of rooms, stored as an ADJACENCY LIST.  (SOLUTION)
    public class RoomGraph
    {
        private readonly List<List<int>> neighbours;

        public int RoomCount => neighbours.Count;
        public int EdgeCount { get; private set; }

        public RoomGraph(int roomCount)
        {
            if (roomCount < 0)
                throw new System.ArgumentException($"roomCount can't be negative, got {roomCount}");

            // SOLUTION A1
            neighbours = new List<List<int>>();
            for (int i = 0; i < roomCount; i++)
                neighbours.Add(new List<int>());
        }

        private void CheckId(int id)
        {
            if (id < 0 || id >= RoomCount)
                throw new System.ArgumentOutOfRangeException(nameof(id), $"room {id} doesn't exist (the graph has {RoomCount} rooms)");
        }

        public bool HasEdge(int a, int b)
        {
            CheckId(a);
            CheckId(b);

            // SOLUTION A2
            return neighbours[a].Contains(b);
        }

        public bool AddEdge(int a, int b)
        {
            CheckId(a);
            CheckId(b);

            // SOLUTION A3
            if (a == b)
                throw new System.ArgumentException($"a room can't be connected to itself (room {a})");
            if (HasEdge(a, b))
                return false;
            neighbours[a].Add(b);
            neighbours[b].Add(a);
            EdgeCount++;
            return true;
        }

        public IReadOnlyList<int> Neighbours(int id)
        {
            CheckId(id);

            // SOLUTION A4
            return neighbours[id];
        }

        public int Degree(int id)
        {
            CheckId(id);

            // SOLUTION A4
            return neighbours[id].Count;
        }

        public List<(int a, int b)> Edges()
        {
            var edges = new List<(int a, int b)>();
            for (int a = 0; a < RoomCount; a++)
                foreach (int b in neighbours[a])
                    if (a < b)
                        edges.Add((a, b));
            return edges;
        }
    }
}
