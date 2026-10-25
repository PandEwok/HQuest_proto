using UnityEngine;

[CreateAssetMenu(menuName = "Cards/Treasure")]
public class TreasureCard : CardData
{
    public CardEffect[] effects;
    public bool returnToDeck;
    public bool singleUse;
}