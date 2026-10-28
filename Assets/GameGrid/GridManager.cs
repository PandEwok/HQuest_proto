using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class GridManager : MonoBehaviour
{
    [SerializeField] private int _width, _height;

    [SerializeField] private Tile _tilePrefab;

    [SerializeField] private Transform _cam;

    [HideInInspector] public Dictionary<Vector2, Tile> _tiles;

    [SerializeField] private Room _roomPrefab;
    [SerializeField] private Door _doorPrefab;

    public GameObject CorridorTiles;

    bool isMarking = false;
    Vector2 markPositionStart;
    Vector2 markPositionEnd;

    int roomNumber = 0;


    [HideInInspector]
    public enum Mode
    {
        GAME,
        ADD,
        SELECT
    }
    [HideInInspector] public Mode currentMode = Mode.ADD;

    [HideInInspector]
    public enum Selection
    {
        NONE,
        ROOM,
        DOOR
    }
    [HideInInspector] public Selection currentSelection = Selection.ROOM;


    void Start()
    {
        GenerateGrid();
    }

    [Button]
    void GenerateGrid()
    {
        _tiles = new Dictionary<Vector2, Tile>();
        Clear();
        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                MeshRenderer tileRenderer = _tilePrefab.baseMesh.GetComponent<MeshRenderer>();
                Vector3 positionOffset = new Vector3(x * tileRenderer.bounds.size.x, 0, y * tileRenderer.bounds.size.z);
                var spawnedTile = Instantiate(_tilePrefab, positionOffset, Quaternion.identity, CorridorTiles.transform);
                spawnedTile.name = $"Tile {x} {y}";

                var isOffset = (x % 2 == 0 && y % 2 != 0) || (x % 2 != 0 && y % 2 == 0);
                spawnedTile.Init(isOffset);


                _tiles[new Vector2(x, y)] = spawnedTile;
            }
        }

        //_cam.transform.position = new Vector3((float)_width / 2 - 0.5f, transform.position.y + Mathf.Max(_width, _height)*0.9f, (float)_height / 2 - 0.5f - _height*1.1f);
    }

    [Button]
    void Clear()
    {
        foreach (Tile tile in this.GetComponentsInChildren<Tile>())
        {
            DestroyImmediate(tile.gameObject);
        }
    }

    public Tile GetTileAtPosition(Vector2 pos)
    {
        if (_tiles.TryGetValue(pos, out var tile)) return tile;
        return null;
    }

    List<Tile> GetTilesFromSelection(Vector2 startTile, Vector2 endTile)
    {
        List<Tile> resultList = new List<Tile>();
        int xDif = startTile.x <= endTile.x ? 1 : -1;
        int yDif = startTile.y <= endTile.y ? 1 : -1;

        for (int x = (int)(startTile.x); x != (int)(endTile.x) + xDif; x += xDif)
        {
            for (int y = (int)(startTile.y); y != (int)(endTile.y) + yDif; y += yDif)
            {
                Tile currentTile;
                if (_tiles.TryGetValue(new Vector2(x, y), out currentTile))
                {
                    resultList.Add(currentTile);
                }
            }
        }
        return resultList;
    }


    private void MakeRoomFromSelection(Tile tile)
    {
        var inverseTiles = _tiles.ToDictionary(x => x.Value, x => x.Key);
        inverseTiles.TryGetValue(tile, out markPositionEnd);
        //markPositionEnd = tile.transform.position;

        Debug.Log($"Released on tile: {tile.name}");
        // Create a room based on the marked area
        Tile startTile;
        _tiles.TryGetValue(markPositionStart, out startTile);
        Vector3 poseStart = startTile.transform.position;
        _tiles.TryGetValue(markPositionStart, out startTile);
        Vector3 poseEnd = tile.transform.position;
        Vector3 roomPosition = (poseStart + poseEnd) / 2;

        //Vector3 roomScale = new Vector3(Mathf.Abs(markPositionEnd.x - markPositionStart.x) + 1, 1, Mathf.Abs(markPositionEnd.z - markPositionStart.z) + 1);
        var room = Instantiate(_roomPrefab, roomPosition, Quaternion.identity, transform);
        room.GiveId(roomNumber); roomNumber++;
        //room.transform.localScale = roomScale;

        // READ : add tiles from starting point to end point as follows ; go down/up the line number (2nd number) until it's end point line, then repeat for each column until it's end point column
        /*int xDif = markPositionStart.x <= markPositionEnd.x ? 1 : -1;
        int yDif = markPositionStart.y <= markPositionEnd.y ? 1 : -1;*/

        List<Tile> tilesInSelection = GetTilesFromSelection(markPositionStart, markPositionEnd);
        foreach (Tile curTile in tilesInSelection)
        {
            room.AddTile(curTile);
        }
        room.Start();
        room.UpdateShape();

        /*for (int x = (int)(markPositionStart.x); x != (int)(markPositionEnd.x) + xDif; x += xDif)
        {
            for (int y = (int)(markPositionStart.y); y != (int)(markPositionEnd.y) + yDif; y += yDif)
            {
                Tile currentTile;
                if (_tiles.TryGetValue(new Vector2(x, y), out currentTile))
                {
                    room.AddTile(currentTile);
                }
            }
        }*/
    }


    private void Update()
    {
        // Save menu open (or mouse over the UI): leave the grid alone
        if (SaveLoadMenu.IsBlockingInput)
        {
            if (Input.GetMouseButtonUp(0) && isMarking)
            {
                // selection in progress cancelled: remove the highlights
                isMarking = false;
                foreach (Tile t in _tiles.Values)
                {
                    if (t != null) t._highlight.SetActive(false);
                }
            }
        }
        else if (currentMode == Mode.ADD)
        {
            UpdateAddMode();
        }
        else if (currentMode == Mode.SELECT)
        {
            UpdateSelectMode();
        }


        // verify if rooms still exist
        foreach (Room room in this.GetComponentsInChildren<Room>())
        {
            room.UpdateList();
            if (room.tiles.Count == 0)
            {
                Destroy(room.gameObject);
            }
        }
    }

    private void UpdateSelectMode()
    {
        // LEFT CLICK BEGIN
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                Tile tile = hit.collider.GetComponentInParent<Tile>();
                if (tile != null)
                {
                    isMarking = true;
                    var inverseTiles = _tiles.ToDictionary(x => x.Value, x => x.Key);
                    inverseTiles.TryGetValue(tile, out markPositionStart);
                }
            }
        }
    }

    private void UpdateAddMode()
    {
        // ROOM
        if (currentSelection == Selection.ROOM)
        {
            // LEFT CLICK BEGIN
            if (Input.GetMouseButtonDown(0))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hit))
                {
                    Tile tile = hit.collider.GetComponentInParent<Tile>();
                    if (tile != null)
                    {
                        isMarking = true;
                        //markPositionStart = tile.transform.position;

                        var inverseTiles = _tiles.ToDictionary(x => x.Value, x => x.Key);
                        inverseTiles.TryGetValue(tile, out markPositionStart);

                        //Debug.Log($"Clicked on tile: {tile.name}");
                    }
                }
            }

            // LEFT CLICK HOLD
            if (Input.GetMouseButton(0))
            {
                if (isMarking)
                {
                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                    if (Physics.Raycast(ray, out RaycastHit hit))
                    {
                        Tile tile = hit.collider.GetComponentInParent<Tile>();
                        if (tile != null)
                        {
                            var inverseTiles = _tiles.ToDictionary(x => x.Value, x => x.Key);
                            Vector2 tilePos;
                            inverseTiles.TryGetValue(tile, out tilePos);
                            List<Tile> tilesInSelection = GetTilesFromSelection(markPositionStart, tilePos);

                            foreach (Tile curTile in tilesInSelection)
                            {
                                curTile._highlight.SetActive(true);
                            }
                        }
                    }
                }
            }

            // LEFT CLICK END
            if (Input.GetMouseButtonUp(0))
            {
                if (isMarking)
                {
                    isMarking = false;
                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                    if (Physics.Raycast(ray, out RaycastHit hit))
                    {
                        Tile tile = hit.collider.GetComponentInParent<Tile>();
                        if (tile != null)
                        {
                            Tile startTile;
                            _tiles.TryGetValue(markPositionStart, out startTile);
                            var inverseTiles = _tiles.ToDictionary(x => x.Value, x => x.Key);

                            bool isStartInRoom = false;
                            Room containRoom = null;
                            foreach (Room room in this.GetComponentsInChildren<Room>())
                            {
                                if (room.IsTileInRoom(startTile))
                                {
                                    isStartInRoom = true;
                                    containRoom = room;
                                    break;
                                }
                            }

                            if (isStartInRoom) // start tile is in an existing room ; expending the room
                            {
                                Vector2 tilePos;
                                inverseTiles.TryGetValue(tile, out tilePos);
                                List<Tile> tilesInSelection = GetTilesFromSelection(markPositionStart, tilePos);

                                foreach (Tile curTile in tilesInSelection)
                                {
                                    if (containRoom != null && !containRoom.IsTileInRoom(curTile))
                                    {
                                        containRoom.AddTile(curTile);
                                    }
                                }

                                foreach (Tile curTile in tilesInSelection)
                                {
                                    if (curTile != tile)
                                    { curTile._highlight.SetActive(false); }
                                }
                                containRoom.UpdateShape();
                            }
                            else // creating new room
                            {
                                MakeRoomFromSelection(tile);

                                Vector2 tilePos;
                                inverseTiles.TryGetValue(tile, out tilePos);
                                List<Tile> tilesInSelection = GetTilesFromSelection(markPositionStart, tilePos);

                                foreach (Tile curTile in tilesInSelection)
                                {
                                    if (curTile != tile)
                                    { curTile._highlight.SetActive(false); }
                                }
                            }
                        }
                    }
                }
            }
        }

        // DOOR
        if (currentSelection == Selection.DOOR)
        {
            // add door (left click)
            if (Input.GetMouseButtonDown(0))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hit))
                {
                    GameObject wall = hit.collider.gameObject;
                    if (wall.CompareTag("Wall"))
                    {
                        ReplaceByDoor(wall);
                    }
                }
            }
        }
    }


    // =====================================================================
    //  SAVE / LOAD
    // =====================================================================

    public int GridWidth => _width;
    public int GridHeight => _height;

    /// <summary>Captures the current map (rooms + doors) into a serializable object.</summary>
    public MapSaveData CaptureMap(string mapName)
    {
        var data = new MapSaveData
        {
            name = mapName,
            savedAtUtcTicks = DateTime.UtcNow.Ticks,
            width = _width,
            height = _height
        };

        // Tile -> grid coordinates
        var coords = new Dictionary<Tile, Vector2>();
        foreach (var kv in _tiles)
        {
            if (kv.Value != null) coords[kv.Value] = kv.Key;
        }

        foreach (Room room in GetComponentsInChildren<Room>())
        {
            room.UpdateList();
            if (room.tiles.Count == 0) continue;

            var roomData = new RoomSaveData();
            foreach (Tile tile in room.tiles)
            {
                if (coords.TryGetValue(tile, out Vector2 c))
                {
                    roomData.tiles.Add(new Vector2Int(Mathf.RoundToInt(c.x), Mathf.RoundToInt(c.y)));
                }
            }
            data.rooms.Add(roomData);
        }

        var doneDoors = new HashSet<string>();
        foreach (Door door in GetComponentsInChildren<Door>())
        {
            if (door.parentTiles == null || door.parentTiles.Count < 2) continue;
            if (!coords.TryGetValue(door.parentTiles[0], out Vector2 a)) continue;
            if (!coords.TryGetValue(door.parentTiles[1], out Vector2 b)) continue;

            var doorData = new DoorSaveData
            {
                tileA = new Vector2Int(Mathf.RoundToInt(a.x), Mathf.RoundToInt(a.y)),
                tileB = new Vector2Int(Mathf.RoundToInt(b.x), Mathf.RoundToInt(b.y))
            };

            // avoid duplicates
            string key = doorData.tileA.x < doorData.tileB.x || (doorData.tileA.x == doorData.tileB.x && doorData.tileA.y <= doorData.tileB.y)
                ? $"{doorData.tileA}-{doorData.tileB}"
                : $"{doorData.tileB}-{doorData.tileA}";
            if (doneDoors.Add(key)) data.doors.Add(doorData);
        }

        return data;
    }

    /// <summary>Replaces the current map with the one contained in <paramref name="data"/>.</summary>
    public bool ApplyMap(MapSaveData data)
    {
        if (data == null) return false;

        // 1. clear everything
        isMarking = false;
        foreach (Room room in GetComponentsInChildren<Room>())
        {
            if (room != null) DestroyImmediate(room.gameObject);
        }
        if (CorridorTiles != null)
        {
            for (int i = CorridorTiles.transform.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(CorridorTiles.transform.GetChild(i).gameObject);
            }
        }

        // 2. regenerate the grid (at the saved size)
        if (data.width > 0 && data.height > 0)
        {
            _width = data.width;
            _height = data.height;
        }
        GenerateGrid();
        roomNumber = 0;

        // 3. recreate the rooms (all rooms first, then their walls)
        var createdRooms = new List<Room>();
        foreach (RoomSaveData roomData in data.rooms)
        {
            var roomTiles = new List<Tile>();
            foreach (Vector2Int c in roomData.tiles)
            {
                if (TryGetTile(c, out Tile tile)) roomTiles.Add(tile);
            }
            if (roomTiles.Count == 0) continue;

            Vector3 center = Vector3.zero;
            foreach (Tile tile in roomTiles) center += tile.transform.position;
            center /= roomTiles.Count;

            var room = Instantiate(_roomPrefab, center, Quaternion.identity, transform);
            room.GiveId(roomNumber); roomNumber++;
            foreach (Tile tile in roomTiles) room.AddTile(tile);
            room.Start();
            createdRooms.Add(room);
        }
        foreach (Room room in createdRooms) room.UpdateShape();

        // 4. put the doors back
        foreach (DoorSaveData doorData in data.doors)
        {
            if (TryGetTile(doorData.tileA, out Tile a) && TryGetTile(doorData.tileB, out Tile b))
            {
                GameObject wall = FindWallBetween(a, b);
                if (wall != null) ReplaceByDoor(wall);
                else Debug.LogWarning($"Load: no wall between {doorData.tileA} and {doorData.tileB}, door skipped.");
            }
        }

        return true;
    }

    bool TryGetTile(Vector2Int coords, out Tile tile)
    {
        tile = null;
        return _tiles != null && _tiles.TryGetValue(new Vector2(coords.x, coords.y), out tile) && tile != null;
    }

    GameObject FindWallBetween(Tile a, Tile b)
    {
        foreach (GameObject w in a.walls)
        {
            if (w == null) continue;
            Wall wall = w.GetComponent<Wall>();
            if (wall != null && wall.parentTiles.Contains(b)) return w;
        }
        return null;
    }


    public void ReplaceByDoor(GameObject target)
    {
        var door = Instantiate(_doorPrefab, target.transform.position, target.transform.rotation, target.transform.parent);
        Wall wall = target.GetComponent<Wall>();
        List<Tile> parentTiles = new List<Tile>();
        if (wall != null)
        {
            parentTiles = wall.parentTiles;
            door.parentTiles = new List<Tile>(wall.parentTiles); // needed for saving
            foreach (Tile tile in parentTiles)
            {
                tile.walls.Remove(target);
                foreach (Room room in this.GetComponentsInChildren<Room>())
                {
                    if (room.tiles.Contains(tile))
                    {
                        room.doors.Add(door.gameObject);
                        break;
                    }
                }
            }
        }
        else { Debug.Log("wall NULL)"); }

        Destroy(target);

        if (parentTiles.Count > 0)
        {
            foreach (Tile tile in parentTiles)
            {
                foreach (Room room in this.GetComponentsInChildren<Room>())
                {
                    if (room.tiles.Contains(tile))
                    {
                        room.UpdateShape();
                    }
                }
            }
        }
    }

    public Vector2 GetTileCoords(Tile tile)
    {
        foreach (var kvp in _tiles)
        {
            if (kvp.Value == tile)
                return kvp.Key;
        }

        return Vector2.zero;
    }

    public List<Tile> GetNeighbours(Tile tile)
    {
        List<Tile> result = new();

        Vector2 pos = GetTileCoords(tile);

        Vector2[] dirs =
        {
        Vector2.up,
        Vector2.down,
        Vector2.left,
        Vector2.right
        };

        foreach (Vector2 dir in dirs)
        {
            if (_tiles.TryGetValue(pos + dir, out Tile neighbour))
            {
                if (CanMoveBetween(tile, neighbour))
                {
                    result.Add(neighbour);
                }
            }
        }

        return result;
    }

    public bool CanMoveBetween(Tile from, Tile to)
    {
        foreach (GameObject wallObj in from.walls)
        {
            if (wallObj == null)
                continue;

            Wall wall = wallObj.GetComponent<Wall>();

            if (wall == null)
                continue;

            if (wall.parentTiles.Contains(to))
            {
                return false;
            }
        }

        return true;
    }
}