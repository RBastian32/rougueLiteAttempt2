using UnityEngine;
using UnityEngine.Tilemaps;
using Roguelite.Dungeon;



public class NeighbourDebugger : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] private Camera cam;
    [SerializeField] private Tilemap tilemap;
    [SerializeField] private MapRenderer mapRenderer;


    // Update is called once per frame
    void Update()
    {
        if(cam == null || tilemap == null || mapRenderer == null)
        {
            Debug.LogError("NeighbourDebugger: assign Camera, Tilemap, and MapRenderer in the Inspector.", this);
            return;
        }
        
        
        if(Input.GetMouseButtonDown(0))
        {
            Vector3 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
            Vector3Int cellPos = tilemap.WorldToCell(mousePos);
            Debug.Log($"Mouse clicked at cell: {cellPos}");
            Debug.Log($"Tile type at cell: {mapRenderer.Grid.Get(cellPos.x, cellPos.y)}");
            if(mapRenderer.Grid.Get(cellPos.x, cellPos.y) == TileType.Wall)
            {
                Debug.Log("This is a wall tile.");
            }
            else if (mapRenderer.Grid.Get(cellPos.x, cellPos.y) == TileType.Floor)
            {
                Debug.Log("This is a floor tile.");
            }
            else
            {
                Debug.Log("This is an unknown tile type.");
            }
            int wallNeighbors = mapRenderer.Grid.CountNeighbors(cellPos.x, cellPos.y, TileType.Wall, true);
            Debug.Log($"Number of wall neighbors (including diagonals): {wallNeighbors}");
            int floorNeighbors = mapRenderer.Grid.CountNeighbors(cellPos.x, cellPos.y, TileType.Floor, true);
            Debug.Log($"Number of floor neighbors (including diagonals): {floorNeighbors}");
        }
    }
}
