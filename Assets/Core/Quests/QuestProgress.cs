using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// Progression minimale (quêtes terminées), sauvegardée dans PlayerPrefs.
public static class QuestProgress
{
    const string Key = "questbook.completed";
    [Serializable] class Save { public List<string> ids = new List<string>(); }

    static HashSet<string> completed;
    public static bool unlockAll;   // pour tester

    static void Load()
    {
        if (completed != null) return;
        completed = new HashSet<string>();
        var json = PlayerPrefs.GetString(Key, "");
        if (!string.IsNullOrEmpty(json))
            foreach (var id in JsonUtility.FromJson<Save>(json).ids) completed.Add(id);
    }

    static void Persist()
    {
        PlayerPrefs.SetString(Key, JsonUtility.ToJson(new Save { ids = completed.ToList() }));
        PlayerPrefs.Save();
    }

    public static bool IsCompleted(QuestData q) { Load(); return completed.Contains(q.id); }
    public static bool IsUnlocked(QuestData q) => unlockAll || q.previousQuest == null || IsCompleted(q.previousQuest);
    public static void MarkCompleted(QuestData q) { Load(); if (completed.Add(q.id)) Persist(); }
    public static void ResetAll() { Load(); completed.Clear(); Persist(); }
}
