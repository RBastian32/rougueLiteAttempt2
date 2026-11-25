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

        // TEMPORARY cave preview, so you can watch Exercise A's random walk in the game.
        // Exercise E replaces this with a proper generator dropdown: delete it then.
        [Header("Cave preview (temporary, until Exercise E)")]
        [SerializeField] private bool previewCave = true;
        [Tooltip("0 = a different random cave every time")]
        [SerializeField] private int caveSeed = 0;
        [SerializeField] private int caveSteps = 800;
        [Tooltip("Widen every dug cell into a 2x2 block, so no passage is narrower than 2 tiles")]
        [SerializeField] private bool widePassages = true;

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

            //Grid = CreateTestRoom();
            Grid = previewCave ? CreateCavePreview() : CreateTwoRooms();
            Debug.Log("Built map:\n" + Grid);   // Uses MapGrid.ToString() from Exercise B.
            Draw(Grid);
            PlacePlayer();
        }

        // TEMPORARY: one random walk from the middle of the map, using your RandomWalk.Carve.
        // The walk starts at the middle cell, which is also where PlacePlayer puts you,
        // so you always spawn on floor.
        private MapGrid CreateCavePreview()
        {
            var grid = new MapGrid(width, height);   // starts as solid Wall
            int usedSeed = caveSeed != 0 ? caveSeed : System.Environment.TickCount;
            Debug.Log($"Cave seed: {usedSeed}");
            var rng = new System.Random(usedSeed);
            RandomWalk.Carve(grid, rng, new Vector2Int(width / 2, height / 2), caveSteps);
            if (widePassages)
                WidenFloors(grid);
            return grid;
        }

        // TEMPORARY: turns every floor cell into a 2x2 block of floor (the cell, plus the
        // ones to its right, above, and above-right). The walker's path is a chain of cells
        // that touch side by side, so the blocks overlap along it and every passage ends up
        // at least 2 tiles wide.
        private static void WidenFloors(MapGrid grid)
        {
            // STEP 1: remember where the floor is BEFORE changing anything. If we widened
            // while scanning, each new floor cell would get widened again further along the
            // same scan, and the floor would spread across the whole map.
            var floors = new System.Collections.Generic.List<Vector2Int>();
            for (int x = 0; x < grid.Width; x++)
                for (int y = 0; y < grid.Height; y++)
                    if (grid.Get(x, y) == TileType.Floor)
                        floors.Add(new Vector2Int(x, y));

            // STEP 2: dig a 2x2 block at each one. The block's bottom-left corner is the
            // cell itself, moved back one step if the block would reach the border wall
            // (the largest interior x is Width - 2, so a 2-wide block may start at Width - 3).
            foreach (Vector2Int c in floors)
            {
                int bx = Mathf.Min(c.x, grid.Width - 3);
                int by = Mathf.Min(c.y, grid.Height - 3);
                grid.FillRect(new RectInt(bx, by, 2, 2), TileType.Floor);
            }
        }

        private MapGrid CreateTestRoom()
        {
            var grid = new MapGrid(width, height);

            // TODO C1: Make a single room. The grid starts as all walls, so carve a
            //          floor rectangle that leaves a 1-tile wall border around the edge.
            //          Use grid.FillRect(new RectInt(x, y, width, height), TileType.Floor).
            //          Think: if the border is 1 tile, where does the floor start, and how
            //          wide is it?
            grid.FillRect(new RectInt(1, 1, width - 2, height - 2), TileType.Floor);

            // CHALLENGE 1: four 2x2 pillars, evenly spaced, worked out from the room size
            // so they stay evenly spaced if you change Width/Height in the Inspector.
            const int pillarSize = 2;

            // Floor area inside the 1-tile border.
            int innerWidth = width - 2;
            int innerHeight = height - 2;

            // Two pillars across leave 3 gaps: wall|gap|pillar|gap|pillar|gap|wall.
            // Integer division rounds down, so any leftover tiles go to the middle gap.
            int gapX = (innerWidth - 2 * pillarSize) / 3;
            int gapY = (innerHeight - 2 * pillarSize) / 3;

            // Left/bottom pillar starts one gap in from the border wall (at 0).
            // Right/top pillar is placed the same distance in from the far border wall.
            int leftX = 1 + gapX;
            int rightX = width - 1 - gapX - pillarSize;
            int bottomY = 1 + gapY;
            int topY = height - 1 - gapY - pillarSize;

            grid.FillRect(new RectInt(leftX, bottomY, pillarSize, pillarSize), TileType.Wall);
            grid.FillRect(new RectInt(rightX, bottomY, pillarSize, pillarSize), TileType.Wall);
            grid.FillRect(new RectInt(leftX, topY, pillarSize, pillarSize), TileType.Wall);
            grid.FillRect(new RectInt(rightX, topY, pillarSize, pillarSize), TileType.Wall);

            return grid;
        }

        // CHALLENGE 2: two rooms side by side, joined by a 3-tile-wide corridor.
        // Designed for a 40 x 20 map (set Width = 40, Height = 20 on the Map object).
        //
        //   room A: columns 2-15, rows 4-15     room B: columns 24-37, rows 3-16
        //   corridor: rows 9-11, columns 16-23 (fills the rock between the rooms)
        private MapGrid CreateTwoRooms()
        {
            var grid = new MapGrid(width, height);   // starts as solid Wall

            // 1. The rooms: read off the sketch as (first column, first row, width, height),
            //    where width = last - first + 1.
            RectInt roomA = new RectInt(2, 4, 14, 12);    // columns 2-15, rows 4-15
            RectInt roomB = new RectInt(24, 3, 14, 14);   // columns 24-37, rows 3-16

            // 2. The corridor is worked out FROM the rooms, so it always fills the gap exactly.
            //    xMax is one PAST a room's last column (half-open), which is exactly where the
            //    corridor should start.
            //
            //    The player's collider is 1 x 1 tile, so a 1-tile corridor is a tight squeeze.
            //    corridorWidth = 3 gives room to spare. It's centred on the middle row (10):
            //    starting 1 row below gives rows 9, 10, 11. All three rows are inside both
            //    rooms, and row 10 still passes through the map's centre cell (20, 10),
            //    which is where PlacePlayer spawns you.
            const int corridorWidth = 3;
            int corridorY = height / 2 - corridorWidth / 2;   // 10 - 1 = 9: bottom row of the corridor
            int corridorX = roomA.xMax;                       // 16: first column after room A
            int corridorLength = roomB.xMin - roomA.xMax;     // 24 - 16 = 8 cells (16-23)
            RectInt corridor = new RectInt(corridorX, corridorY, corridorLength, corridorWidth);

            // 3. Carve them out of the rock. Nothing overlaps a wall we want to keep,
            //    so the order of these three calls doesn't matter.
            grid.FillRect(roomA, TileType.Floor);
            grid.FillRect(roomB, TileType.Floor);
            grid.FillRect(corridor, TileType.Floor);

            return grid;
        }

        private void Draw(MapGrid grid)
        {
            // TODO C2: (1) Clear both Tilemaps first (Tilemap.ClearAllTiles), so rebuilding
            //              doesn't leave old tiles behind.
            floorTilemap.ClearAllTiles();
            wallTilemap.ClearAllTiles();
            //          (2) Loop over every cell. Tilemaps use Vector3Int positions:
            //              new Vector3Int(x, y, 0).
            //          (3) Floor cells: floorTilemap.SetTile(position, floorTile).
            //              Wall cells:  wallTilemap.SetTile(position, wallTile).
            for(int x = 0; x < grid.Width; x++)
            {
                for(int y = 0; y < grid.Height; y++)
                {
                    Vector3Int position = new Vector3Int(x, y, 0);
                    if(grid.Get(x, y) == TileType.Floor)
                    {
                        floorTilemap.SetTile(position, floorTile);
                    }
                    else if(grid.CountNeighbors(x, y, TileType.Floor, true) > 0) // CHALLENGE 2: Only draw walls that touch a floor, so the outer border is invisible.
                    {
                        wallTilemap.SetTile(position, wallTile);
                    }
                }
            }
            
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
            if(player == null)
            {
                return;
            }
            Vector3Int middleCell = new Vector3Int(Grid.Width / 2, Grid.Height / 2, 0);
            Vector3 worldPosition = floorTilemap.GetCellCenterWorld(middleCell);
            player.position = worldPosition;
        }
    }
}
