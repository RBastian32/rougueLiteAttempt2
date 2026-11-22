using UnityEngine;
using UnityEngine.Tilemaps;

namespace Roguelite.Dungeon
{
    // LESSON 2 - EXERCISE C: Turn MapGrid DATA into something you can see and bump into.
    //
    // This is the "view" half of the data/view split. MapGrid says WHAT the map is;
    // MapRenderer decides HOW it looks, by painting tiles onto Unity Tilemaps.
    //
    // Floors and walls go on two SEPARATE Tilemaps. Only the wall Tilemap gets a
    // collider, so the player can walk on floors but not through walls.
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
        [Tooltip("If set, this object is moved to the middle of the map when it's built.")]
        [SerializeField] private Transform player;

        // Other scripts (enemies, the minimap...) will read the map through this later.
        // "private set" means only this class can replace it.
        public MapGrid Grid { get; private set; }

        private void Start()
        {
            Build();
        }

        // [ContextMenu] adds this method to the component's ⋮ menu in the Inspector.
        // You can rebuild the map WITHOUT pressing Play, which is handy while designing.
        [ContextMenu("Build Map")]
        public void Build()
        {
            // A missing Inspector reference is the #1 beginner bug in Unity. Checking up
            // front gives a clear message (click it in the Console to highlight this object)
            // instead of a confusing NullReferenceException somewhere else.
            if (floorTilemap == null || wallTilemap == null || floorTile == null || wallTile == null)
            {
                Debug.LogError("MapRenderer: assign both Tilemaps and both Tiles in the Inspector.", this);
                return;
            }

            Grid = CreateTestRoom();
            Debug.Log("Built map:\n" + Grid);   // Uses MapGrid.ToString() from Exercise B.
            Draw(Grid);
            PlacePlayer();
        }

        private MapGrid CreateTestRoom()
        {
            var grid = new MapGrid(width, height);

            // TODO C1: Make a single room. The grid starts as all walls, so carve a
            //          floor rectangle that leaves a 1-tile wall border around the edge.
            //          Use grid.FillRect(new RectInt(x, y, width, height), TileType.Floor).
            //          Think: if the border is 1 tile, where does the floor start, and how
            //          wide is it?

            return grid;
        }

        private void Draw(MapGrid grid)
        {
            // TODO C2: (1) Clear both Tilemaps first (Tilemap.ClearAllTiles), so rebuilding
            //              doesn't leave old tiles behind.
            //          (2) Loop over every cell. Tilemaps use Vector3Int positions:
            //              new Vector3Int(x, y, 0).
            //          (3) Floor cells: floorTilemap.SetTile(position, floorTile).
            //              Wall cells:  wallTilemap.SetTile(position, wallTile).
        }

        private void PlacePlayer()
        {
            // TODO C3: (1) If player is null, return.
            //          (2) Work out the middle cell of the grid (Grid.Width / 2, Grid.Height / 2).
            //          (3) Convert that GRID cell into a WORLD position with
            //              floorTilemap.GetCellCenterWorld(cell), and move the player there.
            //
            //          Grid coordinates (which cell) and world coordinates (where in the scene)
            //          are different things. The Tilemap knows how to convert between them,
            //          even if you later move or scale it.
        }
    }
}
