using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

/// <summary>
/// Boutons de test :
///  - "Simulate end of the quest" : popup de félicitations -> "Validate" -> termine la quête en cours (réussie)
///    et lance la suivante avec le même flux que le vrai jeu :
///    GameSession.EndQuest(true) -> GameSession.StartQuest(suivante) -> GameSession.ShowQuestIntro().
///  - "Simulate end of the game" : popup de félicitations -> bouton pour quitter le jeu
///    (fonctionne dans l'éditeur et en build, sans faire planter).
///
/// Mise en place (dans la scène de jeu) :
///  1. Créer un GameObject "SimulateQuestEnd" avec un UIDocument (Source Asset = SimulateQuest.uxml).
///  2. Ajouter ce script dessus.
///  3. Dans "Books", glisser le ou les QuestBook (livre de base + extensions) pour qu'il retrouve la quête suivante.
/// </summary>
[RequireComponent(typeof(UIDocument))]
public class SimulateQuestEnd : MonoBehaviour
{
    [Tooltip("Les livres de quêtes, pour trouver la quête suivante (ordre = QuestData.number).")]
    [SerializeField] QuestBook[] books;

    [Tooltip("Ordre d'affichage : doit être supérieur aux autres UIDocument pour que le bouton soit cliquable.")]
    [SerializeField] int sortingOrder = 90;

    [Header("Messages")]
    [SerializeField] string questCompletedMessage = "Congratulations, you completed the quest!";
    [SerializeField] string gameCompletedMessage = "Congratulations, you completed the game!\nThank you for playing.";
    [SerializeField] string webGlQuitMessage = "Thank you for playing!\nYou can now close this tab.";

    [Tooltip("Appelé quand la quête simulée était la dernière du livre (ex : afficher l'écran de fin).")]
    public UnityEvent onLastQuestCompleted;

    Button _questButton;
    Button _gameButton;
    VisualElement _modal;
    Label _modalMessage;
    Button _modalConfirm;
    Action _onConfirm;

    void OnEnable()
    {
        var doc = GetComponent<UIDocument>();
        doc.sortingOrder = sortingOrder;
        var root = doc.rootVisualElement;

        _questButton = root.Q<Button>("simulate-quest-button");
        _gameButton = root.Q<Button>("simulate-game-button");
        _modal = root.Q<VisualElement>("simulate-modal");
        _modalMessage = root.Q<Label>("simulate-modal-message");
        _modalConfirm = root.Q<Button>("simulate-modal-confirm");

        _questButton.clicked += Simulate;
        _gameButton.clicked += SimulateEndOfGame;
        _modalConfirm.clicked += OnModalConfirm;

        HideModal();
    }

    void OnDisable()
    {
        if (_questButton != null) _questButton.clicked -= Simulate;
        if (_gameButton != null) _gameButton.clicked -= SimulateEndOfGame;
        if (_modalConfirm != null) _modalConfirm.clicked -= OnModalConfirm;
    }

    // ---------------------------------------------------------------- Fin de quête

    /// <summary>Affiche la popup de félicitations ; la quête ne se termine qu'au clic sur "Validate".</summary>
    public void Simulate()
    {
        if (GameSession.CurrentQuest == null)
        {
            Debug.LogWarning("SimulateQuestEnd : aucune quête en cours dans GameSession.", this);
            return;
        }

        ShowModal(questCompletedMessage, "Validate", CompleteQuestAndContinue);
    }

    void CompleteQuestAndContinue()
    {
        QuestData current = GameSession.CurrentQuest;
        if (current == null)
        {
            Debug.LogWarning("SimulateQuestEnd : aucune quête en cours dans GameSession.", this);
            return;
        }

        // À faire AVANT EndQuest, qui remet CurrentQuest à null
        QuestData next = FindNextQuest(current);

        GameSession.EndQuest(true);   // marque la quête comme terminée (QuestProgress)

        if (next == null)
        {
            Debug.Log($"SimulateQuestEnd : '{current.title}' était la dernière quête.", this);
            onLastQuestCompleted?.Invoke();
            return;
        }

        Debug.Log($"SimulateQuestEnd : '{current.title}' terminée -> quête suivante : '{next.title}'.", this);
        GameSession.StartQuest(next);
        GameSession.ShowQuestIntro();
    }

    // ---------------------------------------------------------------- Fin du jeu

    public void SimulateEndOfGame()
    {
        ShowModal(gameCompletedMessage, "Quit game", QuitGame);
    }

    void QuitGame()
    {
#if UNITY_EDITOR
        // Dans l'éditeur, Application.Quit() ne fait rien : on arrête le mode Play.
        UnityEditor.EditorApplication.isPlaying = false;
#elif UNITY_WEBGL
        // Un navigateur interdit à une page de se fermer elle-même : Application.Quit() est
        // sans effet en WebGL. On affiche donc un message de remerciement à la place.
        _modalMessage.text = webGlQuitMessage;
        _modalConfirm.style.display = DisplayStyle.None;
        _modal.style.display = DisplayStyle.Flex;
#else
        Application.Quit();
#endif
    }

    // ---------------------------------------------------------------- Popup

    void ShowModal(string message, string buttonLabel, Action onConfirm)
    {
        _onConfirm = onConfirm;
        _modalMessage.text = message;
        _modalConfirm.text = buttonLabel;
        _modalConfirm.style.display = DisplayStyle.Flex;
        _modal.style.display = DisplayStyle.Flex;
    }

    void HideModal()
    {
        _modal.style.display = DisplayStyle.None;
    }

    void OnModalConfirm()
    {
        var action = _onConfirm;
        _onConfirm = null;

        // Pour la fin du jeu en WebGL, QuitGame() ré-affiche lui-même la popup avec un autre message.
        HideModal();
        action?.Invoke();
    }

    // ---------------------------------------------------------------- Utilitaires

    QuestData FindNextQuest(QuestData current)
    {
        if (books == null || books.Length == 0)
            Debug.LogWarning("SimulateQuestEnd : aucun QuestBook assigné, impossible de trouver la quête suivante.", this);

        foreach (var book in books.Where(b => b != null))
        {
            var ordered = book.Ordered().ToList();
            int i = ordered.IndexOf(current);
            if (i >= 0)
                return i + 1 < ordered.Count ? ordered[i + 1] : null;
        }
        return null;
    }
}
