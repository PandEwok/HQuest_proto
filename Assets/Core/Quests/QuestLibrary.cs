using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// Point d'entrée unique : le livre de base (14 quêtes obligatoires) + les extensions.
[CreateAssetMenu(menuName = "Quests/Library")]
public class QuestLibrary : ScriptableObject
{
    public QuestBook coreBook;
    public QuestBook[] extensions;

    public IEnumerable<QuestBook> Books()
    {
        if (coreBook != null) yield return coreBook;
        if (extensions != null) foreach (var b in extensions) if (b != null) yield return b;
    }

    public IEnumerable<QuestData> AllQuests() => Books().SelectMany(b => b.Ordered());

    public QuestData FindQuest(string questId) => AllQuests().FirstOrDefault(q => q.id == questId);

    void OnValidate()
    {
        if (coreBook == null) { Debug.LogWarning("QuestLibrary : le livre de base est manquant.", this); return; }
        int n = coreBook.Ordered().Count();
        if (n != 14) Debug.LogWarning($"QuestLibrary : le livre de base doit contenir 14 quêtes (actuellement {n}).", this);

        var dup = AllQuests().GroupBy(q => q.id).Where(g => g.Count() > 1).Select(g => g.Key);
        foreach (var d in dup) Debug.LogWarning($"QuestLibrary : id de quête en double « {d} ».", this);
    }
}
