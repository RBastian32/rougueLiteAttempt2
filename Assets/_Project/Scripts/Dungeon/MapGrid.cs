using System;
using System.Text;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Roguelite.Dungeon
{
    // What can a single cell of the map be?
    // An enum is a named set of choices. Under the hood each is an int (Wall = 0, Floor = 1).
    // Wall is listed first ON PURPOSE: a brand-new array is filled with the default value
    // (0), so a new map starts as solid rock, and level generators "carve" floors out of it.
    public enum TileType
    {
        Wall = 0,
        Floor = 1,
    }

    // LESSON 2 - EXERCISE B: The map as a 2D grid of data.
    //
    // This is a plain C# class, NOT a MonoBehaviour. It knows nothing about sprites,
    // Tilemaps, or scenes; it's just data plus rules. That makes it:
    //   * easy to test (the Test Runner can create one without a scene),
    //   * reusable (the dungeon generator, pathfinding and minimap can all read it).
    // Keeping game DATA separate from how it's DRAWN is one of the most important habits
    // in game architecture. MapRenderer (Exercise C) does the drawing.
    //
    // Coordinates: x goes right (0 .. Width-1), y goes UP (0 .. Height-1).
    // (0,0) is the bottom-left corner, which matches Unity's world and Tilemap coordinates.
    //
    // Check your work: Window > General > Test Runner > EditMode > Run All.
    // The tests live in Assets/_Project/Tests/EditMode/MapGridTests.cs.
    public class MapGrid
    {
        // The 4 "cardinal" directions as (x, y) offsets. To find the cell above (x, y),
        // add Vector2Int.up (0, 1) to get (x, y + 1). Looping over an array of offsets
        // is a classic trick: it saves writing four nearly identical if-statements.
        public static readonly Vector2Int[] Directions4 =
        {
            Vector2Int.up,
            Vector2Int.right,
            Vector2Int.down,
            Vector2Int.left,
        };

        // TODO B6a: Fill in all 8 directions: the 4 above PLUS the 4 diagonals.
        //           Vector2Int has no named diagonals; write them as new Vector2Int(1, 1), etc.
        public static readonly Vector2Int[] Directions8 =
        {
            Vector2Int.up,
            Vector2Int.right,
            Vector2Int.down,
            Vector2Int.left,
            new Vector2Int(1, 1),
            new Vector2Int(-1, -1),
            new Vector2Int(-1 ,1),
            new Vector2Int(1, -1)

        };

        // "{ get; }" = read-only from outside. It can only be set inside the constructor.
        public int Width { get; }
        public int Height { get; }

        // The actual storage: a 2D array. tiles[x, y] is the cell at column x, row y.
        private TileType[,] tiles;

        public MapGrid(int width, int height)
        {
            // TODO B1: (1) If width or height is less than 1, throw an ArgumentException.
            //              This is called a "guard clause": fail fast with a clear message
            //              instead of breaking strangely later.
            if(width < 1 || height < 1)
            {
                throw new ArgumentException($"MapGrid must be atleast 1x1, but got {width} x {height}");
            }
            //          (2) Store width and height in the Width and Height properties.
            Width = width;
            Height = height;
            //          (3) Create the tiles array with the right size.
            //          Then delete the line below.
            tiles = new TileType[width, height];
            
        }

        // Is (x, y) inside the grid?
        public bool InBounds(int x, int y)
        {
            // TODO B2: Return true only if 0 <= x < Width AND 0 <= y < Height.
            //          Watch the edges: x == Width is OUTSIDE, because indexes start at 0.
            return false;
        }

        // Read a cell.
        public TileType Get(int x, int y)
        {
            // TODO B3a: If (x, y) is out of bounds, return TileType.Wall.
            //           Otherwise return the value stored in the array.
            //
            //           Why return Wall instead of crashing? Pretend the whole world outside
            //           the map is solid rock. Then code like "is my neighbour a floor?" works
            //           even at the edges, with no special cases. This is called a "sentinel" value.
            return TileType.Wall;
        }

        // Write a cell.
        public void Set(int x, int y, TileType type)
        {
            // TODO B3b: If (x, y) is in bounds, store the type in the array.
            //           If it's out of bounds, do nothing (silently ignore it).
        }

        // Set every cell in the grid to one type.
        public void Fill(TileType type)
        {
            // TODO B4: Use two nested for-loops (one for x, one for y) to set every cell.
        }

        // Set every cell inside a rectangle to one type. Any part outside the grid is ignored.
        public void FillRect(RectInt rect, TileType type)
        {
            // TODO B5: Loop x from rect.xMin up to (but NOT including) rect.xMax,
            //          and y from rect.yMin up to (but NOT including) rect.yMax.
            //          Use Set() so cells outside the grid are skipped automatically.
            //
            //          "xMin inclusive, xMax exclusive" is called a half-open range. A rect at
            //          x = 2 with width 3 covers x = 2, 3, 4, and xMax is 5.
        }

        // How many of the cells touching (x, y) are of the given type?
        // includeDiagonals = false -> check 4 neighbours; true -> check 8.
        // The cell itself is NOT counted. Cells outside the map count as Wall (see Get).
        public int CountNeighbors(int x, int y, TileType type, bool includeDiagonals)
        {
            // TODO B6b: (1) Pick Directions8 or Directions4 depending on includeDiagonals.
            //           (2) Loop over the offsets. For each one, look at the cell at
            //               (x + offset.x, y + offset.y) using Get(), and count the matches.
            //           (3) Return the count.
            return 0;
        }

        // Turns the grid into text so you can Debug.Log(grid) and SEE it in the Console.
        // '#' = wall, '.' = floor. Rows are printed top (highest y) first, so the text
        // looks the same way round as the map does in the game.
        // (This one is done for you. Read it, since it's a good example of looping over a grid.)
        public override string ToString()
        {
            var text = new StringBuilder();
            for (int y = Height - 1; y >= 0; y--)
            {
                for (int x = 0; x < Width; x++)
                {
                    text.Append(Get(x, y) == TileType.Wall ? '#' : '.');
                }
                text.AppendLine();
            }
            return text.ToString();
        }
    }
}
