using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Unity.VisualScripting;

public class Room : MonoBehaviour
{
    //[SerializeField] private GameObject bounds;
    public List<Tile> tiles = new List<Tile>();
    public List<GameObject> walls = new List<GameObject>();
    int identifier = 0;

    private GridManager manager;

    [SerializeField] private GameObject wallPF;

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
            Destroy(wall);
        }
    }

    public void UpdateShape()
    {
        // tile color
        Color newColor = new Color(1.7f,1.68f,1.62f);
        /*switch (identifier)
        {
            case 0:
                newColor = Color.blue;
                break;
            case 1:
                newColor = Color.red;
                break;
            case 2:
                newColor = Color.yellow;
                break;
            case 3:
                newColor = Color.green;
                break;
            default:
                newColor = Color.blue;
                break;
        }*/

        foreach (Tile tile in tiles)
        {
            tile.ChangeColor(newColor);
        }


        ClearWalls();
        // walls
        if (manager != null)
        {
            foreach (Tile tile in tiles)
            {
                var inverseTiles = manager._tiles.ToDictionary(x => x.Value, x => x.Key);
                Vector2 tilePos;
                inverseTiles.TryGetValue(tile, out tilePos);

                Vector2 upTile = tilePos + new Vector2(0,1);
                Vector2 downTile = tilePos + new Vector2(0, -1);
                Vector2 rightTile = tilePos + new Vector2(1, 0);
                Vector2 leftTile = tilePos + new Vector2(-1, 0);
                List<Vector2> neighborTiles = new List<Vector2>() { upTile, downTile, rightTile, leftTile };

                foreach (Vector2 neighbor in neighborTiles)
                {
                    Tile nTile; manager._tiles.TryGetValue(neighbor, out nTile);
                    if (nTile != null && !tiles.Contains(nTile))
                    {
                        if (nTile != null)
                        {
                            Vector3 tileWorldPos = tile.transform.position;
                            tileWorldPos += (nTile.transform.position - tileWorldPos) / 2f;
                            var spawnedWall = Instantiate(wallPF, tileWorldPos, Quaternion.identity, nTile.transform);
                            spawnedWall.transform.LookAt(tile.transform);
                            walls.Add(spawnedWall);
                        }
                    }
                }
            }
        }
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
