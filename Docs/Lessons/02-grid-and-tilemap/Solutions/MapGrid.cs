// SPOILER: one possible solution for Exercise B. Try it yourself first!
// (This file is outside Assets/, so Unity ignores it.)
using System;
using System.Text;
using UnityEngine;

namespace Roguelite.Dungeon
{
    public enum TileType
    {
        Wall = 0,
        Floor = 1,
    }

    public class MapGrid
    {
        public static readonly Vector2Int[] Directions4 =
        {
            Vector2Int.up,
            Vector2Int.right,
            Vector2Int.down,
            Vector2Int.left,
        };

        // B6a
        public static readonly Vector2Int[] Directions8 =
        {
            Vector2Int.up,
            Vector2Int.right,
            Vector2Int.down,
            Vector2Int.left,
            new Vector2Int(1, 1),
            new Vector2Int(1, -1),
            new Vector2Int(-1, -1),
            new Vector2Int(-1, 1),
        };

        public int Width { get; }
        public int Height { get; }

        private TileType[,] tiles;

        // B1
        public MapGrid(int width, int height)
        {
            if (width < 1 || height < 1)
            {
                throw new ArgumentException($"MapGrid must be at least 1x1, but got {width}x{height}.");
            }

            Width = width;
            Height = height;
            tiles = new TileType[width, height]; // [x, y], so width first.
        }

        // B2: "=>" is shorthand for a method whose body is a single return statement.
        public bool InBounds(int x, int y) => x >= 0 && x < Width && y >= 0 && y < Height;

        // B3a
        public TileType Get(int x, int y)
        {
            if (!InBounds(x, y))
            {
                return TileType.Wall;
            }
            return tiles[x, y];
        }

        // B3b
        public void Set(int x, int y, TileType type)
        {
            if (!InBounds(x, y))
            {
                return;
            }
            tiles[x, y] = type;
        }

        // B4
        public void Fill(TileType type)
        {
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    tiles[x, y] = type;
                }
            }
        }

        // B5
        public void FillRect(RectInt rect, TileType type)
        {
            for (int x = rect.xMin; x < rect.xMax; x++)
            {
                for (int y = rect.yMin; y < rect.yMax; y++)
                {
                    Set(x, y, type);
                }
            }
        }

        // B6b
        public int CountNeighbors(int x, int y, TileType type, bool includeDiagonals)
        {
            Vector2Int[] directions = includeDiagonals ? Directions8 : Directions4;

            int count = 0;
            foreach (Vector2Int offset in directions)
            {
                if (Get(x + offset.x, y + offset.y) == type)
                {
                    count++;
                }
            }
            return count;
        }

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
