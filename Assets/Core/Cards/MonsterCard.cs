using UnityEngine;

[CreateAssetMenu(menuName = "Cards/Monster")]
public class MonsterCard : CardData
{
    public int move, attackDice, defenseDice, hp, mind;
    public Sprite factionIcon;
}
