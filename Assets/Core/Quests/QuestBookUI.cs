using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public enum QuestBookMode
{
    Selection,      // avant la quête : liste complète, tout est visible par défaut
    InGameHeroes,   // en jeu, vue héros : quête en cours uniquement, texte du parchemin seulement
    InGameZargon    // en jeu, vue Zargon : quête en cours uniquement, toutes les infos
}

[RequireComponent(typeof(UIDocument))]
public class QuestBookUI : MonoBehaviour
{
    public QuestLibrary library;                         // nécessaire en mode Selection
    public QuestBookMode mode = QuestBookMode.Selection; // mode au démarrage
    public bool unlockAllForTesting;

    [Tooltip("TEST UNIQUEMENT : quête utilisée en modes InGame si aucune quête n'a été lancée. Laisser vide dans le vrai jeu.")]
    public QuestData debugQuest;

    public event Action<QuestData> QuestChosen;   // Selection : le gameplay / le launcher s'abonne ici
    public event Action Closed;                   // en jeu : bouton CLOSE

    VisualElement root;
    ScrollView list, detail;
    Label numberLabel, titleLabel;
    Toggle zargonToggle;
    Button startButton, closeButton;
    QuestData current;
    readonly Dictionary<QuestData, Button> buttons = new Dictionary<QuestData, Button>();

    bool ready;

    void OnEnable() { TryInit(); }
    void Start() { TryInit(); }   // l'UIDocument n'est parfois pas encore prêt pendant OnEnable

    bool TryInit()
    {
        if (ready) return true;
        var doc = GetComponent<UIDocument>();
        root = doc != null ? doc.rootVisualElement : null;
        if (root == null) return false;

        list = root.Q<ScrollView>("quest-list");
        if (list == null) return false;   // arbre visuel pas encore construit

        QuestProgress.unlockAll = unlockAllForTesting;
        detail = root.Q<ScrollView>("detail-scroll");
        numberLabel = root.Q<Label>("quest-number");
        titleLabel = root.Q<Label>("quest-title");
        zargonToggle = root.Q<Toggle>("zargon-toggle");
        startButton = root.Q<Button>("start-button");
        closeButton = root.Q<Button>("close-button");

        zargonToggle.RegisterValueChangedCallback(_ => ShowDetail());
        startButton.clicked += OnStart;
        closeButton.clicked += OnClose;

        ready = true;
        SetMode(mode);
        return true;
    }

    void OnDisable()
    {
        if (ready)
        {
            startButton.clicked -= OnStart;
            closeButton.clicked -= OnClose;
        }
        ready = false;
    }

    // ───── API publique ─────
    public void SetMode(QuestBookMode newMode)
    {
        mode = newMode;
        if (!ready) return;   // sera appliqué par TryInit
        bool selection = mode == QuestBookMode.Selection;

        Show(list, selection);
        Show(startButton, selection);
        Show(zargonToggle, selection);
        Show(closeButton, !selection);

        if (selection)
        {
            zargonToggle.SetValueWithoutNotify(true);   // tout visible par défaut
            BuildList();
        }
        else
        {
            if (debugQuest != null && GameSession.CurrentQuest != debugQuest) GameSession.StartQuest(debugQuest); // test
            Select(GameSession.CurrentQuest);            // uniquement la quête en cours
        }
    }

    // En jeu : ouvre le livre dans le mode voulu
    public void Open(QuestBookMode newMode) { mode = newMode; if (!TryInit()) return; Show(root, true); SetMode(newMode); }
    public void Hide() { if (ready) Show(root, false); }

    // À rappeler après QuestProgress.MarkCompleted(...) en mode Selection
    public void Refresh() { if (!ready) return; if (mode == QuestBookMode.Selection) BuildList(); else ShowDetail(); }

    // ───── Liste (mode Selection) ─────
    void BuildList()
    {
        list.Clear();
        buttons.Clear();
        if (library == null) { Select(null); return; }
        QuestData toSelect = null;

        foreach (var book in library.Books())
        {
            list.Add(MakeLabel(book.title, "qb-book-header"));
            foreach (var q in book.Ordered())
            {
                var quest = q;
                var b = new Button(() => Select(quest)) { text = $"{quest.number}. {quest.title}" };
                b.AddToClassList("qb-quest-btn");
                if (QuestProgress.IsCompleted(quest)) b.AddToClassList("completed");
                if (!QuestProgress.IsUnlocked(quest)) b.AddToClassList("locked");
                list.Add(b);
                buttons[quest] = b;
                if (toSelect == null && QuestProgress.IsUnlocked(quest) && !QuestProgress.IsCompleted(quest)) toSelect = quest;
            }
        }
        Select(current != null && buttons.ContainsKey(current) ? current : toSelect ?? buttons.Keys.FirstOrDefault());
    }

    void Select(QuestData q)
    {
        current = q;
        foreach (var kv in buttons) kv.Value.EnableInClassList("selected", kv.Key == q);
        numberLabel.text = q != null ? $"QUEST {q.number}" : "";
        titleLabel.text = q != null ? q.title : "No quest in progress";
        ShowDetail();
    }

    // ───── Détail ─────
    void ShowDetail()
    {
        detail.Clear();
        if (current == null) { startButton.SetEnabled(false); return; }
        startButton.SetEnabled(QuestProgress.IsUnlocked(current));

        Section("Parchment text", current.parchmentText);   // visible par tout le monde

        bool secret = mode == QuestBookMode.InGameZargon
                   || (mode == QuestBookMode.Selection && zargonToggle.value);
        if (!secret) return;

        Section("Note for Zargon", current.zargonNote);
        Section("Objectives", Lines(current.objectives, o => o.Describe()));
        Section("Reward", current.reward != null ? current.reward.Describe() : "");
        Section("Wandering monster", current.WanderingMonsterName());
        Section("Starting point", string.IsNullOrEmpty(current.startMarker) ? "Stairway" : $"Area \"{current.startMarker}\"");
        Section("Items required at start", Lines(current.requiredItems, i => i.title));
        Section("Special rules", Lines(current.specialRules, r => r.Describe()));
        Section("Entities (named monsters, NPCs)", string.Join("\n\n", (current.entities ?? new QuestEntity[0]).Select(e => e.Describe())));

        if (current.notes != null && current.notes.Length > 0)
        {
            var run = mode == QuestBookMode.InGameZargon ? GameSession.Quest : null;
            detail.Add(MakeLabel("Quest notes", "qb-section-title"));
            foreach (var n in current.notes)
            {
                bool used = run != null && run.IsConsumed(n);
                var box = new VisualElement();
                box.AddToClassList("qb-note");
                if (used) box.AddToClassList("used");
                box.Add(MakeLabel($"{n.key} — {QuestNote.TriggerLabel(n.trigger)}{(used ? " (already used)" : "")}", "qb-note-title"));
                box.Add(MakeLabel(n.Describe(), "qb-section-body"));
                detail.Add(box);
            }
        }
        Section("Map contents", current.PlacementSummary());
    }

    void OnStart()
    {
        if (mode == QuestBookMode.Selection && current != null && QuestProgress.IsUnlocked(current))
            QuestChosen?.Invoke(current);
    }

    void OnClose() { Hide(); Closed?.Invoke(); }

    // ───── Outils ─────
    void Section(string title, string body)
    {
        if (string.IsNullOrWhiteSpace(body)) return;
        detail.Add(MakeLabel(title, "qb-section-title"));
        detail.Add(MakeLabel(body, "qb-section-body"));
    }

    static void Show(VisualElement e, bool visible) =>
        e.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;

    static string Lines<T>(T[] arr, Func<T, string> f) =>
        arr == null ? "" : string.Join("\n", arr.Where(x => x != null).Select(x => "• " + f(x)));

    static Label MakeLabel(string text, string cls)
    {
        var l = new Label(text);
        l.AddToClassList(cls);
        return l;
    }
}