using System.Collections.Generic;

namespace Roguelite.Dungeon
{
    // LESSON 4 - EXERCISE A: a graph of rooms, stored as an ADJACENCY LIST.
    //
    // A GRAPH is a set of VERTICES (here: rooms, numbered 0, 1, 2 ...) and EDGES (here: corridors).
    // Each edge joins two rooms. Our graph is UNDIRECTED: a corridor from 2 to 5 is also a
    // corridor from 5 to 2.
    //
    // ADJACENCY LIST: for every room, a list of the rooms it's joined to (its NEIGHBOURS).
    //
    //     0 - 1        neighbours[0] = { 1, 2 }
    //     |            neighbours[1] = { 0 }
    //     2 - 3        neighbours[2] = { 0, 3 }
    //                  neighbours[3] = { 2 }
    //
    // Every edge appears TWICE (in both rooms' lists). That's what makes it undirected.
    //
    // List<List<int>> is a list of lists: like std::vector<std::vector<int>> in C++.
    //
    // Check your work: the tests start with "A" in GraphTests.cs.
    public class RoomGraph
    {
        // neighbours[i] is the list of rooms joined to room i.
        // "readonly" means this field can only be assigned in the constructor (like a C++
        // const member pointer: the list itself can still change, the field can't be replaced).
        private readonly List<List<int>> neighbours;

        // How many rooms (vertices). "=>" makes it a read-only property computed on demand.
        public int RoomCount => neighbours.Count;

        // How many edges. Anyone can read it, only this class can change it ("private set").
        public int EdgeCount { get; private set; }

        public RoomGraph(int roomCount)
        {
            // Guard clause (done for you).
            if (roomCount < 0)
                throw new System.ArgumentException($"roomCount can't be negative, got {roomCount}");

            // TODO A1: Create the outer list and give it ONE EMPTY inner list per room.
            //          (1) neighbours = new List<List<int>>();
            //          (2) a for loop that runs roomCount times and does neighbours.Add(new List<int>());
            //          Then delete the line below.
            neighbours = null;
        }

        // Throws if `id` isn't a real room. (Done for you: every public method calls it first.)
        // nameof(id) turns the parameter's NAME into the string "id", so the message stays right
        // even if you rename the parameter.
        private void CheckId(int id)
        {
            if (id < 0 || id >= RoomCount)
                throw new System.ArgumentOutOfRangeException(nameof(id), $"room {id} doesn't exist (the graph has {RoomCount} rooms)");
        }

        // True if rooms a and b are joined by an edge.
        public bool HasEdge(int a, int b)
        {
            CheckId(a);
            CheckId(b);

            // TODO A2: Look in a's neighbour list for b. List<T>.Contains(item) returns true if
            //          the item is in the list. (Checking one list is enough: every edge is stored
            //          in both, so if b is in a's list, a is in b's.)
            //          Then delete the line below.
            throw new System.NotImplementedException("TODO A2");
        }

        // Joins rooms a and b. Returns true if a NEW edge was added, false if it was already there.
        public bool AddEdge(int a, int b)
        {
            CheckId(a);
            CheckId(b);

            // TODO A3: (1) If a == b, throw an ArgumentException: a corridor from a room to itself
            //              (a "self-loop") makes no sense here.
            //          (2) If the edge already exists (use your HasEdge), return false and change nothing.
            //          (3) Otherwise add b to a's list AND a to b's list, add 1 to EdgeCount,
            //              and return true.
            //          Then delete the line below.
            throw new System.NotImplementedException("TODO A3");
        }

        // The rooms joined to `id`.
        //
        // IReadOnlyList<int> is an INTERFACE: "something you can index and count, but not change".
        // A List<int> IS one, so you can return neighbours[id] directly. The caller can read it
        // and loop over it, but can't call Add or Remove on it, so nobody outside this class can
        // break the "every edge is stored twice" rule. (Like returning a const std::vector<int>&.)
        public IReadOnlyList<int> Neighbours(int id)
        {
            CheckId(id);

            // TODO A4: Return room id's neighbour list. Then delete the line below.
            throw new System.NotImplementedException("TODO A4");
        }

        // The DEGREE of a room: how many edges touch it. A room with degree 1 is a DEAD END.
        public int Degree(int id)
        {
            CheckId(id);

            // TODO A4: Return how many neighbours room id has (a List's size is .Count).
            //          Then delete the line below.
            throw new System.NotImplementedException("TODO A4");
        }

        // Every edge exactly once, as a pair (a, b) with a < b. (Done for you.)
        //
        // (int a, int b) is a TUPLE: a small unnamed bundle of values, like std::pair<int,int>
        // but with names. Make one with (2, 5), read it with .a and .b, or unpack it straight into
        // two variables: foreach ((int a, int b) in graph.Edges()) { ... }
        //
        // The "a < b" check skips the second copy of each edge (5 -> 2 is the same as 2 -> 5).
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
