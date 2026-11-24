using System.Collections.Generic;
using NUnit.Framework;
using Roguelite.Dungeon;
using UnityEngine;

// Tests for Lesson 3. Each test name starts with the TODO it checks (A1_, B3_, C4_ ...).
// Read a test when you're stuck: it's the exact specification of what your code must do.
public class GenerationTests
{
    // ---------- helpers ----------

    private static int CountFloors(MapGrid grid)
    {
        int count = 0;
        for (int x = 0; x < grid.Width; x++)
            for (int y = 0; y < grid.Height; y++)
                if (grid.Get(x, y) == TileType.Floor) count++;
        return count;
    }

    private static bool BorderIsAllWall(MapGrid grid)
    {
        for (int x = 0; x < grid.Width; x++)
            if (grid.Get(x, 0) != TileType.Wall || grid.Get(x, grid.Height - 1) != TileType.Wall) return false;
        for (int y = 0; y < grid.Height; y++)
            if (grid.Get(0, y) != TileType.Wall || grid.Get(grid.Width - 1, y) != TileType.Wall) return false;
        return true;
    }

    // FLOOD FILL (a preview of Lesson 4): starting from one floor cell, visit every floor cell
    // you can walk to, one neighbour at a time. Returns how many cells were reached.
    // If that equals the total number of floors, the whole map is connected.
    private static int CountReachableFloors(MapGrid grid, Vector2Int start)
    {
        if (grid.Get(start.x, start.y) != TileType.Floor) return 0;
        var visited = new HashSet<Vector2Int> { start };
        var toVisit = new Queue<Vector2Int>();
        toVisit.Enqueue(start);
        while (toVisit.Count > 0)
        {
            Vector2Int cell = toVisit.Dequeue();
            foreach (Vector2Int dir in MapGrid.Directions4)
            {
                Vector2Int next = cell + dir;
                if (grid.Get(next.x, next.y) == TileType.Floor && visited.Add(next))
                    toVisit.Enqueue(next);
            }
        }
        return visited.Count;
    }

    private static bool IsAllFloor(MapGrid grid, RectInt rect)
    {
        for (int x = rect.xMin; x < rect.xMax; x++)
            for (int y = rect.yMin; y < rect.yMax; y++)
                if (grid.Get(x, y) != TileType.Floor) return false;
        return true;
    }

    // ---------- A1: ClampToInterior ----------

    [Test]
    public void A1_ClampToInterior_LeavesInsidePointsAlone()
    {
        var grid = new MapGrid(10, 8);
        Assert.AreEqual(new Vector2Int(4, 3), RandomWalk.ClampToInterior(grid, new Vector2Int(4, 3)));
        Assert.AreEqual(new Vector2Int(1, 1), RandomWalk.ClampToInterior(grid, new Vector2Int(1, 1)), "the inside corner is allowed");
        Assert.AreEqual(new Vector2Int(8, 6), RandomWalk.ClampToInterior(grid, new Vector2Int(8, 6)), "Width - 2 and Height - 2 are allowed");
    }

    [Test]
    public void A1_ClampToInterior_PullsOutsidePointsInsideTheBorder()
    {
        var grid = new MapGrid(10, 8);
        Assert.AreEqual(new Vector2Int(1, 6), RandomWalk.ClampToInterior(grid, new Vector2Int(-5, 50)));
        Assert.AreEqual(new Vector2Int(8, 1), RandomWalk.ClampToInterior(grid, new Vector2Int(9, 0)), "the border itself is not allowed");
    }

    // ---------- A2: Carve ----------

    [Test]
    public void A2_Carve_ZeroSteps_CarvesOnlyTheStartCell()
    {
        var grid = new MapGrid(10, 8);
        RandomWalk.Carve(grid, new System.Random(1), new Vector2Int(4, 4), 0);
        Assert.AreEqual(TileType.Floor, grid.Get(4, 4));
        Assert.AreEqual(1, CountFloors(grid));
    }

    [Test]
    public void A2_Carve_StartOnTheBorder_IsMovedInside()
    {
        var grid = new MapGrid(10, 8);
        RandomWalk.Carve(grid, new System.Random(1), new Vector2Int(0, 0), 0);
        Assert.AreEqual(TileType.Floor, grid.Get(1, 1));
        Assert.AreEqual(TileType.Wall, grid.Get(0, 0));
    }

    [Test]
    public void A2_Carve_ManySteps_DigsACaveButNeverTheBorder()
    {
        var grid = new MapGrid(12, 10);
        RandomWalk.Carve(grid, new System.Random(7), new Vector2Int(6, 5), 2000);
        Assert.Greater(CountFloors(grid), 20, "2000 steps should dig out a good part of the map");
        Assert.IsTrue(BorderIsAllWall(grid), "the 1-tile border must stay Wall");
    }

    [Test]
    public void A2_Carve_SameSeed_GivesTheSameCave()
    {
        var a = new MapGrid(30, 20);
        var b = new MapGrid(30, 20);
        RandomWalk.Carve(a, new System.Random(42), new Vector2Int(15, 10), 600);
        RandomWalk.Carve(b, new System.Random(42), new Vector2Int(15, 10), 600);
        Assert.Greater(CountFloors(a), 1, "the walk should have dug something");
        Assert.AreEqual(a.ToString(), b.ToString());
    }

    [Test]
    public void A2_Carve_DifferentSeeds_GiveDifferentCaves()
    {
        var a = new MapGrid(30, 20);
        var b = new MapGrid(30, 20);
        RandomWalk.Carve(a, new System.Random(1), new Vector2Int(15, 10), 600);
        RandomWalk.Carve(b, new System.Random(2), new Vector2Int(15, 10), 600);
        Assert.AreNotEqual(a.ToString(), b.ToString());
    }

    [Test]
    public void A2_Carve_EveryFloorCanBeReachedFromTheStart()
    {
        var grid = new MapGrid(30, 20);
        RandomWalk.Carve(grid, new System.Random(3), new Vector2Int(15, 10), 800);
        Assert.Greater(CountFloors(grid), 1, "the walk should have dug something");
        Assert.AreEqual(CountFloors(grid), CountReachableFloors(grid, new Vector2Int(15, 10)));
    }

    [Test]
    public void A2_Carve_TinyGrid_Throws()
    {
        var grid = new MapGrid(2, 5);
        Assert.Throws<System.ArgumentException>(() => RandomWalk.Carve(grid, new System.Random(1), Vector2Int.one, 10));
    }

    // ---------- B1 / B2: straight corridors ----------

    [Test]
    public void B1_CarveHorizontal_IncludesBothEnds()
    {
        var grid = new MapGrid(10, 8);
        Corridors.CarveHorizontal(grid, 2, 6, 3);
        for (int x = 2; x <= 6; x++) Assert.AreEqual(TileType.Floor, grid.Get(x, 3), $"x = {x}");
        Assert.AreEqual(TileType.Wall, grid.Get(1, 3));
        Assert.AreEqual(TileType.Wall, grid.Get(7, 3));
        Assert.AreEqual(5, CountFloors(grid));
    }

    [Test]
    public void B1_CarveHorizontal_WorksRightToLeft()
    {
        var grid = new MapGrid(10, 8);
        Corridors.CarveHorizontal(grid, 6, 2, 3);
        Assert.AreEqual(5, CountFloors(grid));
        Assert.AreEqual(TileType.Floor, grid.Get(2, 3));
        Assert.AreEqual(TileType.Floor, grid.Get(6, 3));
    }

    [Test]
    public void B1_CarveHorizontal_Thickness3_IsCentredOnTheRow()
    {
        var grid = new MapGrid(10, 8);
        Corridors.CarveHorizontal(grid, 2, 6, 3, 3);
        Assert.AreEqual(15, CountFloors(grid));
        Assert.AreEqual(TileType.Floor, grid.Get(4, 2));
        Assert.AreEqual(TileType.Floor, grid.Get(4, 4));
        Assert.AreEqual(TileType.Wall, grid.Get(4, 5));
    }

    [Test]
    public void B2_CarveVertical_IncludesBothEndsInEitherOrder()
    {
        var a = new MapGrid(8, 10);
        Corridors.CarveVertical(a, 2, 6, 3);
        var b = new MapGrid(8, 10);
        Corridors.CarveVertical(b, 6, 2, 3);
        Assert.AreEqual(5, CountFloors(a));
        Assert.AreEqual(a.ToString(), b.ToString());
        Assert.AreEqual(TileType.Floor, a.Get(3, 2));
        Assert.AreEqual(TileType.Floor, a.Get(3, 6));
        Assert.AreEqual(TileType.Wall, a.Get(3, 7));
    }

    [Test]
    public void B2_CarveVertical_Thickness3_IsCentredOnTheColumn()
    {
        var grid = new MapGrid(8, 10);
        Corridors.CarveVertical(grid, 2, 6, 3, 3);
        Assert.AreEqual(15, CountFloors(grid));
        Assert.AreEqual(TileType.Floor, grid.Get(2, 4));
        Assert.AreEqual(TileType.Floor, grid.Get(4, 4));
    }

    // ---------- B3: L-shaped corridors ----------

    [Test]
    public void B3_CarveL_HorizontalFirst_TurnsAtToXFromY()
    {
        var grid = new MapGrid(12, 12);
        Corridors.CarveL(grid, new Vector2Int(2, 2), new Vector2Int(8, 9), true);
        Assert.AreEqual(TileType.Floor, grid.Get(8, 2), "the corner is (to.x, from.y)");
        Assert.AreEqual(TileType.Wall, grid.Get(2, 9), "(from.x, to.y) is the OTHER corner");
        Assert.AreEqual(14, CountFloors(grid), "7 across + 8 up, sharing the corner cell");
    }

    [Test]
    public void B3_CarveL_VerticalFirst_TurnsAtFromXToY()
    {
        var grid = new MapGrid(12, 12);
        Corridors.CarveL(grid, new Vector2Int(2, 2), new Vector2Int(8, 9), false);
        Assert.AreEqual(TileType.Floor, grid.Get(2, 9), "the corner is (from.x, to.y)");
        Assert.AreEqual(TileType.Wall, grid.Get(8, 2));
        Assert.AreEqual(14, CountFloors(grid));
    }

    [Test]
    public void B3_CarveL_ConnectsTheTwoPoints()
    {
        var grid = new MapGrid(20, 20);
        var from = new Vector2Int(15, 3);
        var to = new Vector2Int(4, 16);
        Corridors.CarveL(grid, from, to, true, 2);
        Assert.AreEqual(TileType.Floor, grid.Get(to.x, to.y));
        Assert.AreEqual(CountFloors(grid), CountReachableFloors(grid, from));
    }

    [Test]
    public void B3_CarveL_SamePoint_CarvesOneCell()
    {
        var grid = new MapGrid(10, 10);
        Corridors.CarveL(grid, new Vector2Int(5, 5), new Vector2Int(5, 5), true);
        Assert.AreEqual(1, CountFloors(grid));
    }

    // ---------- C1-C4: Split ----------

    [Test]
    public void C1_Split_AreaTooSmallToCut_IsASingleLeaf()
    {
        var area = new RectInt(1, 1, 10, 10);
        List<RectInt> leaves = Bsp.Split(area, 6, new System.Random(1));
        Assert.AreEqual(1, leaves.Count);
        Assert.AreEqual(area, leaves[0]);
    }

    [Test]
    public void C1_Split_MinLeafSizeBelowOne_Throws()
    {
        Assert.Throws<System.ArgumentException>(() => Bsp.Split(new RectInt(0, 0, 10, 10), 0, new System.Random(1)));
    }

    [Test]
    public void C2_Split_WideArea_OnlyCutsVertically()
    {
        // 30 wide but only 8 tall: with minLeafSize 6 there's no room for a horizontal cut.
        List<RectInt> leaves = Bsp.Split(new RectInt(0, 0, 30, 8), 6, new System.Random(5));
        Assert.Greater(leaves.Count, 1, "a 30-wide area must be cut at least once");
        foreach (RectInt leaf in leaves) Assert.AreEqual(8, leaf.height, "every cut was vertical, so heights are unchanged");
    }

    [Test]
    public void C2_Split_TallArea_OnlyCutsHorizontally()
    {
        List<RectInt> leaves = Bsp.Split(new RectInt(0, 0, 8, 30), 6, new System.Random(5));
        Assert.Greater(leaves.Count, 1);
        foreach (RectInt leaf in leaves) Assert.AreEqual(8, leaf.width);
    }

    [Test]
    public void C3_Split_EveryLeafIsBigEnoughAndCantBeCutAgain()
    {
        for (int seed = 0; seed < 20; seed++)
        {
            List<RectInt> leaves = Bsp.Split(new RectInt(1, 1, 78, 48), 8, new System.Random(seed));
            Assert.Greater(leaves.Count, 1, $"seed {seed}: a 78x48 area must be cut");
            foreach (RectInt leaf in leaves)
            {
                Assert.GreaterOrEqual(leaf.width, 8, $"seed {seed}: leaf {leaf} too narrow");
                Assert.GreaterOrEqual(leaf.height, 8, $"seed {seed}: leaf {leaf} too short");
                Assert.IsTrue(leaf.width < 16 && leaf.height < 16, $"seed {seed}: leaf {leaf} should have been cut again");
            }
        }
    }

    [Test]
    public void C4_Split_LeavesCoverTheAreaExactlyWithNoOverlaps()
    {
        var area = new RectInt(1, 1, 78, 48);
        List<RectInt> leaves = Bsp.Split(area, 8, new System.Random(11));
        int totalCells = 0;
        for (int i = 0; i < leaves.Count; i++)
        {
            RectInt a = leaves[i];
            totalCells += a.width * a.height;
            Assert.IsTrue(a.xMin >= area.xMin && a.yMin >= area.yMin && a.xMax <= area.xMax && a.yMax <= area.yMax, $"{a} is outside the area");
            for (int j = i + 1; j < leaves.Count; j++)
                Assert.IsFalse(a.Overlaps(leaves[j]), $"{a} overlaps {leaves[j]}");
        }
        Assert.AreEqual(area.width * area.height, totalCells, "the leaves must add up to the whole area");
    }

    [Test]
    public void C4_Split_SameSeed_GivesTheSameLeaves()
    {
        var area = new RectInt(1, 1, 78, 48);
        List<RectInt> first = Bsp.Split(area, 8, new System.Random(9));
        Assert.Greater(first.Count, 1, "a 78x48 area must be cut");
        CollectionAssert.AreEqual(first, Bsp.Split(area, 8, new System.Random(9)));
    }

    // ---------- C5: PlaceRoom ----------

    [Test]
    public void C5_PlaceRoom_StaysInsideTheLeafMargin()
    {
        var leaf = new RectInt(10, 20, 12, 9);
        var rng = new System.Random(4);
        for (int i = 0; i < 200; i++)
        {
            RectInt room = Bsp.PlaceRoom(leaf, 1, 3, rng);
            Assert.GreaterOrEqual(room.xMin, leaf.xMin + 1, $"{room}");
            Assert.GreaterOrEqual(room.yMin, leaf.yMin + 1, $"{room}");
            Assert.LessOrEqual(room.xMax, leaf.xMax - 1, $"{room}");
            Assert.LessOrEqual(room.yMax, leaf.yMax - 1, $"{room}");
            Assert.GreaterOrEqual(room.width, 3);
            Assert.GreaterOrEqual(room.height, 3);
        }
    }

    [Test]
    public void C5_PlaceRoom_CanUseTheWholeSpace()
    {
        // A leaf with exactly enough room: margin 1 + room 4 + margin 1 = 6. Only one room fits.
        RectInt room = Bsp.PlaceRoom(new RectInt(0, 0, 6, 6), 1, 4, new System.Random(1));
        Assert.AreEqual(new RectInt(1, 1, 4, 4), room);
    }

    [Test]
    public void C5_PlaceRoom_LeafTooSmall_Throws()
    {
        Assert.Throws<System.ArgumentException>(() => Bsp.PlaceRoom(new RectInt(0, 0, 5, 10), 1, 4, new System.Random(1)));
    }

    // ---------- D1-D4: GenerateBsp ----------

    [Test]
    public void D1_GenerateBsp_SameSeed_GivesTheSameDungeon()
    {
        Assert.AreEqual(DungeonGenerator.GenerateBsp(60, 40, 123).Grid.ToString(),
                        DungeonGenerator.GenerateBsp(60, 40, 123).Grid.ToString());
    }

    [Test]
    public void D1_GenerateBsp_DifferentSeeds_GiveDifferentDungeons()
    {
        Assert.AreNotEqual(DungeonGenerator.GenerateBsp(60, 40, 1).Grid.ToString(),
                           DungeonGenerator.GenerateBsp(60, 40, 2).Grid.ToString());
    }

    [Test]
    public void D2_GenerateBsp_MakesSeveralRoomsThatAreFloorAndDontOverlap()
    {
        DungeonResult d = DungeonGenerator.GenerateBsp(60, 40, 5);
        Assert.Greater(d.Rooms.Count, 3, "a 60x40 map with leaves of 10+ should have several rooms");
        for (int i = 0; i < d.Rooms.Count; i++)
        {
            Assert.IsTrue(IsAllFloor(d.Grid, d.Rooms[i]), $"room {i} {d.Rooms[i]} isn't all Floor");
            for (int j = i + 1; j < d.Rooms.Count; j++)
                Assert.IsFalse(d.Rooms[i].Overlaps(d.Rooms[j]), $"rooms {i} and {j} overlap");
        }
    }

    [Test]
    public void D3_GenerateBsp_EveryFloorIsConnected()
    {
        for (int seed = 0; seed < 10; seed++)
        {
            DungeonResult d = DungeonGenerator.GenerateBsp(60, 40, seed);
            Assert.AreEqual(CountFloors(d.Grid), CountReachableFloors(d.Grid, d.Spawn),
                $"seed {seed}: some floor can't be reached from the spawn (a missing corridor?)");
        }
    }

    [Test]
    public void D3_GenerateBsp_BorderStaysWall()
    {
        for (int seed = 0; seed < 10; seed++)
            Assert.IsTrue(BorderIsAllWall(DungeonGenerator.GenerateBsp(60, 40, seed).Grid), $"seed {seed}");
    }

    [Test]
    public void D4_GenerateBsp_SpawnIsTheCentreOfTheFirstRoom()
    {
        DungeonResult d = DungeonGenerator.GenerateBsp(60, 40, 8);
        Assert.AreEqual(DungeonGenerator.Center(d.Rooms[0]), d.Spawn);
        Assert.AreEqual(TileType.Floor, d.Grid.Get(d.Spawn.x, d.Spawn.y));
    }

    // ---------- D5: GenerateCave ----------

    [Test]
    public void D5_GenerateCave_IsRepeatableConnectedAndSpawnsOnFloor()
    {
        DungeonResult a = DungeonGenerator.GenerateCave(50, 30, 77, 1500);
        DungeonResult b = DungeonGenerator.GenerateCave(50, 30, 77, 1500);
        Assert.AreEqual(a.Grid.ToString(), b.Grid.ToString(), "same seed, same cave");
        Assert.AreEqual(new Vector2Int(25, 15), a.Spawn, "spawn at the middle of the map");
        Assert.AreEqual(TileType.Floor, a.Grid.Get(a.Spawn.x, a.Spawn.y));
        Assert.AreEqual(CountFloors(a.Grid), CountReachableFloors(a.Grid, a.Spawn));
        Assert.IsTrue(BorderIsAllWall(a.Grid));
    }
}
