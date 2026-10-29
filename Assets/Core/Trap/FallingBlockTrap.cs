using UnityEngine;
using System.Collections.Generic;

public class FallingBlockTrap : ITrap
{
    public string TrapName => "Falling-Block Trap";
    public bool IsDiscovered { get; set; } = false;
    public bool IsSprung { get; set; } = false;

    public bool CanBeDisarmed => !IsSprung;
    public bool CanBeJumped => !IsSprung;

    private CombatDiceManager diceManager;

    public FallingBlockTrap(CombatDiceManager diceManager)
    {
        this.diceManager = diceManager;
    }

    public void OnDiscovered(Character hero)
    {
        IsDiscovered = true;
        Debug.Log("Zargon: 'The ceiling looks dangerous.' (Pointe la case du piège)");
    }

    public void OnSpring(Character hero)
    {
        IsSprung = true;
        IsDiscovered = true;
        Debug.Log("Zargon: \"You've just sprung a falling-block trap! The ceiling is caving in! Look out!\"");

        int skullsRolled = 0;
        if (diceManager != null)
        {
            // Utilise la méthode privée de tirage ou l'équivalent via CombatDiceManager
            // On peut simuler 3 dés et compter les crânes
            var results = diceManager.ResolveCombat(true, 3, 0, out List<CombatDiceManager.DieFace> attackRolls, out _);
            // Alternative : compter directement les crânes dans attackRolls
            skullsRolled = 0;
            foreach (var face in attackRolls)
            {
                if (face == CombatDiceManager.DieFace.Skull) skullsRolled++;
            }
        }
        else
        {
            skullsRolled = Random.Range(0, 4); // Sécurité si le manager n'est pas assigné
        }

        if (skullsRolled > 0 && GameSession.ActiveHero != null)
        {
            GameSession.ActiveHero.ModifyBodyPoints(-skullsRolled);
            Debug.Log($"Le plafond s'écroule et inflige {skullsRolled} dégâts de Point de Corps.");
        }

        Debug.Log("La case est maintenant un bloc permanent. Le héros doit choisir d'avancer ou de reculer sur une case vide, ce qui termine son tour.");
    }

    public void Disarm(Character hero)
    {
        if (!CanBeDisarmed) return;
        Debug.Log($"{hero.name} désamorce le piège de bloc tombant.");
    }

    public bool Jump(Character hero)
    {
        if (!CanBeJumped)
        {
            Debug.Log("Impossible de sauter : le piège est déjà déclenché et bloque le passage.");
            return false;
        }
        Debug.Log($"{hero.name} tente de sauter par-dessus le piège à bloc.");
        return true;
    }
}