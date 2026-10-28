using UnityEngine;

// EXEMPLE : comment la scène de jeu lit la quête en cours. À adapter / supprimer.
public class QuestExampleUsage : MonoBehaviour
{
    void Start()
    {
        QuestData quest = GameSession.CurrentQuest;
        QuestRuntime run = GameSession.Quest;
        if (quest == null) { Debug.LogWarning("Aucune quête en cours."); return; }

        // 1) Infos générales
        Debug.Log($"Quest {quest.number}: {quest.title}");
        Debug.Log($"Wandering monster: {quest.WanderingMonsterName()}");

        // 2) Règles spéciales
        if (quest.HasRule(QuestRuleType.StartWithoutEquipment)) { /* retirer l'équipement des héros */ }
        var altBlock = quest.GetRule(QuestRuleType.AltFallingBlock);
        if (altBlock != null) Debug.Log($"Falling block hurts on {altBlock.value}+, helmet: {altBlock.extraValues[0]}");

        // 3) Construire la carte
        foreach (var p in quest.placements)
        {
            if (p.kind == PlacementKind.Monster)
            {
                var entity = quest.FindEntity(p.entityId);   // null = monstre standard
                // instancier p.monster (ou entity.baseMonster) en p.cell, avec les règles de entity
            }
        }

        // 4) Réagir à une action d'un héros : fouille au trésor dans la pièce "room_3"
        foreach (var note in run.AvailableNotes(QuestTrigger.OnSearchTreasure, "room_3"))
        {
            foreach (var effect in note.effects)
            {
                if (effect.type == QuestEffectType.GiveGold) { /* ajouter effect.value d'or */ }
                if (effect.type == QuestEffectType.GiveCard) { /* donner effect.card */ }
            }
            run.Consume(note);
        }

        // 5) Fin de partie
        // GameSession.EndQuest(true); puis retour à la scène du Quest Book
    }
}
