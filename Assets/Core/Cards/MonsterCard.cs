using UnityEngine;

[CreateAssetMenu(menuName = "Cards/Monster")]
public class MonsterCard : CardData
{
    [Header("Caractéristiques")]
    public int moveSquares;   // Cases de déplacement
    public int attackDice;
    public int defenseDice;
    public int hp;            // Points de vie
    public int mind;          // Points d'esprit

    [Header("Identité")]
    public Sprite icon;       // la pastille ronde en bas à gauche de la carte
    public MonsterTag[] tags; // Orc, Undead... (vide = aucun)

    // Vrai si aucune étiquette n'est exigée, ou si le monstre la possède
    public bool Matches(MonsterTag required) =>
        required == null || System.Array.IndexOf(tags, required) >= 0;
}