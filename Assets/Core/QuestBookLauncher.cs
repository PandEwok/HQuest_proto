using UnityEngine;
using UnityEngine.SceneManagement;

// À mettre dans la scène du Quest Book : lance la quête choisie.
public class QuestBookLauncher : MonoBehaviour
{
    public QuestBookUI questBook;
    public string gameSceneName = "Game";   // la scène doit être dans File > Build Settings

    void OnEnable() => questBook.QuestChosen += OnQuestChosen;
    void OnDisable() => questBook.QuestChosen -= OnQuestChosen;

    void OnQuestChosen(QuestData quest)
    {
        if (GameSession.SelectedHeroes.Count == 0)
            Debug.LogWarning("Aucun héros sélectionné : passe d'abord par le menu de sélection.");

        GameSession.StartQuest(quest);
        SceneManager.LoadScene(gameSceneName);
    }
}
