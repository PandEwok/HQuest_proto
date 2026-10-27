using UnityEngine;

[CreateAssetMenu(menuName = "Cards/Hero")]
public class HeroCard : CardData
{
    public HeroClass heroClass;

    [Header("Caractéristiques de départ")]
    public int attackDice;
    public int defenseDice;
    public int startHP;
    public int startMind;
    public int moveDice = 2;

    [Header("Équipement de départ")]
    public EquipmentCard startWeapon;
    public EquipmentCard startArmor;
}