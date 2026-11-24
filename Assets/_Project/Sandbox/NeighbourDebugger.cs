using UnityEngine;
using UnityEngine.Tilemaps;
using Roguelite.Dungeon;



public class NeighbourDebugger : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] private Camera cam;
    [SerializeField] private Tilemap tilemap;
    [SerializeField] private MapRenderer mapRenderer;

    void Start()
    {
        if(cam == null)
        {
            cam = Camera.main;
        }
        if(mapRenderer == null)
        {
            mapRenderer = GetComponent<MapRenderer>();
        }
        if(cam == null || tilemap == null || mapRenderer == null)
        {
            Debug.LogError("NeighbourDebugger: Missing references. Please assign Camera, Tilemap, and MapRenderer in the Inspector.", this);
            enabled = false; // Disable this script to prevent further errors
        }
    }
    // Update is called once per frame
    void Update()
    {
        if(mapRenderer.Grid == null)
        {
            return; // Grid not yet built, skip processing
        }

        if(Input.GetMouseButtonDown(0))
        {
            Vector3 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
            Vector3Int cellPos = tilemap.WorldToCell(mousePos);
            // Ask the grid everything first, storing each answer in a local variable...
            TileType type = mapRenderer.Grid.Get(cellPos.x, cellPos.y);
            int wallNeighbors4way = mapRenderer.Grid.CountNeighbors(cellPos.x, cellPos.y, TileType.Wall, false);
            int wallNeighbors8way = mapRenderer.Grid.CountNeighbors(cellPos.x, cellPos.y, TileType.Wall, true);
            int floorNeighbors4way = mapRenderer.Grid.CountNeighbors(cellPos.x, cellPos.y, TileType.Floor, false);
            int floorNeighbors8way = mapRenderer.Grid.CountNeighbors(cellPos.x, cellPos.y, TileType.Floor, true);

            // ...then print it all as ONE Console entry per click.
            Debug.Log($"Cell ({cellPos.x}, {cellPos.y}): {type} | " +
                      $"walls {wallNeighbors4way} (4-way), {wallNeighbors8way} (8-way) | " +
                      $"floors {floorNeighbors4way} (4-way), {floorNeighbors8way} (8-way)");
        }
    }
}
