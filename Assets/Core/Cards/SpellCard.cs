using System;
using UnityEngine;

public enum SpellEffectType
{
    Damage, Heal,
    BonusAttackDice, BonusDefenseDice,
    MoveDiceMultiplier, PassThroughWalls, PassThroughMonsters,
    Paralyze, SkipNextTurn,
    AttackWithDice, OpenDoor,
    SummonByTable, TeleportToSafeSpot
}

public enum SpellTarget { Self, AnyHero, AnyMonster, Door, AllHeroesInArea, AllHeroesAndMonstersInArea }
public enum SpellArea { None, Line, Room, RoomOrCorridor }
public enum ResistRule { None, ReduceDamagePerSuccess, NegateOnSuccess, BreakFreeOnSuccess }

[Flags]
public enum SpellEnd { None = 0, AfterNextAttack = 1, AfterNextMove = 2, WhenHeroLosesHP = 4, NoMonsterInSight = 8 }

[Serializable]
public class SpellResistance
{
    public ResistRule rule;
    public int dice;            // dés rouges lancés (0 = 1 dé par point d'esprit de la cible)
    public int successOn = 5;   // 5 = succès sur 5 ou 6 / 6 = succès sur 6 seulement
}

[Serializable]
public class SummonedMonster
{
    public MonsterCard monster;
    public int count;
}

[Serializable]
public class SummonGroup
{
    public int rollFrom;        // résultat du dé rouge, ex: 1
    public int rollTo;          // ex: 2
    public SummonedMonster[] monsters;
}

[Serializable]
public class SpellEffect
{
    public SpellEffectType type;
    public int value;
    public SpellTarget target;
    public SpellArea area;
    public SpellEnd endsWhen;
    public SpellResistance resistance;
    public MonsterTag[] immuneTags;       // monstres insensibles à l'effet
    public SummonGroup[] summonTable;     // seulement pour SummonByTable
}

[CreateAssetMenu(menuName = "Cards/Spell")]
public class SpellCard : CardData
{
    public SpellGroup group;              // vide = aucun groupe
    public bool playerChoosesOne;         // vrai = le joueur choisit UN seul effet de la liste (Djinn)
    public SpellEffect[] effects;
}