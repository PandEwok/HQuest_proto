using UnityEngine;
using System.Collections.Generic;

public class HeroQuestCombatTester : MonoBehaviour
{
    public enum Role { Hero, Enemy }

    [Header("Combat Roles")]
    [Tooltip("Who is attacking?")]
    [SerializeField] private Role attackerRole = Role.Hero;

    [Tooltip("Who is defending?")]
    [SerializeField] private Role defenderRole = Role.Enemy;

    [Header("Dice Face Ratios (Standard HQ Die: 3 Skulls, 2 Hero Shields, 1 Enemy Shield)")]
    [SerializeField] private int skullFaces = 3;
    [SerializeField] private int heroShieldFaces = 2;
    [SerializeField] private int enemyShieldFaces = 1;

    [Header("Dice Quantities")]
    [Tooltip("Number of dice rolled by the Attacker")]
    [SerializeField] private int attackDiceCount = 3;

    [Tooltip("Number of dice rolled by the Defender")]
    [SerializeField] private int defenseDiceCount = 2;

    public enum DiceFace { Skull, HeroShield, EnemyShield }

    [ContextMenu("Simulate Attack & Defense")]
    public void SimulateCombat()
    {
        // 1. Build the face pool for a standard HeroQuest combat die (6 faces total by default)
        List<DiceFace> dicePool = new List<DiceFace>();
        for (int i = 0; i < skullFaces; i++) dicePool.Add(DiceFace.Skull);
        for (int i = 0; i < heroShieldFaces; i++) dicePool.Add(DiceFace.HeroShield);
        for (int i = 0; i < enemyShieldFaces; i++) dicePool.Add(DiceFace.EnemyShield);

        if (dicePool.Count == 0)
        {
            Debug.LogWarning("Dice face pool is empty! Please configure faces in the Inspector.");
            return;
        }

        // 2. ROLL ATTACKER DICE
        int totalSkulls = 0;
        string attackRollsText = "[ ";
        for (int i = 0; i < attackDiceCount; i++)
        {
            DiceFace roll = dicePool[Random.Range(0, dicePool.Count)];
            attackRollsText += roll.ToString();
            if (i < attackDiceCount - 1) attackRollsText += ", ";

            // Attacker scores hits on Skulls
            if (roll == DiceFace.Skull)
            {
                totalSkulls++;
            }
        }
        attackRollsText += " ]";

        // 3. ROLL DEFENDER DICE
        int validBlocks = 0;
        string defenseRollsText = "[ ";
        for (int i = 0; i < defenseDiceCount; i++)
        {
            DiceFace roll = dicePool[Random.Range(0, dicePool.Count)];
            defenseRollsText += roll.ToString();
            if (i < defenseDiceCount - 1) defenseRollsText += ", ";

            // Hero defends ONLY with Hero Shields (White)
            if (defenderRole == Role.Hero && roll == DiceFace.HeroShield)
            {
                validBlocks++;
            }
            // Enemy defends ONLY with Enemy Shields (Black)
            else if (defenderRole == Role.Enemy && roll == DiceFace.EnemyShield)
            {
                validBlocks++;
            }
        }
        defenseRollsText += " ]";

        // 4. SUBTRACT DEFENDER SHIELDS FROM ATTACKER SKULLS
        int unblockedDamage = Mathf.Max(0, totalSkulls - validBlocks);

        // 5. PRINT DETAILED BREAKDOWN TO CONSOLE
        Debug.Log($"<color=cyan>[HeroQuest Combat Test]</color>\n" +
                  $"[ATTACK] {attackerRole} rolled {attackDiceCount} dice: {attackRollsText} -> {totalSkulls} Skulls\n" +
                  $"[DEFENSE] {defenderRole} rolled {defenseDiceCount} dice: {defenseRollsText} -> {validBlocks} Matching Shields\n" +
                  $"[CALCULATION] {totalSkulls} Skulls - {validBlocks} Blocks = {totalSkulls - validBlocks} (Clamped to min 0)\n" +
                  $"[RESULT] <color=red>{unblockedDamage} Unblocked Damage Dealt!</color>");
    }
}