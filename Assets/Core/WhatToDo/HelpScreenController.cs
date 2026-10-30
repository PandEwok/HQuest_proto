using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Écran d'aide bilingue (FR / EN), affiché dans la même scène que le jeu.
///
/// Mise en place (tout reste dans la scène de jeu, aucun changement de scène) :
///  1. Bouton "?" : ajouter un UIDocument avec HelpButton.uxml
///     (ou coller le Button "help-button" dans l'UXML de ta scène de jeu, avec la classe "help-button").
///  2. Écran d'aide : créer un GameObject "HelpScreen" avec un UIDocument (Source Asset = HelpScreen.uxml)
///     + ce script.
///  3. Dans l'inspecteur du script, glisser dans "Button Document" l'UIDocument qui contient le bouton "?".
///
/// Au clic sur "?", l'interface d'aide devient visible par-dessus le jeu ; elle se ferme avec X, FERMER
/// ou un clic sur le fond sombre.
/// </summary>
[RequireComponent(typeof(UIDocument))]
public class HelpScreenController : MonoBehaviour
{
    [Tooltip("UIDocument qui contient le bouton \"help-button\" (UI de la scène de jeu ou HelpButton.uxml).")]
    [SerializeField] UIDocument buttonDocument;

    [Tooltip("Ordre d'affichage de l'écran d'aide : doit être supérieur aux autres UIDocument.")]
    [SerializeField] int sortingOrder = 100;

    [Tooltip("Met le jeu en pause (Time.timeScale = 0) tant que l'aide est ouverte.")]
    [SerializeField] bool pauseWhileOpen = true;

    enum Lang { FR, EN }

    class Content
    {
        public string Title, Intro, ListTitle, Close, SwitchTo;
        public string[] Items;
    }

    static readonly Content French = new Content
    {
        Title = "Aide",
        Intro = "Bienvenue dans notre prototype de HeroQuest, un projet éprouvant dont le délai ne nous a pas permis d'implémenter l'intégralité des features. Nous espérons tout de même que vous passerez un bon moment.",
        ListTitle = "Voici ce que vous pouvez faire :",
        Items = new[]
        {
            "Vous déplacer sur la carte en jouant un héros",
            "Subir les effets de pièges (attention, seules certaines quêtes en ont, comme la quête 3)",
            "Cliquer sur le bouton « Simulate end of the quest » pour passer à la quête suivante",
            "Cliquer sur le bouton « Simulate end of the game » pour voir l'écran de fin",
            "Cliquer sur l'engrenage en haut à gauche de la scène de jeu pour quitter le jeu",
            "Sur l'écran titre, vous pouvez expérimenter l'éditeur qui a été utilisé pour faire les cartes de jeu. Attention cependant, ces cartes sont stockées localement et ne peuvent pas servir pour jouer."
        },
        Close = "FERMER",
        SwitchTo = "EN"
    };

    static readonly Content English = new Content
    {
        Title = "Help",
        Intro = "Welcome to our HeroQuest prototype, a grueling project whose deadline didn't allow us to implement every feature. We still hope you'll have a good time.",
        ListTitle = "Here's what you can do:",
        Items = new[]
        {
            "Move around the map by playing a hero",
            "Suffer the effects of traps (note that only some quests have them, such as quest 3)",
            "Click the “Simulate end of the quest” button to move on to the next quest",
            "Click the “Simulate end of the game” button to see the end screen",
            "Click the gear icon at the top left of the game scene to quit the game",
            "On the title screen, you can try out the editor that was used to make the game maps. Be careful though: these maps are stored locally and can't be used to play."
        },
        Close = "CLOSE",
        SwitchTo = "FR"
    };

    const string HiddenClass = "help-overlay--hidden";

    VisualElement _overlay, _list;
    Label _title, _intro, _listTitle;
    Button _helpButton, _langButton, _closeButton, _okButton;
    Lang _lang = Lang.FR;
    float _previousTimeScale = 1f;
    bool _isOpen;

    void OnEnable()
    {
        var doc = GetComponent<UIDocument>();
        doc.sortingOrder = sortingOrder;
        var root = doc.rootVisualElement;

        _overlay = root.Q("help-overlay");
        _list = root.Q("help-list");
        _title = root.Q<Label>("help-title");
        _intro = root.Q<Label>("help-intro");
        _listTitle = root.Q<Label>("help-list-title");
        _langButton = root.Q<Button>("help-lang-button");
        _closeButton = root.Q<Button>("help-close-button");
        _okButton = root.Q<Button>("help-ok-button");

        _langButton.clicked += ToggleLanguage;
        _closeButton.clicked += Hide;
        _okButton.clicked += Hide;
        _overlay.RegisterCallback<ClickEvent>(OnOverlayClick);

        Refresh();
    }

    // Start : les autres UIDocument ont tous fini leur OnEnable, le bouton "?" existe forcément.
    void Start()
    {
        if (buttonDocument == null)
        {
            Debug.LogWarning("HelpScreenController : aucun 'Button Document' assigné, le bouton \"?\" ne sera pas relié.", this);
            return;
        }

        _helpButton = buttonDocument.rootVisualElement.Q<Button>("help-button");
        if (_helpButton == null)
        {
            Debug.LogWarning("HelpScreenController : bouton \"help-button\" introuvable dans le Button Document.", this);
            return;
        }
        _helpButton.clicked += Show;
    }

    void OnDisable()
    {
        if (_isOpen) Hide();
        if (_langButton == null) return;
        _langButton.clicked -= ToggleLanguage;
        _closeButton.clicked -= Hide;
        _okButton.clicked -= Hide;
        _overlay.UnregisterCallback<ClickEvent>(OnOverlayClick);
    }

    void OnDestroy()
    {
        if (_helpButton != null) _helpButton.clicked -= Show;
    }

    public void Show()
    {
        if (_isOpen) return;
        _isOpen = true;
        _overlay.RemoveFromClassList(HiddenClass);

        if (pauseWhileOpen)
        {
            _previousTimeScale = Time.timeScale;
            Time.timeScale = 0f;
        }
    }

    public void Hide()
    {
        if (!_isOpen) return;
        _isOpen = false;
        _overlay.AddToClassList(HiddenClass);

        if (pauseWhileOpen) Time.timeScale = _previousTimeScale;
    }

    // Clic sur le fond sombre (hors du panneau) = fermer
    void OnOverlayClick(ClickEvent evt)
    {
        if (evt.target == _overlay) Hide();
    }

    void ToggleLanguage()
    {
        _lang = _lang == Lang.FR ? Lang.EN : Lang.FR;
        Refresh();
    }

    void Refresh()
    {
        Content c = _lang == Lang.FR ? French : English;

        _title.text = c.Title;
        _intro.text = c.Intro;
        _listTitle.text = c.ListTitle;
        _okButton.text = c.Close;
        _langButton.text = c.SwitchTo;

        _list.Clear();
        foreach (string item in c.Items)
        {
            var row = new VisualElement();
            row.AddToClassList("help-item");

            var bullet = new VisualElement();
            bullet.AddToClassList("help-item__bullet");

            var label = new Label(item);
            label.AddToClassList("help-item__text");

            row.Add(bullet);
            row.Add(label);
            _list.Add(row);
        }
    }
}
