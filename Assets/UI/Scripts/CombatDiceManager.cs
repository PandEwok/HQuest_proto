using UnityEngine;
using System.Collections.Generic;

public class CombatDiceManager : MonoBehaviour
{
    // Define the possible faces of a HeroQuest combat die
    public enum DieFace
    {
        Skull,
        HeroShield,
        MonsterShield
    }

    // A single die has a 6-sided layout: 3 Skulls, 2 Hero Shields, 1 Monster Shield
    private readonly List<DieFace> standardDicePool = new List<DieFace>()
    {
        DieFace.Skull, DieFace.Skull, DieFace.Skull,
        DieFace.HeroShield, DieFace.HeroShield,
        DieFace.MonsterShield
    };

    /// <summary>
    /// Core modular combat function.
    /// </summary>
    /// <param name="isHeroAttacking">True if Hero attacks Monster, False if Monster attacks Hero.</param>
    /// <param name="attackDiceCount">Number of dice the attacker rolls.</param>
    /// <param name="defenseDiceCount">Number of dice the defender rolls.</param>
    /// <returns>The final amount of successful hits dealt after defense.</returns>
    public int ResolveCombat(bool isHeroAttacking, int attackDiceCount, int defenseDiceCount, out List<DieFace> attackRolls, out List<DieFace> defenseRolls)
    {
        attackRolls = RollDice(attackDiceCount);
        defenseRolls = RollDice(defenseDiceCount);

        int skullsRolled = 0;
        int matchingShieldsRolled = 0;

        // 1. Count Attacker's Skulls
        foreach (DieFace face in attackRolls)
        {
            if (face == DieFace.Skull)
            {
                skullsRolled++;
            }
        }

        // 2. Count Defender's Shields (Heroes look for HeroShield, Monsters look for MonsterShield)
        DieFace targetShield = isHeroAttacking ? DieFace.MonsterShield : DieFace.HeroShield;

        foreach (DieFace face in defenseRolls)
        {
            if (face == targetShield)
            {
                matchingShieldsRolled++;
            }
            // Note: If a Hero rolls a MonsterShield (or vice versa), it naturally counts as nothing.
        }

        // 3. Calculate final hits (Skulls minus Shields, minimum 0)
        int finalHits = Mathf.Max(0, skullsRolled - matchingShieldsRolled);

        Debug.Log($"Attacker Rolled Skulls: {skullsRolled} | Defender Rolled Shields: {matchingShieldsRolled} | Final Hits: {finalHits}");

        return finalHits;
    }

    // Helper method to simulate rolling X random dice
    private List<DieFace> RollDice(int count)
    {
        List<DieFace> results = new List<DieFace>();
        for (int i = 0; i < count; i++)
        {
            int randomIndex = Random.Range(0, standardDicePool.Count);
            results.Add(standardDicePool[randomIndex]);
        }
        return results;
    }
}