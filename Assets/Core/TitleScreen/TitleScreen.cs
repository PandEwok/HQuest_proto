using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class TitleScreen : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private StyleSheet styleSheet;      // TitleScreen.uss
    [SerializeField] private Texture2D logo;             // logo HeroQuest (import en Texture 2D, alpha activee)

    [Header("Scenes")]
    [Tooltip("Scene chargee par le bouton Start (ex : HeroSelection)")]
    [SerializeField] private string startScene;
    [Tooltip("Scene chargee par le bouton Editor")]
    [SerializeField] private string editorScene;

    private Button startButton;
    private Button editorButton;
    private Button closeButton;

    private void OnEnable()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;

        if (styleSheet != null && !root.styleSheets.Contains(styleSheet))
            root.styleSheets.Add(styleSheet);

        if (logo != null)
            root.Q<VisualElement>("logo").style.backgroundImage = new StyleBackground(logo);

        startButton = root.Q<Button>("start-button");
        editorButton = root.Q<Button>("editor-button");
        closeButton = root.Q<Button>("close-button");

        startButton.clicked += OnStartClicked;
        editorButton.clicked += OnEditorClicked;
        closeButton.clicked += OnCloseClicked;

        // Desactive le bouton Editor si aucune scene n'est configuree
        editorButton.SetEnabled(!string.IsNullOrEmpty(editorScene));
    }

    private void OnDisable()
    {
        if (startButton != null) startButton.clicked -= OnStartClicked;
        if (editorButton != null) editorButton.clicked -= OnEditorClicked;
        if (closeButton != null) closeButton.clicked -= OnCloseClicked;
    }

    private void OnStartClicked()
    {
        if (!string.IsNullOrEmpty(startScene))
            SceneManager.LoadScene(startScene);
    }

    private void OnEditorClicked()
    {
        if (!string.IsNullOrEmpty(editorScene))
            SceneManager.LoadScene(editorScene);
    }

    private void OnCloseClicked()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
