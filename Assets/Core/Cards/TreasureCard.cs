using System;
using UnityEngine;

public enum TreasureEffectType
{
    GainGold, Damage, EndTurn, SpawnWanderingMonster,
    HealByDiceRoll, NextAttackBonusDice, NextDefenseBonusDice, NextAttackExtraAttacks
}

public enum TreasureTiming { WhenDrawn, KeptByHero }

[Serializable]
public class TreasureEffect
{
    public TreasureEffectType type;
    public int value;
}

[CreateAssetMenu(menuName = "Cards/Treasure")]
public class TreasureCard : CardData
{
    public TreasureTiming timing;   // WhenDrawn = effet immédiat / KeptByHero = le héros la garde et l'utilise quand il veut
    public bool returnToDeck;       // vrai = remise SOUS la pile après le tirage
    public bool singleUse;          // vrai = défaussée après utilisation (potions)
    public TreasureEffect[] effects;
}