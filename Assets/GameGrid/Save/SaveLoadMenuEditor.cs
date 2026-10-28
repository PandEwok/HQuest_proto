#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

// Place this in an "Editor" folder (e.g. Assets/Scripts/Editor).
[CustomEditor(typeof(SaveLoadMenu))]
public class SaveLoadMenuEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        SerializedProperty extra = serializedObject.FindProperty("extraSlots");
        int extraCount = Mathf.Max(0, extra.intValue);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Slot management", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            $"{SaveLoadMenu.NativeSlotCount} native slots + {extraCount} extra = {SaveLoadMenu.NativeSlotCount + extraCount} slots.\n" +
            "Extra slots are shown in blue in the menu.", MessageType.None);

        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("+ Add a slot", GUILayout.Height(26)))
            {
                extra.intValue = extraCount + 1;
                serializedObject.ApplyModifiedProperties();
            }

            // A slot that contains a save cannot be removed
            int lastIndex = SaveLoadMenu.NativeSlotCount + extraCount - 1;
            bool canRemove = extraCount > 0 && !MapSaveSystem.Exists(lastIndex);
            using (new EditorGUI.DisabledScope(!canRemove))
            {
                if (GUILayout.Button("- Remove last slot", GUILayout.Height(26)))
                {
                    extra.intValue = extraCount - 1;
                    serializedObject.ApplyModifiedProperties();
                }
            }
        }

        EditorGUILayout.Space(4);
        if (GUILayout.Button("Open saves folder"))
        {
            System.IO.Directory.CreateDirectory(MapSaveSystem.FolderPath);
            EditorUtility.RevealInFinder(MapSaveSystem.FolderPath);
        }
    }
}
#endif