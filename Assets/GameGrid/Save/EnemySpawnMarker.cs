using UnityEngine;

/// <summary>
/// Added automatically by GridManager to every monster placed with the map editor.
/// It remembers which prefab the monster comes from (all the monster prefabs are named "Enemy"),
/// so that the monster can be saved and reloaded. Nothing to set up by hand.
/// </summary>
public class EnemySpawnMarker : MonoBehaviour
{
    public string monsterId;   // "Goblin", "Skeleton", "Zombi", "Orc", "Abomination", "Mummy", "DreadWarrior", "Gargoyle"
}
