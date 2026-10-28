using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// Un livre = un lot de quêtes (le livre de base, puis chaque extension).
[CreateAssetMenu(menuName = "Quests/Book")]
public class QuestBook : ScriptableObject
{
    public string id;                      // ex: "base.book.core"
    public string title;
    [TextArea(3, 10)] public string introduction;   // texte de Mentor (début)
    [TextArea(3, 10)] public string conclusion;     // texte de Mentor (fin)
    public QuestData[] quests;

    public IEnumerable<QuestData> Ordered() =>
        (quests ?? new QuestData[0]).Where(q => q != null).OrderBy(q => q.number);
}
