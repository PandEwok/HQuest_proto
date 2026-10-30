using UnityEngine;
using System.Collections.Generic;

public class SpearTrap : ITrap
{
    public string TrapName => "Spear Trap";
    public bool IsDiscovered { get; set; } = false;
    public bool IsSprung { get; set; } = false;

    public bool CanBeDisarmed => !IsSprung;
    public bool CanBeJumped => true;

    private CombatDiceManager diceManager;

    /// <summary>GameObject du piège, détruit une fois la lance déclenchée.</summary>
    public GameObject Owner { get; set; }

    // Le paramètre owner est optionnel : le code qui crée déjà le piège ne casse pas.
    // Si le piège est déjà créé, renseigne simplement trap.Owner = gameObject;
    public SpearTrap(CombatDiceManager diceManager, GameObject owner = null)
    {
        this.diceManager = diceManager;
        Owner = owner;
    }

    public void OnDiscovered(Character hero)
    {
        IsDiscovered = true;
        Debug.Log("Zargon: 'The square looks suspicious.' (Pointe la case du piège)");
    }

    public void OnSpring(Character hero)
    {
        if (IsSprung) return;   // ignore les contacts multiples (capsule + parent, etc.)
        IsSprung = true;

        Debug.Log("Zargon: 'Une lance jaillit du mur, du sol ou du plafond !' Jet de défense (1 dé de combat)...");

        var gameManager = UnityEngine.Object.FindFirstObjectByType<GameManager>();
        if (gameManager == null || hero == null)
        {
            Debug.LogWarning("SpearTrap: GameManager ou Character introuvable.");
            DestroyOwner();
            return;
        }

        bool dodged = false;
        if (diceManager != null)
        {
            diceManager.ResolveCombat(true, 1, 0, out List<CombatDiceManager.DieFace> rolls, out _);
            if (rolls.Count > 0)
            {
                var face = rolls[0];
                Debug.Log($"SpearTrap: résultat du dé = {face}");
                // Tout sauf un crâne protège le héros
                dodged = face != CombatDiceManager.DieFace.Skull;
            }
            else
            {
                Debug.LogWarning("SpearTrap: aucun résultat de dé, la lance touche par défaut.");
            }
        }
        else
        {
            Debug.LogWarning("SpearTrap: CombatDiceManager manquant, la lance touche par défaut.");
        }

        if (dodged)
        {
            Debug.Log($"{hero.name} esquive la lance (bouclier obtenu) : aucun dégât, le déplacement continue.");
            Debug.Log("Le piège à lance disparaît pour toujours.");
            DestroyOwner();
            return;
        }

        // La lance touche
        HeroCard card = gameManager.GetCardFromCharacter(hero);
        if (card != null)
        {
            card.ModifyBodyPoints(-1);
            Debug.Log($"La lance touche (crâne) ! {hero.name} perd 1 Point de Corps.");
        }
        else
        {
            Debug.LogWarning("SpearTrap: HeroCard introuvable, dégât non appliqué.");
        }

        Debug.Log("Le piège à lance disparaît pour toujours.");
        DestroyOwner();

        // Comme le pit trap : le héros s'arrête sur la case, puis le tour se termine (uniquement si touché).
        hero.InterruptMovement(() => gameManager.TriggerEndTurnFor(hero));
    }

    public void Disarm(Character hero)
    {
        if (!CanBeDisarmed) return;
        Debug.Log($"{hero.name} désamorce le piège à lance.");
    }

    public bool Jump(Character hero)
    {
        Debug.Log($"{hero.name} saute par-dessus le piège à lance.");
        return true;
    }

    private void DestroyOwner()
    {
        if (Owner != null) UnityEngine.Object.Destroy(Owner);
    }
}