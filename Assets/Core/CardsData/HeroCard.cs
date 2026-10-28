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

    [ContextMenu("Log Info")]
    public void LogInfo()
    {
        Debug.Log(
            $"=== HeroCard : {title} ===\n" +
            $"ID : {id}\n" +
            $"Classe : {heroClass}\n" +
            $"Description : {description}\n" +
            $"Illustration : {(illustration != null ? illustration.name : "aucune")}\n" +
            $"Dés d'attaque : {attackDice}\n" +
            $"Dés de défense : {defenseDice}\n" +
            $"PV de départ : {startHP}\n" +
            $"Esprit de départ : {startMind}\n" +
            $"Dés de mouvement : {moveDice}\n" +
            $"Arme de départ : {(startWeapon != null ? startWeapon.title : "aucune")}\n" +
            $"Armure de départ : {(startArmor != null ? startArmor.title : "aucune")}",
            this); // le 2e argument permet de cliquer sur le log pour ping l'asset
    }
}