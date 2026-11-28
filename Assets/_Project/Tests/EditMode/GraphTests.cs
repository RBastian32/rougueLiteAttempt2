using System.Collections.Generic;
using NUnit.Framework;
using Roguelite.Dungeon;
using UnityEngine;

// Tests for Lesson 4. Each test name starts with the TODO it checks (A1_, B3_, D2_ ...).
// Read a test when you're stuck: it's the exact specification of what your code must do.
public class GraphTests
{
    // ---------- helpers ----------

    // Builds a graph from a list of (a, b) pairs, so a test can describe a graph in one line.
    private static RoomGraph MakeGraph(int rooms, params (int a, int b)[] edges)
    {
        var graph = new RoomGraph(rooms);
        foreach ((int a, int b) in edges)
            graph.AddEdge(a, b);
        return graph;
    }

    // True if every room can be reached from room 0. Written independently of your Bfs,
    // so a bug in Exercise C can't hide a bug in Exercise B.
    private static bool IsConnected(RoomGraph graph)
    {
        if (graph.RoomCount == 0) return true;
        var seen = new HashSet<int> { 0 };
        var stack = new Stack<int>();
        stack.Push(0);
        while (stack.Count > 0)
            foreach (int next in graph.Neighbours(stack.Pop()))
                if (seen.Add(next))
                    stack.Push(next);
        return seen.Count == graph.RoomCount;
    }

    private static int TotalLength(RoomGraph graph, IReadOnlyList<Vector2Int> centres)
    {
        int total = 0;
        foreach ((int a, int b) in graph.Edges())
            total += Mathf.Abs(centres[a].x - centres[b].x) + Mathf.Abs(centres[a].y - centres[b].y);
        return total;
    }

    private static int Count(RoomType[] roles, RoomType type)
    {
        int count = 0;
        foreach (RoomType role in roles)
            if (role == type) count++;
        return count;
    }

    // The 5-room example from the lecture (Unit 5): A, B, C, D, E.
    // Its minimum spanning tree is A-B (4), B-C (3), C-D (6), A-E (8): total 21.
    private static readonly List<Vector2Int> LectureRooms = new List<Vector2Int>
    {
        new Vector2Int(0, 0),    // 0 = A
        new Vector2Int(4, 0),    // 1 = B
        new Vector2Int(4, 3),    // 2 = C
        new Vector2Int(10, 3),   // 3 = D
        new Vector2Int(0, 8),    // 4 = E
    };

    // ---------- A1: the constructor ----------

    [Test]
    public void A1_NewGraph_HasTheRoomsAndNoEdges()
    {
        var graph = new RoomGraph(4);
        Assert.AreEqual(4, graph.RoomCount);
        Assert.AreEqual(0, graph.EdgeCount);
    }

    [Test]
    public void A1_NegativeRoomCount_Throws()
    {
        Assert.Throws<System.ArgumentException>(() => new RoomGraph(-1));
    }

    // ---------- A2 / A3: HasEdge and AddEdge ----------

    [Test]
    public void A3_AddEdge_WorksInBothDirections()
    {
        var graph = new RoomGraph(3);
        Assert.IsTrue(graph.AddEdge(0, 2), "a new edge returns true");
        Assert.IsTrue(graph.HasEdge(0, 2));
        Assert.IsTrue(graph.HasEdge(2, 0), "the graph is UNDIRECTED: 0-2 is the same edge as 2-0");
        Assert.IsFalse(graph.HasEdge(0, 1));
        Assert.AreEqual(1, graph.EdgeCount);
    }

    [Test]
    public void A3_AddEdge_Duplicate_IsIgnored()
    {
        var graph = new RoomGraph(3);
        graph.AddEdge(0, 1);
        Assert.IsFalse(graph.AddEdge(0, 1), "same edge again returns false");
        Assert.IsFalse(graph.AddEdge(1, 0), "the reversed edge is the same edge");
        Assert.AreEqual(1, graph.EdgeCount);
        Assert.AreEqual(1, graph.Degree(0), "no duplicate in the neighbour list");
    }

    [Test]
    public void A3_AddEdge_ToItself_Throws()
    {
        var graph = new RoomGraph(3);
        Assert.Throws<System.ArgumentException>(() => graph.AddEdge(1, 1));
    }

    [Test]
    public void A3_AddEdge_RoomThatDoesntExist_Throws()
    {
        var graph = new RoomGraph(3);
        Assert.Throws<System.ArgumentOutOfRangeException>(() => graph.AddEdge(0, 3));
    }

    // ---------- A4: Neighbours and Degree ----------

    [Test]
    public void A4_NeighboursAndDegree_ListEveryConnection()
    {
        // A star: room 0 in the middle, joined to 1, 2 and 3.
        RoomGraph graph = MakeGraph(4, (0, 1), (0, 2), (3, 0));
        CollectionAssert.AreEquivalent(new[] { 1, 2, 3 }, graph.Neighbours(0));
        CollectionAssert.AreEquivalent(new[] { 0 }, graph.Neighbours(3));
        Assert.AreEqual(3, graph.Degree(0));
        Assert.AreEqual(1, graph.Degree(2));
    }

    [Test]
    public void A4_Edges_ListsEachEdgeOnce()
    {
        RoomGraph graph = MakeGraph(4, (0, 1), (2, 1), (3, 2));
        CollectionAssert.AreEquivalent(new[] { (0, 1), (1, 2), (2, 3) }, graph.Edges());
    }

    // ---------- B1: Distance ----------

    [Test]
    public void B1_Distance_IsManhattan()
    {
        Assert.AreEqual(7, SpanningTree.Distance(new Vector2Int(1, 2), new Vector2Int(4, 6)));
        Assert.AreEqual(7, SpanningTree.Distance(new Vector2Int(4, 6), new Vector2Int(1, 2)), "order doesn't matter");
        Assert.AreEqual(0, SpanningTree.Distance(new Vector2Int(3, 3), new Vector2Int(3, 3)));
    }

    // ---------- B2-B4: Prim ----------

    [Test]
    public void B4_Prim_NoRoomsOrOneRoom_HasNoEdges()
    {
        Assert.AreEqual(0, SpanningTree.Prim(new List<Vector2Int>()).EdgeCount);
        RoomGraph one = SpanningTree.Prim(new List<Vector2Int> { new Vector2Int(5, 5) });
        Assert.AreEqual(1, one.RoomCount);
        Assert.AreEqual(0, one.EdgeCount);
    }

    [Test]
    public void B4_Prim_LectureExample_GivesTheKnownTree()
    {
        RoomGraph tree = SpanningTree.Prim(LectureRooms);
        Assert.AreEqual(4, tree.EdgeCount, "a tree on 5 rooms has 5 - 1 = 4 edges");
        Assert.AreEqual(21, TotalLength(tree, LectureRooms), "the minimum total length is 21");
        Assert.IsTrue(tree.HasEdge(0, 1), "A-B");
        Assert.IsTrue(tree.HasEdge(1, 2), "B-C");
        Assert.IsTrue(tree.HasEdge(2, 3), "C-D");
        Assert.IsTrue(tree.HasEdge(0, 4), "A-E");
    }

    [Test]
    public void B4_Prim_RandomRooms_IsAlwaysASpanningTree()
    {
        for (int seed = 1; seed <= 20; seed++)
        {
            var rng = new System.Random(seed);
            var centres = new List<Vector2Int>();
            int n = rng.Next(2, 15);
            for (int i = 0; i < n; i++)
                centres.Add(new Vector2Int(rng.Next(0, 60), rng.Next(0, 40)));

            RoomGraph tree = SpanningTree.Prim(centres);
            Assert.AreEqual(n, tree.RoomCount, $"seed {seed}");
            Assert.AreEqual(n - 1, tree.EdgeCount, $"seed {seed}: a spanning tree has exactly n - 1 edges");
            Assert.IsTrue(IsConnected(tree), $"seed {seed}: every room must be reachable");
        }
    }

    [Test]
    public void B4_Prim_IsNeverLongerThanTheLesson3Chain()
    {
        for (int seed = 1; seed <= 20; seed++)
        {
            var rng = new System.Random(seed);
            var centres = new List<Vector2Int>();
            for (int i = 0; i < 10; i++)
                centres.Add(new Vector2Int(rng.Next(0, 60), rng.Next(0, 40)));

            int chain = 0;
            for (int i = 0; i < centres.Count - 1; i++)
                chain += SpanningTree.Distance(centres[i], centres[i + 1]);

            int mst = TotalLength(SpanningTree.Prim(centres), centres);
            Assert.LessOrEqual(mst, chain, $"seed {seed}: the chain is a spanning tree too, so the MINIMUM one can't be longer");
        }
    }

    // ---------- C1/C2: Distances ----------

    [Test]
    public void C2_Distances_OnAChain_CountTheSteps()
    {
        RoomGraph chain = MakeGraph(4, (0, 1), (1, 2), (2, 3));
        CollectionAssert.AreEqual(new[] { 0, 1, 2, 3 }, Bfs.Distances(chain, 0));
        CollectionAssert.AreEqual(new[] { 2, 1, 0, 1 }, Bfs.Distances(chain, 2));
    }

    [Test]
    public void C2_Distances_TakeTheShortestRoute()
    {
        // A loop 0-1-2-3-4-0: room 3 is 3 steps one way round, but only 2 the other way.
        RoomGraph loop = MakeGraph(5, (0, 1), (1, 2), (2, 3), (3, 4), (4, 0));
        CollectionAssert.AreEqual(new[] { 0, 1, 2, 2, 1 }, Bfs.Distances(loop, 0));
    }

    [Test]
    public void C2_Distances_UnreachableRooms_AreMinusOne()
    {
        RoomGraph split = MakeGraph(4, (0, 1), (2, 3));
        CollectionAssert.AreEqual(new[] { 0, 1, -1, -1 }, Bfs.Distances(split, 0));
    }

    [Test]
    public void C1_Distances_StartThatDoesntExist_Throws()
    {
        Assert.Throws<System.ArgumentOutOfRangeException>(() => Bfs.Distances(new RoomGraph(3), 5));
    }

    // ---------- C3: Farthest ----------

    [Test]
    public void C3_Farthest_FindsTheEndOfTheLongestBranch()
    {
        //   3 - 0 - 1 - 2
        //           |
        //           4 - 5 - 6     ← room 6 is 4 steps from 0
        RoomGraph tree = MakeGraph(7, (0, 3), (0, 1), (1, 2), (1, 4), (4, 5), (5, 6));
        Assert.AreEqual(6, Bfs.Farthest(tree, 0));
        Assert.AreEqual(6, Bfs.Farthest(tree, 3));
        Assert.AreEqual(3, Bfs.Farthest(tree, 6));
    }

    [Test]
    public void C3_Farthest_Tie_PicksTheLowestId()
    {
        RoomGraph star = MakeGraph(4, (0, 1), (0, 2), (0, 3));
        Assert.AreEqual(1, Bfs.Farthest(star, 0));
    }

    [Test]
    public void C3_Farthest_SingleRoom_IsItself()
    {
        Assert.AreEqual(0, Bfs.Farthest(new RoomGraph(1), 0));
    }

    // ---------- D1: loops ----------

    [Test]
    public void D1_AddExtra_ChanceZero_AddsNothing()
    {
        RoomGraph tree = SpanningTree.Prim(LectureRooms);
        int added = Loops.AddExtra(tree, LectureRooms, 100, 0.0, new System.Random(1));
        Assert.AreEqual(0, added);
        Assert.AreEqual(4, tree.EdgeCount);
    }

    [Test]
    public void D1_AddExtra_ChanceOne_AddsEveryCloseMissingEdge()
    {
        // Missing edges and their lengths: A-C 7, A-D 13, B-D 9, B-E 12, C-E 9, D-E 15.
        // With maxDistance 9, exactly A-C, B-D and C-E are close enough.
        RoomGraph graph = SpanningTree.Prim(LectureRooms);
        int added = Loops.AddExtra(graph, LectureRooms, 9, 1.0, new System.Random(1));
        Assert.AreEqual(3, added);
        Assert.AreEqual(7, graph.EdgeCount);
        Assert.IsTrue(graph.HasEdge(0, 2), "A-C (7)");
        Assert.IsTrue(graph.HasEdge(1, 3), "B-D (9): 'up to maxDistance' includes 9");
        Assert.IsTrue(graph.HasEdge(2, 4), "C-E (9)");
        Assert.IsFalse(graph.HasEdge(1, 4), "B-E (12) is too long");
    }

    [Test]
    public void D1_AddExtra_SameSeed_SameLoops()
    {
        RoomGraph a = SpanningTree.Prim(LectureRooms);
        RoomGraph b = SpanningTree.Prim(LectureRooms);
        Loops.AddExtra(a, LectureRooms, 100, 0.5, new System.Random(42));
        Loops.AddExtra(b, LectureRooms, 100, 0.5, new System.Random(42));
        CollectionAssert.AreEquivalent(a.Edges(), b.Edges());
    }

    // ---------- D2-D4: roles ----------

    [Test]
    public void D2_Assign_SpawnAndBossAtOppositeEnds()
    {
        RoomGraph chain = MakeGraph(5, (0, 1), (1, 2), (2, 3), (3, 4));
        RoomType[] roles = RoomRoles.Assign(chain, 0, new System.Random(1));
        Assert.AreEqual(5, roles.Length);
        Assert.AreEqual(RoomType.Spawn, roles[0]);
        Assert.AreEqual(RoomType.Boss, roles[4], "the boss is in the room farthest from the spawn");
        Assert.AreEqual(1, Count(roles, RoomType.Spawn));
        Assert.AreEqual(1, Count(roles, RoomType.Boss));
    }

    [Test]
    public void D2_Assign_SingleRoom_IsJustTheSpawn()
    {
        RoomType[] roles = RoomRoles.Assign(new RoomGraph(1), 0, new System.Random(1));
        CollectionAssert.AreEqual(new[] { RoomType.Spawn }, roles);
    }

    [Test]
    public void D3_Assign_DeadEnds_BecomeTreasure()
    {
        //   3 - 0 - 1 - 2
        //           |
        //           4 - 5 - 6
        // Dead ends (degree 1): 3, 2 and 6. Room 6 is the boss, so 3 and 2 are treasure.
        RoomGraph tree = MakeGraph(7, (0, 3), (0, 1), (1, 2), (1, 4), (4, 5), (5, 6));
        RoomType[] roles = RoomRoles.Assign(tree, 0, new System.Random(1));
        Assert.AreEqual(RoomType.Boss, roles[6]);
        Assert.AreEqual(RoomType.Treasure, roles[3]);
        Assert.AreEqual(RoomType.Treasure, roles[2]);
        Assert.AreEqual(2, Count(roles, RoomType.Treasure));
    }

    [Test]
    public void D3_Assign_SpawnIsNeverTreasure()
    {
        // The spawn (0) is a dead end here, but it stays the spawn.
        RoomGraph chain = MakeGraph(3, (0, 1), (1, 2));
        RoomType[] roles = RoomRoles.Assign(chain, 0, new System.Random(1));
        Assert.AreEqual(RoomType.Spawn, roles[0]);
        Assert.AreEqual(0, Count(roles, RoomType.Treasure));
    }

    [Test]
    public void D4_Assign_ExactlyOneShop_InANormalRoom()
    {
        RoomGraph tree = MakeGraph(7, (0, 3), (0, 1), (1, 2), (1, 4), (4, 5), (5, 6));
        for (int seed = 1; seed <= 10; seed++)
        {
            RoomType[] roles = RoomRoles.Assign(tree, 0, new System.Random(seed));
            Assert.AreEqual(1, Count(roles, RoomType.Shop), $"seed {seed}");
            int shop = System.Array.IndexOf(roles, RoomType.Shop);
            Assert.IsTrue(shop == 1 || shop == 4 || shop == 5,
                $"seed {seed}: the shop must be a room that had no other role (1, 4 or 5), got {shop}");
        }
    }

    [Test]
    public void D4_Assign_NoNormalRoomLeft_NoShop()
    {
        RoomGraph pair = MakeGraph(2, (0, 1));
        RoomType[] roles = RoomRoles.Assign(pair, 0, new System.Random(1));
        CollectionAssert.AreEqual(new[] { RoomType.Spawn, RoomType.Boss }, roles);
    }

    [Test]
    public void D4_Assign_SameSeed_SameRoles()
    {
        RoomGraph tree = MakeGraph(7, (0, 3), (0, 1), (1, 2), (1, 4), (4, 5), (5, 6));
        CollectionAssert.AreEqual(RoomRoles.Assign(tree, 0, new System.Random(7)),
                                  RoomRoles.Assign(tree, 0, new System.Random(7)));
    }

    // ---------- E1: the generator ----------

    [Test]
    public void E1_GenerateBsp_HasAConnectedGraphWithOneVertexPerRoom()
    {
        for (int seed = 1; seed <= 10; seed++)
        {
            DungeonResult d = DungeonGenerator.GenerateBsp(60, 40, seed);
            Assert.IsNotNull(d.Graph, "set result.Graph in GenerateBsp");
            Assert.AreEqual(d.Rooms.Count, d.Graph.RoomCount, $"seed {seed}: one graph vertex per room");
            Assert.GreaterOrEqual(d.Graph.EdgeCount, d.Rooms.Count - 1, $"seed {seed}: at least a spanning tree");
            Assert.IsTrue(IsConnected(d.Graph), $"seed {seed}");
        }
    }

    [Test]
    public void E1_GenerateBsp_RolesHaveOneSpawnInRoomZeroAndOneBoss()
    {
        for (int seed = 1; seed <= 10; seed++)
        {
            DungeonResult d = DungeonGenerator.GenerateBsp(60, 40, seed);
            Assert.IsNotNull(d.Roles, "set result.Roles in GenerateBsp");
            Assert.AreEqual(d.Rooms.Count, d.Roles.Length, $"seed {seed}: one role per room");
            Assert.AreEqual(RoomType.Spawn, d.Roles[0], $"seed {seed}: room 0 is the spawn");
            Assert.AreEqual(1, Count(d.Roles, RoomType.Boss), $"seed {seed}");
            Assert.AreEqual(DungeonGenerator.Center(d.Rooms[0]), d.Spawn, $"seed {seed}: the player spawns in the spawn room");
        }
    }

    [Test]
    public void E1_GenerateBsp_SameSeed_SameGraphAndRoles()
    {
        DungeonResult a = DungeonGenerator.GenerateBsp(60, 40, 99);
        DungeonResult b = DungeonGenerator.GenerateBsp(60, 40, 99);
        Assert.IsNotNull(a.Graph, "set result.Graph in GenerateBsp");
        CollectionAssert.AreEquivalent(a.Graph.Edges(), b.Graph.Edges());
        CollectionAssert.AreEqual(a.Roles, b.Roles);
        Assert.AreEqual(a.Grid.ToString(), b.Grid.ToString());
    }
}
