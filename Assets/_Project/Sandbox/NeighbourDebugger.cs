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
            Debug.Log($"Mouse clicked at cell: {cellPos}");
            Debug.Log($"Tile type at cell: {mapRenderer.Grid.Get(cellPos.x, cellPos.y)}");
            TileType type = mapRenderer.Grid.Get(cellPos.x, cellPos.y);
            if(type == TileType.Wall)
            {
                Debug.Log("This is a wall tile.");
            }
            else if (type == TileType.Floor)
            {
                Debug.Log("This is a floor tile.");
            }
            else
            {
                Debug.Log("This is an unknown tile type.");
            }
            int wallNeighbors8way = mapRenderer.Grid.CountNeighbors(cellPos.x, cellPos.y, TileType.Wall, true);
            Debug.Log($"Number of wall neighbors (including diagonals): {wallNeighbors8way}");
            int floorNeighbors8way = mapRenderer.Grid.CountNeighbors(cellPos.x, cellPos.y, TileType.Floor, true);
            Debug.Log($"Number of floor neighbors (including diagonals): {floorNeighbors8way}");
            int wallNeighbors4way = mapRenderer.Grid.CountNeighbors(cellPos.x, cellPos.y, TileType.Wall, false);
            Debug.Log($"Number of wall neighbors (excluding diagonals): {wallNeighbors4way}");
            int floorNeighbors4way = mapRenderer.Grid.CountNeighbors(cellPos.x, cellPos.y, TileType.Floor, false);
            Debug.Log($"Number of floor neighbors (excluding diagonals): {floorNeighbors4way}");
        }
    }
}
