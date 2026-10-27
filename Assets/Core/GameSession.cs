using System.Collections.Generic;

public static class GameSession
{
    public const int MaxHeroes = 4;

    // Dans l'ordre de sélection (le 1er cliqué = joueur 1, etc.)
    public static readonly List<HeroCard> SelectedHeroes = new();

    public static void SetHeroes(IEnumerable<HeroCard> heroes)
    {
        SelectedHeroes.Clear();
        SelectedHeroes.AddRange(heroes);
    }
}