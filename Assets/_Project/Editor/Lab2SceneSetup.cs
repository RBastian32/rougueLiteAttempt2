using Roguelite.Dungeon;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Tilemaps;

// EDITOR TOOL (not part of the game): does the Lab 2 Exercise C editor setup for you.
// Run it from the menu bar: Roguelite > Lab 2 > Set Up Map Scene.
//
// Anything inside a folder named "Editor" is compiled only into the Unity editor and
// never ships with the game. That's why "using UnityEditor;" is allowed here but would
// break a build if it were in a normal game script.
//
// It's safe to run more than once: it reuses objects that already exist.
public static class Lab2SceneSetup
{
    private const string ArtFolder = "Assets/_Project/Art/";
    private const string ScenePath = "Assets/Scenes/cameraTestScene.unity";

    // [MenuItem] adds a static method to Unity's top menu bar.
    [MenuItem("Roguelite/Lab 2/Set Up Map Scene")]
    private static void SetUp()
    {
        // You can't safely edit the palette prefab while it's open in Prefab Mode.
        if (PrefabStageUtility.GetCurrentPrefabStage() != null)
        {
            EditorUtility.DisplayDialog("Lab 2 setup",
                "Leave Prefab Mode first (click the back arrow at the top of the Hierarchy), then run this again.", "OK");
            return;
        }

        CleanPalette();
        ColorTiles();

        // Open the scene with the Player in it (asks to save the current scene first, if needed).
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        var scene = EditorSceneManager.OpenScene(ScenePath);

        // Step 6-7: one Grid with two Tilemap children.
        GameObject grid = FindOrCreate("Grid", null, typeof(Grid));
        GameObject floor = FindOrCreate("Floor", grid.transform, typeof(Tilemap), typeof(TilemapRenderer));
        GameObject walls = FindOrCreate("Walls", grid.transform, typeof(Tilemap), typeof(TilemapRenderer));

        // Step 8: walls draw on top and are solid.
        walls.GetComponent<TilemapRenderer>().sortingOrder = 1;
        var tileCollider = GetOrAdd<TilemapCollider2D>(walls);
        var body = GetOrAdd<Rigidbody2D>(walls);
        body.bodyType = RigidbodyType2D.Static;
        GetOrAdd<CompositeCollider2D>(walls);
        tileCollider.usedByComposite = true;

        // Step 9: the player draws on top of everything.
        GameObject player = GameObject.Find("Player");
        if (player != null && player.TryGetComponent(out SpriteRenderer sprite))
            sprite.sortingOrder = 10;

        // Steps 10-11: the Map object with MapRenderer and all its references.
        GameObject map = FindOrCreate("Map", null);
        var mapRenderer = GetOrAdd<MapRenderer>(map);

        // MapRenderer's fields are private ([SerializeField]), so we can't write
        // "mapRenderer.floorTilemap = ...". SerializedObject edits them the same way
        // the Inspector does when you drag something into a slot.
        var so = new SerializedObject(mapRenderer);
        so.FindProperty("floorTilemap").objectReferenceValue = floor.GetComponent<Tilemap>();
        so.FindProperty("wallTilemap").objectReferenceValue = walls.GetComponent<Tilemap>();
        so.FindProperty("floorTile").objectReferenceValue = AssetDatabase.LoadAssetAtPath<TileBase>(ArtFolder + "FloorTile.asset");
        so.FindProperty("wallTile").objectReferenceValue = AssetDatabase.LoadAssetAtPath<TileBase>(ArtFolder + "WallTile.asset");
        if (player != null) so.FindProperty("player").objectReferenceValue = player.transform;
        so.ApplyModifiedProperties();

        // Step 12: zoom the camera out a little.
        if (Camera.main != null) Camera.main.orthographicSize = 7f;

        // Paint the map now so you can see it without pressing Play, then save.
        mapRenderer.Build();
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("Lab 2 setup done. Press Play to walk around your room.");
    }

    // Removes the Grid objects that were accidentally built inside the palette.
    private static void CleanPalette()
    {
        string path = ArtFolder + "Dungeon.prefab";
        if (AssetDatabase.LoadAssetAtPath<GameObject>(path) == null) return;

        GameObject root = PrefabUtility.LoadPrefabContents(path);
        // Loop backwards because we're deleting children while looping over them.
        for (int i = root.transform.childCount - 1; i >= 0; i--)
        {
            Transform child = root.transform.GetChild(i);
            if (child.name == "Grid") Object.DestroyImmediate(child.gameObject);
        }
        PrefabUtility.SaveAsPrefabAsset(root, path);
        PrefabUtility.UnloadPrefabContents(root);
    }

    // Step 5: give floors and walls different colours (only if they're still plain white).
    private static void ColorTiles()
    {
        SetColorIfWhite("FloorTile.asset", new Color(0.22f, 0.22f, 0.26f));   // dark grey
        SetColorIfWhite("WallTile.asset", new Color(0.62f, 0.55f, 0.48f));    // light brown
        AssetDatabase.SaveAssets();
    }

    private static void SetColorIfWhite(string file, Color color)
    {
        var tile = AssetDatabase.LoadAssetAtPath<Tile>(ArtFolder + file);
        if (tile == null || tile.color != Color.white) return;
        tile.color = color;
        EditorUtility.SetDirty(tile);   // tells Unity the asset changed and needs saving
    }

    // Finds a root object (or a child of parent) by name, or creates it with the given components.
    private static GameObject FindOrCreate(string name, Transform parent, params System.Type[] components)
    {
        Transform existing = parent != null ? parent.Find(name) : GameObject.Find(name)?.transform;
        if (existing != null && existing.parent == parent) return existing.gameObject;

        var go = new GameObject(name, components);
        if (parent != null) go.transform.SetParent(parent, false);
        Undo.RegisterCreatedObjectUndo(go, "Lab 2 setup");
        return go;
    }

    private static T GetOrAdd<T>(GameObject go) where T : Component
    {
        return go.TryGetComponent(out T existing) ? existing : go.AddComponent<T>();
    }
}
