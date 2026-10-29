using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Unity.VisualScripting;

public class Room : MonoBehaviour
{
    //[SerializeField] private GameObject bounds;
    [HideInInspector] public List<Tile> tiles = new List<Tile>();
    [HideInInspector] public List<GameObject> walls = new List<GameObject>();
    [HideInInspector] public List<GameObject> doors = new List<GameObject>();
    int identifier = 0;

    private GridManager manager;

    [SerializeField] private GameObject wallPF;
    [SerializeField] private GameObject wallTorchPF;
    [SerializeField] private GameObject wallPropPF1;
    [SerializeField] private GameObject wallPropPF2;
    [SerializeField] private GameObject wallPropPF3;
    [SerializeField] private GameObject wallPropPF4;
    [SerializeField] private GameObject wallPropPF5;
    [SerializeField] private GameObject rockFillPF;
    [SerializeField] private GameObject rockEdgePF;

    [HideInInspector] public List<GameObject> rocks = new List<GameObject>();



    private GameObject wallPFType;

    private int wallType;
    private int PropType;
    private bool needTorch;
    private int wallCount;
    private int torchStart;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void Start()
    {
        //bounds.GetComponentInChildren<MeshRenderer>().bounds.size.Set(20, 20, 20);
        manager = transform.parent.GetComponent<GridManager>();

        /*var root = new GameObject(RootName).transform;
        root.SetParent(transform, false);*/
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public bool IsTileInRoom(Tile tile)
    {
        if (tiles.Contains(tile))
        {
            return true;
        }
        return false;
    }

    public void UpdateList()
    {
        tiles = new List<Tile>();
        foreach (Tile tile in this.GetComponentsInChildren<Tile>())
        {
            tiles.Add(tile);
        }
        /*if ((transform.GetComponentsInChildren<Tile>()).Length == 0)
        {
            Destroy(this);
        }*/
    }

    void ClearWalls()
    {
        List<GameObject> wallsTempList = new List<GameObject>();
        foreach (GameObject obj in walls)
        {
            wallsTempList.Add(obj);
        }
        walls.Clear();
        foreach (GameObject wall in wallsTempList)
        {
            if (wall != null) {
                foreach (Tile tile in wall.GetComponent<Wall>().parentTiles)
                {
                    tile.walls.Remove(wall);
                }
            }

            Destroy(wall);
        }
    }
    void ClearRocks()
    {
        List<GameObject> rocksTempList = new List<GameObject>();
        foreach (GameObject obj in rocks)
        {
            rocksTempList.Add(obj);
        }
        rocks.Clear();
        foreach (GameObject rock in rocksTempList)
        {
            Destroy(rock);
        }
    }

    public void UpdateShape()
    {
        // tile color
        Color newColor = new Color(1.7f,1.68f,1.62f);

        foreach (Tile tile in tiles)
        {
            tile.ChangeColor(newColor);
        }


        ClearWalls(); ClearRocks();
        // walls
        if (manager != null)
        {
            torchStart = Random.Range(0, 5);
            needTorch = true;
            wallCount = 0;
            foreach (Tile tile in tiles)
            {
                var inverseTiles = manager._tiles.ToDictionary(x => x.Value, x => x.Key);
                Vector2 tilePos;
                inverseTiles.TryGetValue(tile, out tilePos);

                Vector2 upTile = tilePos + new Vector2(0, 1);
                Vector2 downTile = tilePos + new Vector2(0, -1);
                Vector2 rightTile = tilePos + new Vector2(1, 0);
                Vector2 leftTile = tilePos + new Vector2(-1, 0);
                List<Vector2> neighborTiles = new List<Vector2>() { upTile, downTile, rightTile, leftTile };

                int notForeignTiles = 0;
                foreach (Vector2 neighbor in neighborTiles)
                {
                    Tile nTile; manager._tiles.TryGetValue(neighbor, out nTile);
                    if (nTile != null && !tiles.Contains(nTile))
                    {
                        if (nTile != null)
                        {
                            Vector3 tileWorldPos = tile.surface.transform.position;
                            tileWorldPos += (nTile.surface.transform.position - tileWorldPos) / 2f;

                            bool wallAlreadyPresent = false;
                            // check if wall already exists in neighbor
                            foreach (Transform child in nTile.transform.GetComponentsInChildren<Transform>())
                            {
                                if (child.CompareTag("Wall") || child.CompareTag("Door"))
                                {
                                    if (child.position == tileWorldPos)
                                    {
                                        wallAlreadyPresent = true;

                                        if (child.CompareTag("Door") && !doors.Contains(child.gameObject))
                                        {
                                            doors.Add(child.gameObject);
                                        }
                                    }
                                }
                            }
                            foreach (Transform child in tile.transform.GetComponentsInChildren<Transform>())
                            {
                                if (child.CompareTag("Door"))
                                {
                                    if (child.position == tileWorldPos)
                                    {
                                        wallAlreadyPresent = true;
                                    }
                                }
                            }
                            if (!wallAlreadyPresent)
                            {
                                if (!needTorch)
                                {
                                    if (wallCount == 3)
                                    {
                                        wallType = Random.Range(0, 5);
                                        if (wallType == 1)
                                        {
                                            wallPFType = wallTorchPF;
                                        }
                                        wallCount = 0;
                                    }
                                    else
                                    {
                                        PropType = Random.Range(0, 10);
                                        switch (PropType)
                                        {
                                            case 1:
                                                wallPFType = wallPropPF1;
                                                break;
                                            case 2:
                                                wallPFType = wallPropPF2;
                                                break;
                                            case 3:
                                                wallPFType = wallPropPF3;
                                                break;
                                            case 4:
                                                wallPFType = wallPropPF4;
                                                break;
                                            case 5:
                                                wallPFType = wallPropPF5;
                                                break;
                                            default:
                                                wallPFType = wallPF;
                                                break;
                                        }
                                        wallCount++;
                                    }
                                }
                                else
                                {
                                    if (torchStart == wallCount)
                                    {
                                        wallPFType = wallTorchPF;
                                        needTorch = false;
                                    }
                                    else
                                    {
                                        wallPFType = wallPF;
                                        wallCount++;
                                    }
                                }

                                var spawnedWall = Instantiate(wallPFType, tileWorldPos, Quaternion.identity, tile.transform);
                                spawnedWall.transform.LookAt(tile.surface.transform);

                                // adding to relationship lists
                                walls.Add(spawnedWall);
                                tile.walls.Add(spawnedWall);
                                nTile.walls.Add(spawnedWall);
                                spawnedWall.GetComponent<Wall>().parentTiles.Add(tile);
                                spawnedWall.GetComponent<Wall>().parentTiles.Add(nTile);
                            }
                            var spawnedRock = Instantiate(rockEdgePF, tileWorldPos, Quaternion.identity, tile.transform);
                            spawnedRock.transform.LookAt(tile.surface.transform);
                            // adding rock to relationship lists
                            rocks.Add(spawnedRock);
                            tile.walls.Add(spawnedRock);
                            nTile.walls.Add(spawnedRock);
                            spawnedRock.GetComponent<Wall>().parentTiles.Add(tile);
                            spawnedRock.GetComponent<Wall>().parentTiles.Add(nTile);
                        }
                    }
                    else
                    {
                        notForeignTiles++;
                    }
                }
                if (notForeignTiles >= 4)
                {

                    if (doors.Count < 1)
                    {
                        var spawnedRock = Instantiate(rockFillPF, tile.surface.transform.position, Quaternion.identity, tile.transform);
                        rocks.Add(spawnedRock);
                    }
                }
            }
            if (doors.Count < 1)
            {
                ClearWalls();
            }
            else
            {
                ClearRocks();
            }
        }
        //Debug.Log("room number of tile : " + tiles.Count);
        
        //Debug.Log(this.name + " number of doors " + doors.Count);
    }

    public void GiveId(int id = 0)
    {
        identifier = id;
    }

    public void AddTile(Tile tile)
    {
        // Add the tile to the room's list of tiles
        if (!tiles.Contains(tile))
        {
            tiles.Add(tile);
        }
        
        tile.transform.parent = transform;
    }
}
