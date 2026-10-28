using System.Collections.Generic;
using UnityEngine;

public static class GameSession
{
    public const int MaxHeroes = 4;

    // Dans l'ordre de sélection (le 1er cliqué = joueur 1, etc.)
    public static readonly List<HeroCard> SelectedHeroes = new();

    // ───── Quête en cours ─────
    public static QuestData CurrentQuest { get; private set; }   // les données (lecture seule)
    public static QuestRuntime Quest { get; private set; }       // l'état de la partie (drapeaux, notes consommées...)

    public static void SetHeroes(IEnumerable<HeroCard> heroes)
    {
        SelectedHeroes.Clear();
        SelectedHeroes.AddRange(heroes);
    }

    public static void StartQuest(QuestData quest)
    {
        CurrentQuest = quest;
        Quest = new QuestRuntime(quest);
    }

    // À appeler à la fin de la partie (success = quête réussie)
    public static void EndQuest(bool success)
    {
        if (success && CurrentQuest != null) QuestProgress.MarkCompleted(CurrentQuest);
        CurrentQuest = null;
        Quest = null;
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
