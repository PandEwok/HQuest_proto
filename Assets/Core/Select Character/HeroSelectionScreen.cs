using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class HeroSelectionScreen : MonoBehaviour
{
    [SerializeField] private HeroRoster roster;
    [SerializeField] private VisualTreeAsset heroCardTemplate;
    [SerializeField] private int minHeroes = 1;
    [Tooltip("Scène à charger après validation (laisser vide pour ne rien charger)")]
    [SerializeField] private string nextScene;

    public event Action<IReadOnlyList<HeroCard>> Confirmed;

    private readonly List<HeroCard> selected = new();
    private readonly Dictionary<HeroCard, VisualElement> cards = new();

    private ScrollView heroList;
    private Label counter;
    private Button validateButton;

    [SerializeField] private StyleSheet styleSheet;   // à glisser dans l'inspecteur : HeroSelection.uss

    private void OnEnable()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;

        if (styleSheet != null && !root.styleSheets.Contains(styleSheet))
            root.styleSheets.Add(styleSheet);

        heroList = root.Q<ScrollView>("hero-list");
        counter = root.Q<Label>("counter");
        validateButton = root.Q<Button>("validate-button");

        validateButton.clicked += OnValidateClicked;

        selected.Clear();
        BuildCards();
        Refresh();
    }

    private void OnDisable()
    {
        if (validateButton != null) validateButton.clicked -= OnValidateClicked;
    }

    private void BuildCards()
    {
        heroList.Clear();
        cards.Clear();

        foreach (var hero in roster.heroes)
        {
            if (hero == null) continue;

            var card = heroCardTemplate.Instantiate().Q<VisualElement>("hero-card");

            card.Q<Label>("hero-name").text = hero.title;

            if (hero.illustration != null)
                card.Q<VisualElement>("hero-image").style.backgroundImage = new StyleBackground(hero.illustration);

            var stats = card.Q<VisualElement>("stats");
            AddStat(stats, "Attack", $"{hero.attackDice} dice");
            AddStat(stats, "Defense", $"{hero.defenseDice} dice");
            AddStat(stats, "Body Points", hero.startHP.ToString());
            AddStat(stats, "Mind Points", hero.startMind.ToString());
            AddStat(stats, "Movement", $"{hero.moveDice} dice");
            AddStat(stats, "Weapon", hero.startWeapon != null ? hero.startWeapon.title : "—");
            AddStat(stats, "Armor", hero.startArmor != null ? hero.startArmor.title : "—");

            var captured = hero;
            card.RegisterCallback<ClickEvent>(_ => Toggle(captured));

            heroList.Add(card);
            cards[hero] = card;
        }
    }

    private static void AddStat(VisualElement parent, string label, string value)
    {
        var row = new VisualElement();
        row.AddToClassList("stat-row");

        var l = new Label(label);
        l.AddToClassList("stat-label");
        var v = new Label(value);
        v.AddToClassList("stat-value");

        row.Add(l);
        row.Add(v);
        parent.Add(row);
    }

    private void Toggle(HeroCard hero)
    {
        if (selected.Remove(hero))
        {
            // désélectionné
        }
        else if (selected.Count < GameSession.MaxHeroes)
        {
            selected.Add(hero);
        }
        else
        {
            // Groupe complet : on fait clignoter le compteur
            counter.AddToClassList("counter--warning");
            counter.schedule.Execute(() => counter.RemoveFromClassList("counter--warning")).StartingIn(400);
            return;
        }

        Refresh();
    }

    private void Refresh()
    {
        bool full = selected.Count >= GameSession.MaxHeroes;

        foreach (var pair in cards)
        {
            bool isSelected = selected.Contains(pair.Key);
            pair.Value.EnableInClassList("hero-card--selected", isSelected);
            pair.Value.EnableInClassList("hero-card--locked", full && !isSelected);
        }

        counter.text = $"Selected: {selected.Count} / {GameSession.MaxHeroes}";
        counter.EnableInClassList("counter--full", full);
        validateButton.SetEnabled(selected.Count >= minHeroes);
    }

    private void OnValidateClicked()
    {
        GameSession.SetHeroes(selected);
        Confirmed?.Invoke(selected);

        if (!string.IsNullOrEmpty(nextScene))
            SceneManager.LoadScene(nextScene);
    }
}