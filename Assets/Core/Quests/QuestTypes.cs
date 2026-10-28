using System;
using UnityEngine;

// ───────── Déclencheurs : QUAND une note de quête s'active ─────────
public enum QuestTrigger
{
    OnQuestStart, OnSearchTreasure, OnSearchTraps, OnSearchSecretDoors,
    OnOpenDoor, OnEnterRoom, OnMoveOntoCell, OnNpcFound, OnLastHeroPassed
}

// ───────── Effets : CE QUI se passe ─────────
public enum QuestEffectType
{
    ReadText, NothingFound, GiveGold, GiveCard, RestoreHeroEquipment,
    DamageHero, StatueHero,
    ActivateEntity, SpawnAllRemainingMonsters, OpenAllDoors, RevealSecretDoor, BlockCell,
    SoundAlarm, SetFlag, EndQuest
}

[Serializable]
public class QuestEffect
{
    public QuestEffectType type;
    public int value;            // or, dégâts, nombre de tours...
    public CardData card;        // GiveCard : potion (TreasureCard), artefact, équipement...
    public string targetId;      // ActivateEntity : id d'une entité ; SetFlag : nom du drapeau
    public Vector2Int cell;      // RevealSecretDoor / BlockCell
    [TextArea] public string text; // ReadText

    public string Describe()
    {
        switch (type)
        {
            case QuestEffectType.ReadText: return $"Read aloud: \"{text}\"";
            case QuestEffectType.NothingFound: return "Nothing of interest (empty chest...)";
            case QuestEffectType.GiveGold: return $"Gives {value} gold coins";
            case QuestEffectType.GiveCard: return $"Gives card \"{(card != null ? card.title : "?")}\"";
            case QuestEffectType.RestoreHeroEquipment: return "Returns equipment and spells to the heroes";
            case QuestEffectType.DamageHero: return $"The hero loses {value} Body Points";
            case QuestEffectType.StatueHero: return $"The hero turns to stone for {value} turns (cannot move, cannot be harmed)";
            case QuestEffectType.ActivateEntity: return $"Activates / places entity \"{targetId}\"";
            case QuestEffectType.SpawnAllRemainingMonsters: return "Places all remaining monsters";
            case QuestEffectType.OpenAllDoors: return "Opens all doors";
            case QuestEffectType.RevealSecretDoor: return $"Reveals a secret door at {cell}";
            case QuestEffectType.BlockCell: return $"Permanently blocks square {cell}";
            case QuestEffectType.SoundAlarm: return "An alarm sounds";
            case QuestEffectType.SetFlag: return $"Sets flag \"{targetId}\"";
            case QuestEffectType.EndQuest: return "Ends the quest";
        }
        return type.ToString();
    }
}

// ───────── Objectifs ─────────
public enum ObjectiveType { KillEntity, KillAllMonsters, FindCard, RescueNpc, CarryToStairs, Escape, Custom }

// ───────── Récompenses ─────────
public enum RewardMode { None, GoldSplit, GoldPerHero, PerKillBounty }

// ───────── Règles spéciales de quête (remplacent les règles normales) ─────────
public enum QuestRuleType
{
    NoTrapsNorSecretDoors,   // Q1
    StartWithoutEquipment,   // Q6
    CarryObjectLimit,        // Q4  : value = objets max portés, extraValues[0] = dés de déplacement
    DoorTeleportation,       // Q10 : value = PV perdus si case occupée
    ExitOnDiceRoll,          // Q10 : extraValues = résultats (2, 12) permettant de sortir
    FoolsGold,               // Q10 : value = or "fou" (5000)
    AltFallingBlock,         // Q13 : value = jet minimum qui blesse (4), extraValues[0] = jet avec casque (6)
    FalseDoors,              // Q12
    TreasureKeptByFinder,    // Q3
    Custom
}

[Serializable]
public class QuestRule
{
    public QuestRuleType type;
    public int value;
    public int[] extraValues;
    [TextArea] public string description;

    public string Describe() => string.IsNullOrEmpty(description) ? type.ToString() : $"{type} — {description}";
}
