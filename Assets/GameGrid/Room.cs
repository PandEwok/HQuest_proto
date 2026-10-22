using System.Collections.Generic;
using UnityEngine;

public class Room : MonoBehaviour
{
    //[SerializeField] private GameObject bounds;
    private List<Tile> tiles = new List<Tile>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //bounds.GetComponentInChildren<MeshRenderer>().bounds.size.Set(20, 20, 20);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void addTile(Tile tile)
    {
        // Add the tile to the room's list of tiles
        tiles.Add(tile);
    }
}
