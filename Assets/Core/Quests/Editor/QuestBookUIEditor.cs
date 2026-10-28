#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

// Boutons de test dans l'inspecteur de QuestBookUI (en Play mode)
[CustomEditor(typeof(QuestBookUI))]
public class QuestBookUIEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        var ui = (QuestBookUI)target;

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Test des modes (Play mode)", EditorStyles.boldLabel);

        if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox("Lance le Play mode pour utiliser ces boutons.", MessageType.Info);
            return;
        }

        if (GUILayout.Button("Selection (liste, tout visible)")) ui.Open(QuestBookMode.Selection);
        if (GUILayout.Button("In game — Heroes (parchemin seul)")) ui.Open(QuestBookMode.InGameHeroes);
        if (GUILayout.Button("In game — Zargon (tout)")) ui.Open(QuestBookMode.InGameZargon);
        if (GUILayout.Button("Hide")) ui.Hide();

        if (ui.mode != QuestBookMode.Selection && GameSession.CurrentQuest == null)
            EditorGUILayout.HelpBox("Aucune quête en cours : renseigne « Debug Quest » ci-dessus.", MessageType.Warning);
    }
}
#endif
