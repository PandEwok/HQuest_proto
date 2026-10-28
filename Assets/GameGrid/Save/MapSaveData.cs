using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>Content of a save file (serialized as JSON).</summary>
[Serializable]
public class MapSaveData
{
    public int version = 1;
    public string name;
    public long savedAtUtcTicks;
    public int width;
    public int height;
    public List<RoomSaveData> rooms = new List<RoomSaveData>();
    public List<DoorSaveData> doors = new List<DoorSaveData>();

    public DateTime SavedAtLocal => new DateTime(savedAtUtcTicks, DateTimeKind.Utc).ToLocalTime();
}

[Serializable]
public class RoomSaveData
{
    // grid coordinates of the room's tiles
    public List<Vector2Int> tiles = new List<Vector2Int>();
}

[Serializable]
public class DoorSaveData
{
    // grid coordinates of the two tiles separated by the door
    public Vector2Int tileA;
    public Vector2Int tileB;
}