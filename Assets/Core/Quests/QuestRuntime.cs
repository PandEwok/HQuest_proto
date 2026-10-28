using System.Collections.Generic;
using System.Linq;

// État de la quête PENDANT la partie. Les QuestData (assets) ne sont jamais modifiés.
public class QuestRuntime
{
    public readonly QuestData data;

    readonly HashSet<string> flags = new HashSet<string>();
    readonly HashSet<string> deadGroups = new HashSet<string>();
    readonly HashSet<QuestNote> consumedNotes = new HashSet<QuestNote>();

    public QuestRuntime(QuestData data) { this.data = data; }

    // Drapeaux (effet SetFlag)
    public void SetFlag(string flag) => flags.Add(flag);
    public bool HasFlag(string flag) => flags.Contains(flag);

    // À appeler quand un monstre/entité/groupe est détruit (id d'entité ou groupId du placement)
    public void MarkDead(string idOrGroup) => deadGroups.Add(idOrGroup);
    public bool IsDead(string idOrGroup) => deadGroups.Contains(idOrGroup);

    // Notes qui peuvent s'activer maintenant (déclencheur + pièce, condition de mort remplie, pas déjà prises)
    public List<QuestNote> AvailableNotes(QuestTrigger trigger, string roomId = null) =>
        data.NotesFor(trigger, roomId)
            .Where(n => !consumedNotes.Contains(n))
            .Where(n => string.IsNullOrEmpty(n.requiresDeadGroup) || IsDead(n.requiresDeadGroup))
            .ToList();

    // Marque une note comme utilisée (si firstHeroOnly, elle ne sera plus proposée)
    public bool IsConsumed(QuestNote note) => consumedNotes.Contains(note);

    public void Consume(QuestNote note) { if (note.firstHeroOnly) consumedNotes.Add(note); }
}
