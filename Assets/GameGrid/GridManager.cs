using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.GraphicsBuffer;

public class GridManager : MonoBehaviour
{
    [SerializeField] private int _width, _height;

    [SerializeField] private Tile _tilePrefab;

    [SerializeField] private Transform _cam;

    [HideInInspector] public Dictionary<Vector2, Tile> _tiles;

    [SerializeField] private Room _roomPrefab;
    [SerializeField] private Door _doorPrefab;

    [SerializeField] private GameObject RockPF;
    [SerializeField] private GameObject TrapPF;
    [SerializeField] private GameObject WeaponRack1PF_H;
    [SerializeField] private GameObject WeaponRack1PF_V;
    [SerializeField] private GameObject WeaponRack2PF_H;
    [SerializeField] private GameObject WeaponRack2PF_V;
    [SerializeField] private GameObject Cupboard1PF_H;
    [SerializeField] private GameObject Cupboard1PF_V;
    [SerializeField] private GameObject Cupboard2PF_H;
    [SerializeField] private GameObject Cupboard2PF_V;
    [SerializeField] private GameObject Table1PF_H;
    [SerializeField] private GameObject Table1PF_V;
    [SerializeField] private GameObject Table2PF_H;
    [SerializeField] private GameObject Table2PF_V;
    [SerializeField] private GameObject Chest1PF;
    [SerializeField] private GameObject Chest2PF;


    public GameObject CorridorTiles;

    bool isMarking = false;
    Vector2 markPositionStart;
    Vector2 markPositionEnd;

    int roomNumber = 0;

    int PropType;


    [HideInInspector] public enum Mode
    {
        GAME,
        ADD,
        SELECT,
        REMOVE

    }
    [HideInInspector] public Mode currentMode = Mode.ADD;

    [HideInInspector] public enum Selection
    {
        NONE,
        ROOM,
        DOOR,
        ROCK,
        TRAP,
        WRV, //WeaponRackVertical
        WRH,  //WeaponRackHorizontal
        CUPV,
        CUPH,
        TABLEV,
        TABLEH,
        CHEST
    }
    [HideInInspector] public Selection currentSelection = Selection.ROOM;


    void Start()
    {
        GenerateGrid();
    }

    [Button] void GenerateGrid()
    {
        _tiles = new Dictionary<Vector2, Tile>();
        Clear();
        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                MeshRenderer tileRenderer = _tilePrefab.baseMesh.GetComponent<MeshRenderer>();
                Vector3 positionOffset = new Vector3(x * tileRenderer.bounds.size.x, 0, y * tileRenderer.bounds.size.z);
                var spawnedTile = Instantiate(_tilePrefab, positionOffset, Quaternion.identity,CorridorTiles.transform);
                spawnedTile.name = $"Tile {x} {y}";

                var isOffset = (x % 2 == 0 && y % 2 != 0) || (x % 2 != 0 && y % 2 == 0);
                spawnedTile.Init(isOffset);


                _tiles[new Vector2(x, y)] = spawnedTile;
            }
        }

        //_cam.transform.position = new Vector3((float)_width / 2 - 0.5f, transform.position.y + Mathf.Max(_width, _height)*0.9f, (float)_height / 2 - 0.5f - _height*1.1f);
    }

    [Button] void Clear()
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
        if (currentMode == Mode.ADD)
        {
            UpdateAddMode();
        }
        else if (currentMode == Mode.SELECT)
        {
            UpdateSelectMode();
        }
        else if (currentMode == Mode.REMOVE)
        {
            UpdateRemoveMode();
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

    private void UpdateRemoveMode()
    {
        // LEFT CLICK BEGIN
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                GameObject tile = hit.collider.gameObject;
                if (!tile.CompareTag("Tile"))
                {
                    Destroy(tile);
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
        else if (currentSelection == Selection.DOOR) // DOOR
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
        else // PROPS
        {
            if (Input.GetMouseButtonDown(0))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hit))
                {
                    GameObject tile = hit.collider.gameObject;
                    if (tile.CompareTag("Tile"))
                    {
                        PropType = UnityEngine.Random.Range(0, 2);
                        if (currentSelection == Selection.ROCK)
                        {
                            Instantiate(RockPF, tile.transform);
                        }

                        if (currentSelection == Selection.TRAP)
                        {
                            Instantiate(TrapPF, tile.transform);
                        }

                        if (currentSelection == Selection.WRV)
                        {
                            if (PropType == 1)
                            {
                                Instantiate(WeaponRack1PF_V, tile.transform);
                            }
                            else
                            {
                                Instantiate(WeaponRack2PF_V, tile.transform);
                            }
                        }

                        if (currentSelection == Selection.WRH)
                        {
                            if (PropType == 1)
                            {
                                Instantiate(WeaponRack1PF_H, tile.transform);
                            }
                            else
                            {
                                Instantiate(WeaponRack2PF_H, tile.transform);
                            }
                        }

                        if (currentSelection == Selection.CUPH)
                        {
                            if (PropType == 1)
                            {
                                Instantiate(Cupboard1PF_H, tile.transform);
                            }
                            else
                            {
                                Instantiate(Cupboard2PF_H, tile.transform);
                            }
                        }

                        if (currentSelection == Selection.CUPV)
                        {
                            if (PropType == 1)
                            {
                                Instantiate(Cupboard1PF_V, tile.transform);
                            }
                            else
                            {
                                Instantiate(Cupboard2PF_V, tile.transform);
                            }
                        }

                        if (currentSelection == Selection.TABLEH)
                        {
                            if (PropType == 1)
                            {
                                Instantiate(Table1PF_H, tile.transform);
                            }
                            else
                            {
                                Instantiate(Table2PF_H, tile.transform);
                            }
                        }

                        if (currentSelection == Selection.TABLEV)
                        {
                            if (PropType == 1)
                            {
                                Instantiate(Table1PF_V, tile.transform);
                            }
                            else
                            {
                                Instantiate(Table2PF_V, tile.transform);
                            }
                        }

                        if (currentSelection == Selection.CHEST)
                        {
                            if (PropType == 1)
                            {
                                Instantiate(Chest1PF, tile.transform);
                            }
                            else
                            {
                                Instantiate(Chest2PF, tile.transform);
                            }
                        }
                    }
                }
            }
        }
    }


    void ReplaceByDoor(GameObject target)
    {
        var door = Instantiate(_doorPrefab, target.transform.position, target.transform.rotation, target.transform.parent);
        Wall wall = target.GetComponent<Wall>();
        List<Tile> parentTiles = new List<Tile>();
        if (wall != null)
        {
            parentTiles = wall.parentTiles;
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
}
