using System;
using UnityEngine;

public enum EquipmentSlot { Weapon, BodyArmor, Helmet, Shield, Tool, Consumable }

public enum EquipmentAbilityType
{
    RangedAttack, ThrowableWeapon, DestroyMonster, MoveDiceMultiplier, DisarmTrap
}

[Serializable]
public class EquipmentAbility
{
    public EquipmentAbilityType type;
    public int value;
    public MonsterTag requiredTargetTag;   // vide = aucune condition
}

[CreateAssetMenu(menuName = "Cards/Equipment")]
public class EquipmentCard : CardData
{
    public EquipmentSlot slot;
    public int price;                      // pièces d'or

    [Header("Dés")]
    public int attackDice;
    public int defenseDice;
    public bool diagonalAttack;

    [Header("Restrictions")]
    public bool blocksShield;              // "on ne peut pas utiliser de bouclier avec cette arme"
    public int maxMoveDice;                // 0 = pas de limite
    public HeroClass[] onlyFor;            // vide = tout le monde
    public HeroClass[] forbiddenFor;       // vide = personne

    [Header("Pouvoirs")]
    public EquipmentAbility[] abilities;

    public bool CanBeUsedBy(HeroClass hero) =>
        (onlyFor.Length == 0 || Array.IndexOf(onlyFor, hero) >= 0)
        && Array.IndexOf(forbiddenFor, hero) < 0;
}