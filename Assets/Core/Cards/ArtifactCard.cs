using System;
using UnityEngine;

[Flags]

public enum ArtifactSlot { Weapon, Armor, Ring, Accessory, Consumable }
public enum UsageLimit { Unlimited, OncePerQuest, SingleUse }

public enum AbilityType
{
    ResurrectHero, TeleportToStart, CastSameSpellTwice, CastDifferentSpellsPerTurn,
    RerollAttackDie, IgnoreTargetDefense, ExtraAttack, ImprisonMonster, BonusAttackDice
}

[Serializable]
public class ArtifactAbility
{
    public AbilityType type;
    public int value;
    public UsageLimit usage;
    public MonsterTag requiredTargetTag;   // vide = toujours actif
}

[CreateAssetMenu(menuName = "Cards/Artifact")]
public class ArtifactCard : CardData
{
    public ArtifactSlot slot;

    [Header("Dés accordés")]
    public int attackDice;
    public int defenseDice;
    public bool diagonalAttack;

    [Header("Bonus passifs")]
    public int bonusHP;
    public int bonusMind;

    [Header("Restrictions de classe")]
    public HeroClass[] onlyFor;
    public HeroClass[] forbiddenFor;

    [Header("Pouvoirs")]
    public ArtifactAbility[] abilities;

    public bool CanBeUsedBy(HeroClass hero) =>
     (onlyFor.Length == 0 || System.Array.IndexOf(onlyFor, hero) >= 0)
     && System.Array.IndexOf(forbiddenFor, hero) < 0;
}