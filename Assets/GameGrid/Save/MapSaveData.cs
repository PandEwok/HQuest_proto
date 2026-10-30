using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>Content of a save file (serialized as JSON).</summary>
[Serializable]
public class MapSaveData
{
    // 1 = rooms + doors only (old saves, walls/props regenerated randomly)
    // 2 = + walls, props (furniture, traps...), secret doors, door rotation
    // 3 = + monsters
    public int version = 3;
    public string name;
    public long savedAtUtcTicks;
    public int width;
    public int height;
    public List<RoomSaveData> rooms = new List<RoomSaveData>();
    public List<DoorSaveData> doors = new List<DoorSaveData>();
    public List<WallSaveData> walls = new List<WallSaveData>();
    public List<PropSaveData> props = new List<PropSaveData>();
    public List<MonsterSaveData> monsters = new List<MonsterSaveData>();   // v3

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
    public bool isSecret;          // v2: SecretDoor instead of Door
    public Vector3 localEuler;     // v2: rotation relative to its tile
}

/// <summary>A wall (plain, torch, banner, barrel, shelf...) between two tiles.</summary>
[Serializable]
public class WallSaveData
{
    public Vector2Int tileA;       // tile the wall is parented to (owner)
    public Vector2Int tileB;       // tile on the other side
    public string prefab;          // prefab name (e.g. "WallTorch_PF")
    public Vector3 localEuler;
}

/// <summary>An object placed on a tile (furniture, trap, rock, stairs...).</summary>
[Serializable]
public class PropSaveData
{
    public Vector2Int tile;
    public string prefab;          // prefab name (e.g. "Chest1_PF")
    public Vector3 localPosition;
    public Vector3 localEuler;
    public Vector3 localScale = Vector3.one;
}

/// <summary>A monster placed on a tile.</summary>
[Serializable]
public class MonsterSaveData
{
    public Vector2Int tile;
    public string monster;         // monster id (e.g. "Goblin", "DreadWarrior"), see GridManager.BuildEnemyRegistry
    public Vector3 localEuler;     // world rotation of the monster
}