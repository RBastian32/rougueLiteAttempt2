using System;
using NUnit.Framework;
using Roguelite.Dungeon;
using UnityEngine;

namespace Roguelite.Tests
{
    // UNIT TESTS: small pieces of code that check other code automatically.
    //
    // Every test has three parts, often called "Arrange, Act, Assert":
    //   Arrange - set up the situation (make a grid),
    //   Act     - do the thing being tested (call a method),
    //   Assert  - check the result is what you expected.
    // If an Assert fails, the test goes red and tells you what it expected vs. what it got.
    //
    // Run them: Window > General > Test Runner > EditMode > Run All.
    // You don't need to edit this file, but DO read the tests for the TODO you're on:
    // they're an exact description of what your code must do.
    public class MapGridTests
    {
        // A helper used by several tests: how many cells of this type are in the grid?
        private static int CountTiles(MapGrid grid, TileType type)
        {
            int count = 0;
            for (int x = 0; x < grid.Width; x++)
                for (int y = 0; y < grid.Height; y++)
                    if (grid.Get(x, y) == type) count++;
            return count;
        }

        // ---------- B1: constructor ----------

        [Test]
        public void B1_Constructor_StoresWidthAndHeight()
        {
            var grid = new MapGrid(10, 5);

            Assert.AreEqual(10, grid.Width);
            Assert.AreEqual(5, grid.Height);
        }

        [Test]
        public void B1_Constructor_RejectsSizesBelowOne()
        {
            Assert.Throws<ArgumentException>(() => new MapGrid(0, 5));
            Assert.Throws<ArgumentException>(() => new MapGrid(5, -1));
        }

        // ---------- B2: InBounds ----------

        [Test]
        public void B2_InBounds_TrueForCellsInside()
        {
            var grid = new MapGrid(10, 5);

            Assert.IsTrue(grid.InBounds(0, 0), "bottom-left corner");
            Assert.IsTrue(grid.InBounds(9, 4), "top-right corner");
            Assert.IsTrue(grid.InBounds(3, 2), "somewhere in the middle");
        }

        [Test]
        public void B2_InBounds_FalseForCellsOutside()
        {
            var grid = new MapGrid(10, 5);

            Assert.IsFalse(grid.InBounds(-1, 0), "left of the grid");
            Assert.IsFalse(grid.InBounds(0, -1), "below the grid");
            Assert.IsFalse(grid.InBounds(10, 0), "x == Width is one past the end");
            Assert.IsFalse(grid.InBounds(0, 5), "y == Height is one past the end");
        }

        // ---------- B3: Get / Set ----------

        [Test]
        public void B3_NewGrid_IsAllWalls()
        {
            var grid = new MapGrid(4, 3);

            Assert.AreEqual(12, CountTiles(grid, TileType.Wall));
        }

        [Test]
        public void B3_Set_ThenGet_ReturnsTheSameValue()
        {
            var grid = new MapGrid(4, 3);

            grid.Set(2, 1, TileType.Floor);

            Assert.AreEqual(TileType.Floor, grid.Get(2, 1));
        }

        [Test]
        public void B3_Set_ChangesOnlyThatOneCell()
        {
            var grid = new MapGrid(4, 3);

            grid.Set(2, 1, TileType.Floor);

            Assert.AreEqual(1, CountTiles(grid, TileType.Floor));
        }

        [Test]
        public void B3_XAndY_AreNotSwapped()
        {
            // A non-square grid catches the very common bug of mixing up x and y.
            var grid = new MapGrid(4, 2);

            grid.Set(3, 1, TileType.Floor);

            Assert.AreEqual(TileType.Floor, grid.Get(3, 1));
        }

        [Test]
        public void B3_Get_OutOfBounds_ReturnsWall()
        {
            var grid = new MapGrid(3, 3);
            grid.Fill(TileType.Floor);

            Assert.AreEqual(TileType.Wall, grid.Get(-1, 0));
            Assert.AreEqual(TileType.Wall, grid.Get(3, 0));
            Assert.AreEqual(TileType.Wall, grid.Get(0, 100));
        }

        [Test]
        public void B3_Set_OutOfBounds_IsIgnored()
        {
            var grid = new MapGrid(3, 3);

            Assert.DoesNotThrow(() => grid.Set(-1, 0, TileType.Floor));
            Assert.DoesNotThrow(() => grid.Set(3, 3, TileType.Floor));
            Assert.AreEqual(0, CountTiles(grid, TileType.Floor));
        }

        // ---------- B4: Fill ----------

        [Test]
        public void B4_Fill_SetsEveryCell()
        {
            var grid = new MapGrid(5, 4);

            grid.Fill(TileType.Floor);

            Assert.AreEqual(20, CountTiles(grid, TileType.Floor));
        }

        // ---------- B5: FillRect ----------

        [Test]
        public void B5_FillRect_CoversExactlyTheRectangle()
        {
            var grid = new MapGrid(6, 6);

            // x = 1..3 (3 wide), y = 1..2 (2 tall) -> 6 cells.
            grid.FillRect(new RectInt(1, 1, 3, 2), TileType.Floor);

            Assert.AreEqual(6, CountTiles(grid, TileType.Floor));
            Assert.AreEqual(TileType.Floor, grid.Get(1, 1), "bottom-left of the rect");
            Assert.AreEqual(TileType.Floor, grid.Get(3, 2), "top-right of the rect");
            Assert.AreEqual(TileType.Wall, grid.Get(4, 1), "xMax is NOT included");
            Assert.AreEqual(TileType.Wall, grid.Get(1, 3), "yMax is NOT included");
        }

        [Test]
        public void B5_FillRect_PartlyOutsideTheGrid_IsClipped()
        {
            var grid = new MapGrid(5, 5);

            // Covers x = -2..1, y = -2..1, but only x = 0..1, y = 0..1 is inside the grid.
            Assert.DoesNotThrow(() => grid.FillRect(new RectInt(-2, -2, 4, 4), TileType.Floor));

            Assert.AreEqual(4, CountTiles(grid, TileType.Floor));
        }

        // ---------- B6: neighbours ----------

        [Test]
        public void B6_Directions8_HasEightDifferentOffsets()
        {
            Assert.AreEqual(8, MapGrid.Directions8.Length);
            CollectionAssert.AllItemsAreUnique(MapGrid.Directions8);
            CollectionAssert.DoesNotContain(MapGrid.Directions8, Vector2Int.zero, "(0,0) is the cell itself, not a neighbour");
            foreach (var d in MapGrid.Directions8)
            {
                Assert.IsTrue(Mathf.Abs(d.x) <= 1 && Mathf.Abs(d.y) <= 1, $"{d} is not next to the centre cell");
            }
        }

        [Test]
        public void B6_CountNeighbors_InTheMiddle()
        {
            var grid = new MapGrid(3, 3);
            grid.Fill(TileType.Floor);

            Assert.AreEqual(4, grid.CountNeighbors(1, 1, TileType.Floor, includeDiagonals: false));
            Assert.AreEqual(8, grid.CountNeighbors(1, 1, TileType.Floor, includeDiagonals: true));
        }

        [Test]
        public void B6_CountNeighbors_DoesNotCountTheCellItself()
        {
            var grid = new MapGrid(1, 1);
            grid.Fill(TileType.Floor);

            Assert.AreEqual(0, grid.CountNeighbors(0, 0, TileType.Floor, includeDiagonals: true));
        }

        [Test]
        public void B6_CountNeighbors_OutsideTheMapCountsAsWall()
        {
            var grid = new MapGrid(3, 3);
            grid.Fill(TileType.Floor);

            // Corner (0,0): neighbours to the left and below are off the map, so they count as walls.
            Assert.AreEqual(2, grid.CountNeighbors(0, 0, TileType.Wall, includeDiagonals: false));
            Assert.AreEqual(5, grid.CountNeighbors(0, 0, TileType.Wall, includeDiagonals: true));
        }

        [Test]
        public void B6_CountNeighbors_MixedTiles()
        {
            var grid = new MapGrid(3, 3);       // all walls
            grid.Set(1, 2, TileType.Floor);     // above the centre
            grid.Set(2, 2, TileType.Floor);     // top-right diagonal of the centre

            Assert.AreEqual(1, grid.CountNeighbors(1, 1, TileType.Floor, includeDiagonals: false));
            Assert.AreEqual(2, grid.CountNeighbors(1, 1, TileType.Floor, includeDiagonals: true));
        }
    }
}
