using System.Collections.Generic;
using UnityEngine;
using System.Linq;

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
    [SerializeField] private GameObject wallPropPF5; // already serialized in Room.prefab
    [SerializeField] private GameObject rockFillPF;
    [SerializeField] private GameObject rockEdgePF;

    [HideInInspector] public List<GameObject> rocks = new List<GameObject>();



    private GameObject wallPFType;

    // ------------------------------------------------------------------
    //  SAVE / LOAD support
    // ------------------------------------------------------------------
    /// <summary>A wall imposed by a save file (replaces the random choice).</summary>
    public class ForcedWall
    {
        public GameObject prefab;
        public Tile owner;          // tile the wall is parented to
        public Vector3 localEuler;
    }

    /// <summary>When not null (set by GridManager.ApplyMap), UpdateShape builds exactly these walls.</summary>
    [System.NonSerialized] public Dictionary<(Tile, Tile), ForcedWall> forcedWalls;

    public static (Tile, Tile) PairKey(Tile a, Tile b)
    {
        return a.GetInstanceID() <= b.GetInstanceID() ? (a, b) : (b, a);
    }

    public static string StripClone(string n)
    {
        const string suffix = "(Clone)";
        while (n.EndsWith(suffix)) n = n.Substring(0, n.Length - suffix.Length);
        return n.Trim();
    }

    GameObject[] KnownWallPrefabs()
    {
        return new[] { wallPF, wallTorchPF, wallPropPF1, wallPropPF2, wallPropPF3, wallPropPF4, wallPropPF5 };
    }

    /// <summary>Prefab name of a spawned wall (null if it is not one of this room's wall prefabs).</summary>
    public string GetWallId(GameObject instance)
    {
        string n = StripClone(instance.name);
        foreach (GameObject pf in KnownWallPrefabs())
        {
            if (pf != null && pf.name == n) return pf.name;
        }
        return null;
    }

    static bool HasDoorAt(Tile t, Vector3 pos)
    {
        foreach (Door d in t.GetComponentsInChildren<Door>())
        {
            if ((d.transform.position - pos).sqrMagnitude < 0.0001f) return true;
        }
        foreach (SecretDoor d in t.GetComponentsInChildren<SecretDoor>())
        {
            if ((d.transform.position - pos).sqrMagnitude < 0.0001f) return true;
        }
        return false;
    }

    public GameObject FindWallPrefab(string id)
    {
        foreach (GameObject pf in KnownWallPrefabs())
        {
            if (pf != null && pf.name == id) return pf;
        }
        return null;
    }

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
            if (wall != null)
            {
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
        Color newColor = new Color(1.7f, 1.68f, 1.62f);

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
                            if (forcedWalls != null)
                            {
                                // Loading a save: the saved walls are authoritative. Ignore any rock / wall of the
                                // neighbouring room that is still in the hierarchy (Destroy() is deferred to the end of the
                                // frame, so they are still "present" during the load) and only yield to a real door.
                                wallAlreadyPresent = HasDoorAt(tile, tileWorldPos) || HasDoorAt(nTile, tileWorldPos);
                            }
                            if (!wallAlreadyPresent)
                            {
                                ForcedWall forced = null;
                                bool spawnWall = true;

                                if (forcedWalls != null)
                                {
                                    // loading a save: build exactly the saved wall (or none)
                                    if (forcedWalls.TryGetValue(PairKey(tile, nTile), out forced))
                                    {
                                        wallPFType = forced.prefab;
                                        spawnWall = forced.owner == tile;
                                    }
                                    else spawnWall = false;
                                }
                                else
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

                                }

                                if (spawnWall)
                                {
                                    var spawnedWall = Instantiate(wallPFType, tileWorldPos, Quaternion.identity, tile.transform);
                                    spawnedWall.transform.LookAt(tile.surface.transform);
                                    if (forced != null) spawnedWall.transform.localEulerAngles = forced.localEuler;

                                    // adding to relationship lists
                                    walls.Add(spawnedWall);
                                    tile.walls.Add(spawnedWall);
                                    nTile.walls.Add(spawnedWall);
                                    spawnedWall.GetComponent<Wall>().parentTiles.Add(tile);
                                    spawnedWall.GetComponent<Wall>().parentTiles.Add(nTile);
                                }
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