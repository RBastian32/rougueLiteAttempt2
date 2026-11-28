// LESSON 4 SOLUTION E2: the pieces to ADD to your MapRenderer.cs (not a whole file).
//
// 1. A field, next to the other private fields:
//
//        private DungeonResult lastBsp;   // the last BSP dungeon built, for the Gizmos (null otherwise)
//
// 2. In Build(), right after the null check:
//
//        lastBsp = null;
//
//    and in the Bsp case of the switch, after "DungeonResult result = DungeonGenerator.GenerateBsp(...)":
//
//        lastBsp = result;
//
// 3. These methods, anywhere inside the class:

        // Which colour to draw each kind of room in.
        private static Color ColorFor(RoomType role)
        {
            switch (role)
            {
                case RoomType.Spawn: return Color.green;
                case RoomType.Boss: return Color.red;
                case RoomType.Treasure: return Color.yellow;
                case RoomType.Shop: return Color.cyan;
                default: return Color.white;
            }
        }

        // The world position of the middle of a grid cell.
        private Vector3 CellCentre(Vector2Int cell) =>
            floorTilemap.GetCellCenterWorld(new Vector3Int(cell.x, cell.y, 0));

        // Unity calls this in the EDITOR every time it redraws the Scene view (and the Game view,
        // if its Gizmos button is on). Nothing drawn here appears in a built game.
        private void OnDrawGizmos()
        {
            if (lastBsp == null || lastBsp.Graph == null || lastBsp.Roles == null || floorTilemap == null)
                return;

            // The graph: one line per edge, centre to centre.
            Gizmos.color = Color.white;
            foreach ((int a, int b) in lastBsp.Graph.Edges())
            {
                Vector3 from = CellCentre(DungeonGenerator.Center(lastBsp.Rooms[a]));
                Vector3 to = CellCentre(DungeonGenerator.Center(lastBsp.Rooms[b]));
                Gizmos.DrawLine(from, to);
            }

            // The rooms: an outline in the colour of each room's role.
            for (int i = 0; i < lastBsp.Rooms.Count; i++)
            {
                RectInt room = lastBsp.Rooms[i];
                Vector3 corner = floorTilemap.CellToWorld(new Vector3Int(room.xMin, room.yMin, 0));
                Vector3 size = new Vector3(room.width * floorTilemap.cellSize.x,
                                           room.height * floorTilemap.cellSize.y, 0f);
                Gizmos.color = ColorFor(lastBsp.Roles[i]);
                Gizmos.DrawWireCube(corner + size / 2f, size);
            }
        }
