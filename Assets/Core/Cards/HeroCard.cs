using UnityEngine;

[CreateAssetMenu(menuName = "Cards/Hero")]
public class HeroCard : CardData
{
    public int attackDice, defenseDice, hp, mind;
    public int moveDice = 2;
    public EquipmentCard startWeapon, startArmor;
}