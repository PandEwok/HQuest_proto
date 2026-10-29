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

    public SpearTrap(CombatDiceManager diceManager)
    {
        this.diceManager = diceManager;
    }

    public void OnDiscovered(Character hero)
    {
        IsDiscovered = true;
        Debug.Log("Zargon: 'The square looks suspicious.' (Pointe la case du piège)");
    }

    public void OnSpring(Character hero)
    {
        IsSprung = true;
        Debug.Log("Une lance jaillit du mur, du sol ou du plafond ! Jet de défense (1 dé de combat)...");

        bool dodged = false;
        if (diceManager != null)
        {
            // On lance 1 dé pour voir si le héros esquive
            var results = diceManager.ResolveCombat(true, 1, 0, out List<CombatDiceManager.DieFace> rolls, out _);
            if (rolls.Count > 0)
            {
                var face = rolls[0];
                // Si ce n'est pas un crâne (donc un bouclier de héros ou de monstre), le piège est esquivé
                if (face != CombatDiceManager.DieFace.Skull)
                {
                    dodged = true;
                }
            }
        }

        if (!dodged)
        {
            if (GameSession.ActiveHero != null)
            {
                GameSession.ActiveHero.ModifyBodyPoints(-1);
            }
            Debug.Log("La lance touche ! Le héros perd 1 Point de Corps.");
        }
        else
        {
            Debug.Log("Le héros a esquivé la lance (Bouclier obtenu) ! Il peut continuer son mouvement.");
        }

        Debug.Log("Le piège à lance disparaît pour toujours, la case peut maintenant être traversée en sécurité.");
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
}