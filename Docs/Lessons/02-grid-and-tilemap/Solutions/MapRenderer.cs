// SPOILER: one possible solution for Exercise C. Try it yourself first!
// (This file is outside Assets/, so Unity ignores it.)
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Roguelite.Dungeon
{
    public class MapRenderer : MonoBehaviour
    {
        [Header("Map size (in tiles)")]
        [SerializeField] private int width = 20;
        [SerializeField] private int height = 12;

        [Header("Tilemaps (drag from the Hierarchy)")]
        [SerializeField] private Tilemap floorTilemap;
        [SerializeField] private Tilemap wallTilemap;

        [Header("Tiles (drag from the Project window)")]
        [SerializeField] private TileBase floorTile;
        [SerializeField] private TileBase wallTile;

        [Header("Optional")]
        [SerializeField] private Transform player;

        public MapGrid Grid { get; private set; }

        private void Start()
        {
            Build();
        }

        [ContextMenu("Build Map")]
        public void Build()
        {
            if (floorTilemap == null || wallTilemap == null || floorTile == null || wallTile == null)
            {
                Debug.LogError("MapRenderer: assign both Tilemaps and both Tiles in the Inspector.", this);
                return;
            }

            Grid = CreateTestRoom();
            Debug.Log("Built map:\n" + Grid);
            Draw(Grid);
            PlacePlayer();
        }

        private MapGrid CreateTestRoom()
        {
            var grid = new MapGrid(width, height);

            // C1: start at (1,1), and lose one tile on BOTH sides, so -2.
            grid.FillRect(new RectInt(1, 1, width - 2, height - 2), TileType.Floor);

            return grid;
        }

        private void Draw(MapGrid grid)
        {
            // C2
            floorTilemap.ClearAllTiles();
            wallTilemap.ClearAllTiles();

            for (int x = 0; x < grid.Width; x++)
            {
                for (int y = 0; y < grid.Height; y++)
                {
                    var cell = new Vector3Int(x, y, 0);
                    if (grid.Get(x, y) == TileType.Floor)
                    {
                        floorTilemap.SetTile(cell, floorTile);
                    }
                    else
                    {
                        wallTilemap.SetTile(cell, wallTile);
                    }
                }
            }
        }

        private void PlacePlayer()
        {
            // C3
            if (player == null)
            {
                return;
            }

            var cell = new Vector3Int(Grid.Width / 2, Grid.Height / 2, 0);
            player.position = floorTilemap.GetCellCenterWorld(cell);
        }
    }
}
