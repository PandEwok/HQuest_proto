using UnityEngine;

public enum EquipmentType { Weapon, Armor, Tool }
public enum EquipmentTag { DiagonalAttack, Ranged, NoShield, NoWizard, Stackable }

[CreateAssetMenu(fileName = "Equipment", menuName = "Scriptable Objects/Cards/Equipment")]
public class EquipmentCard : CardData
{
    public EquipmentType type;
    public int attackDice, defenseDice, price;
    public EquipmentTag[] tags;
}

