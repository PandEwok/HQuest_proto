using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

/// <summary>
/// Écran où Zargon lit le parchemin de la quête choisie.
/// Il lit tout dans GameSession.CurrentQuest : on peut donc y arriver depuis n'importe où
/// via GameSession.ShowQuestIntro().
/// Tout l'écran (numéro, titre, parchemin, bouton) grossit à l'arrivée et rétrécit à la sortie.
/// </summary>
[RequireComponent(typeof(UIDocument))]
public class QuestIntroScreen : MonoBehaviour
{
    [SerializeField] private StyleSheet styleSheet;   // QuestIntro.uss
    [SerializeField] private string gameScene = "GameScene";

    [Header("Animation")]
    [SerializeField] private float enterDuration = 0.45f;
    [SerializeField] private float exitDuration = 0.2f;

    [Tooltip("TEST UNIQUEMENT : quête utilisée si aucune quête n'a été lancée. Laisser vide dans le vrai jeu.")]
    [SerializeField] private QuestData debugQuest;

    public event Action Confirmed;

    private enum Phase { Entering, Idle, Leaving }

    private Button confirmButton;
    private VisualElement content;
    private Phase phase;
    private float t;

    private void OnEnable()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;

        if (styleSheet != null && !root.styleSheets.Contains(styleSheet))
            root.styleSheets.Add(styleSheet);

        var numberLabel = root.Q<Label>("quest-number");
        var titleLabel = root.Q<Label>("quest-title");
        var textLabel = root.Q<Label>("parchment-text");
        confirmButton = root.Q<Button>("confirm-button");
        content = root.Q<VisualElement>("content");

        if (debugQuest != null && GameSession.CurrentQuest == null)
            GameSession.StartQuest(debugQuest);

        QuestData quest = GameSession.CurrentQuest;

        if (quest != null)
        {
            numberLabel.text = $"QUEST {quest.number}";
            titleLabel.text = quest.title;
            textLabel.text = Quote(quest.parchmentText);
            confirmButton.SetEnabled(true);
        }
        else
        {
            numberLabel.text = "";
            titleLabel.text = "No quest selected";
            textLabel.text = "Zargon has nothing to tell you: no quest has been chosen.";
            confirmButton.SetEnabled(false);
        }

        confirmButton.clicked += OnConfirmClicked;

        // Départ de l'animation d'arrivée : tout est réduit et invisible dès la première image
        phase = Phase.Entering;
        t = 0f;
        Apply(0.05f, 0f);
    }

    private void OnDisable()
    {
        if (confirmButton != null) confirmButton.clicked -= OnConfirmClicked;
    }

    private void Update()
    {
        if (content == null || phase == Phase.Idle) return;

        // On limite le dt pour qu'un gros hitch de chargement ne "mange" l'animation d'arrivée
        float dt = Mathf.Min(Time.unscaledDeltaTime, 0.05f);

        if (phase == Phase.Entering)
        {
            t += dt / Mathf.Max(0.01f, enterDuration);
            float k = Mathf.Clamp01(t);
            Apply(Mathf.LerpUnclamped(0.05f, 1f, EaseOutBack(k)), k);

            if (k >= 1f)
            {
                Apply(1f, 1f);
                phase = Phase.Idle;
            }
        }
        else // Leaving
        {
            t += dt / Mathf.Max(0.01f, exitDuration);
            float k = Mathf.Clamp01(t);
            Apply(1f - k * k, 1f - k);   // ease-in : accélère vers la fin

            if (k >= 1f)
            {
                phase = Phase.Idle;
                LoadGameScene();
            }
        }
    }

    private void Apply(float scale, float opacity)
    {
        content.style.scale = new StyleScale(new Scale(new Vector3(scale, scale, 1f)));
        content.style.opacity = Mathf.Clamp01(opacity);
    }

    private static float EaseOutBack(float k)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1f;
        float x = k - 1f;
        return 1f + c3 * x * x * x + c1 * x * x;
    }

    private void OnConfirmClicked()
    {
        if (phase == Phase.Leaving) return;   // évite le double clic
        confirmButton.SetEnabled(false);

        Confirmed?.Invoke();

        phase = Phase.Leaving;
        t = 0f;
    }

    private void LoadGameScene()
    {
        if (!string.IsNullOrEmpty(gameScene))
            SceneManager.LoadScene(gameScene);
        else
            Debug.LogWarning("QuestIntroScreen : nom de la scène de jeu non renseigné !");
    }

    // Entoure le texte de guillemets, sauf s'il en a déjà
    private static string Quote(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return "";
        text = text.Trim();
        char first = text[0];
        if (first == '"' || first == '“' || first == '«') return text;
        return "“" + text + "”";
    }
}