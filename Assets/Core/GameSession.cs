using System.Collections.Generic;
using UnityEngine;
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

    public static void LogSelection()
    {
        if (SelectedHeroes.Count == 0)
        {
            Debug.LogWarning("GameSession : aucun héros sélectionné.");
            return;
        }

        for (int i = 0; i < SelectedHeroes.Count; i++)
        {
            Debug.Log($"--- Joueur {i + 1} ---");
            SelectedHeroes[i].LogInfo();
        }
    }

}