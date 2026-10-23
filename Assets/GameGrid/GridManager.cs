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

    private Dictionary<Vector2, Tile> _tiles;

    [SerializeField] private Room _roomPrefab;

    bool isMarking = false;
    Vector2 markPositionStart;
    Vector2 markPositionEnd;


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
                var spawnedTile = Instantiate(_tilePrefab, positionOffset, Quaternion.identity, transform);
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
        var room = Instantiate(_roomPrefab, roomPosition, Quaternion.identity);
        //room.transform.localScale = roomScale;

        // READ : add tiles from starting point to end point as follows ; go down/up the line number (2nd number) until it's end point line, then repeat for each column until it's end point column
        int xDif = markPositionStart.x <= markPositionEnd.x ? 1 : -1;
        int yDif = markPositionStart.y <= markPositionEnd.y ? 1 : -1;
        for (int x = (int)(markPositionStart.x); x != (int)(markPositionEnd.x) + xDif; x += xDif)
        {
            for (int y = (int)(markPositionStart.y); y != (int)(markPositionEnd.y) + yDif; y += yDif)
            {
                Tile currentTile;
                if (_tiles.TryGetValue(new Vector2(x, y), out currentTile))
                {
                    room.addTile(currentTile);
                }
            }
        }
    }


    private void Update()
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
                        MakeRoomFromSelection(tile);



                        /*var inverseTiles = _tiles.ToDictionary(x => x.Value, x => x.Key);
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
                        var room = Instantiate(_roomPrefab, roomPosition, Quaternion.identity);
                        //room.transform.localScale = roomScale;

                        // READ : add tiles from starting point to end point as follows ; go down/up the line number (2nd number) until it's end point line, then repeat for each column until it's end point column
                        int xDif = markPositionStart.x <= markPositionEnd.x ? 1 : -1;
                        int yDif = markPositionStart.y <= markPositionEnd.y ? 1 : -1;
                        for (int x = (int)(markPositionStart.x); x != (int)(markPositionEnd.x)+xDif; x += xDif)
                        {
                            for (int y = (int)(markPositionStart.y); y != (int)(markPositionEnd.y) + yDif; y += yDif)
                            {
                                Tile currentTile;
                                if (_tiles.TryGetValue(new Vector2 (x, y), out currentTile))
                                {
                                    room.addTile(currentTile);
                                }
                            }
                        }*/
                    }
                }
            }
            
        }
    }
}
