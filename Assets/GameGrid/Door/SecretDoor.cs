using System.Collections.Generic;
using UnityEngine;

public class SecretDoor : MonoBehaviour
{
    // The two tiles on either side of the door (filled in by GridManager.ReplaceByDoor).
    // Used to save / reload the door.
    [HideInInspector] public List<Tile> parentTiles = new List<Tile>();
}