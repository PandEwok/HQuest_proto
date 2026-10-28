using System.Collections.Generic;
using UnityEngine;

public class TrapManager : MonoBehaviour
{
    [SerializeField] private CombatDiceManager combatDiceManager;

    // Dictionnaire associant une position sur la grille à un piège
    private Dictionary<Vector2, ITrap> activeTraps = new Dictionary<Vector2, ITrap>();

    public void RegisterTrap(Vector2 position, ITrap trap)
    {
        if (!activeTraps.ContainsKey(position))
        {
            activeTraps.Add(position, trap);
        }
    }

    public ITrap GetTrapAt(Vector2 position)
    {
        activeTraps.TryGetValue(position, out var trap);
        return trap;
    }

    /// <summary>
    /// À appeler quand un héros effectue une action de recherche de pièges autour de lui.
    /// </summary>
    public void SearchForTrapsInArea(List<Vector2> searchablePositions, Character hero)
    {
        foreach (var pos in searchablePositions)
        {
            if (activeTraps.TryGetValue(pos, out var trap) && !trap.IsDiscovered)
            {
                trap.OnDiscovered(hero);
            }
        }
    }

    /// <summary>
    /// Vérifie et déclenche un piège si le héros marche sur la case position.
    /// </summary>
    public bool CheckAndTriggerStepOnTrap(Vector2 position, Character hero)
    {
        if (activeTraps.TryGetValue(position, out var trap) && !trap.IsSprung)
        {
            trap.OnSpring(hero);
            return true; // Un piège a été déclenché, le mouvement s'arrête
        }
        return false;
    }
}