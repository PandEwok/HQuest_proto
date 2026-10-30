using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

/// <summary>
/// Répartition des groupes de sorts (règles HeroQuest) :
///  - Wizard + Elf : le Wizard choisit un groupe, puis l'Elf en choisit un, puis le Wizard reçoit les deux restants.
///  - Wizard seul  : il reçoit les quatre groupes (pas d'Elf avec qui partager).
///  - Elf seul     : il choisit un groupe, les trois autres ne sont pas utilisés.
///  - Aucun        : personne ne lance de sorts, l'écran l'indique simplement.
/// </summary>
[RequireComponent(typeof(UIDocument))]
public class SpellSelectionScreen : MonoBehaviour
{
    [SerializeField] private StyleSheet styleSheet;   // SpellSelection.uss

    [Tooltip("Les 12 SpellCard. Ils sont regroupés automatiquement via SpellCard.group")]
    [SerializeField] private List<SpellCard> allSpells = new();

    [Header("Titres des HeroCard (comme dans GameManager)")]
    [SerializeField] private string wizardTitle = "Wizard";
    [SerializeField] private string elfTitle = "Elf";

    [Tooltip("Scène à charger après validation (laisser vide pour ne rien charger)")]
    [SerializeField] private string nextScene;

    public event Action Confirmed;

    private class GroupInfo
    {
        public SpellGroup group;
        public string displayName;
        public string element;
        public readonly List<SpellCard> spells = new();
        public VisualElement card;
        public Label badge;
    }

    private readonly List<GroupInfo> groups = new();
    private readonly List<string> pickers = new();        // ordre des choix : qui choisit à chaque étape
    private readonly List<GroupInfo> picked = new();      // groupes déjà choisis (même ordre que pickers)
    private GroupInfo pending;                            // groupe cliqué mais pas encore validé

    private bool hasWizard, hasElf;

    private ScrollView groupList;
    private Label titleLabel, banner, info;
    private VisualElement summary;
    private Button backButton, confirmButton;

    private bool IsRecap => picked.Count >= pickers.Count;
    private string CurrentPicker => IsRecap ? null : pickers[picked.Count];

    // ───────────────────────── Cycle de vie ─────────────────────────

    private void OnEnable()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;

        if (styleSheet != null && !root.styleSheets.Contains(styleSheet))
            root.styleSheets.Add(styleSheet);

        groupList = root.Q<ScrollView>("group-list");
        titleLabel = root.Q<Label>("title");
        banner = root.Q<Label>("banner");
        info = root.Q<Label>("info");
        summary = root.Q<VisualElement>("summary");
        backButton = root.Q<Button>("back-button");
        confirmButton = root.Q<Button>("confirm-button");

        backButton.clicked += OnBackClicked;
        confirmButton.clicked += OnConfirmClicked;

        hasWizard = GameSession.SelectedHeroes.Any(h => h != null && h.title == wizardTitle);
        hasElf = GameSession.SelectedHeroes.Any(h => h != null && h.title == elfTitle);

        pickers.Clear();
        picked.Clear();
        pending = null;

        if (hasWizard && hasElf) { pickers.Add(wizardTitle); pickers.Add(elfTitle); }
        else if (hasWizard) { for (int i = 0; i < 3; i++) pickers.Add(wizardTitle); }  // Wizard seul : 3 groupes max
        else if (hasElf) { pickers.Add(elfTitle); }
        // Personne : aucun choix à faire

        BuildGroups();
        BuildCards();
        Refresh();
    }

    private void OnDisable()
    {
        if (backButton != null) backButton.clicked -= OnBackClicked;
        if (confirmButton != null) confirmButton.clicked -= OnConfirmClicked;
    }

    // ───────────────────────── Construction ─────────────────────────

    private void BuildGroups()
    {
        groups.Clear();

        foreach (var spell in allSpells)
        {
            if (spell == null || spell.group == null) continue;

            var g = groups.Find(x => x.group == spell.group);
            if (g == null)
            {
                g = new GroupInfo
                {
                    group = spell.group,
                    displayName = GetDisplayName(spell.group),
                };
                g.element = GetElementKey(g.displayName);
                groups.Add(g);
            }
            g.spells.Add(spell);
        }
    }

    private void BuildCards()
    {
        groupList.Clear();

        foreach (var g in groups)
        {
            var card = new VisualElement();
            card.AddToClassList("group-card");
            card.AddToClassList("group-card--" + g.element);

            var bar = new VisualElement();
            bar.AddToClassList("group-card__bar");
            card.Add(bar);

            var name = new Label(g.displayName);
            name.AddToClassList("group-card__name");
            card.Add(name);

            var tagline = new Label(GetTagline(g.element));
            tagline.AddToClassList("group-card__tagline");
            card.Add(tagline);

            foreach (var spell in g.spells)
            {
                var row = new VisualElement();
                row.AddToClassList("spell");

                var sn = new Label(spell.title);
                sn.AddToClassList("spell__name");
                var sd = new Label(spell.description);
                sd.AddToClassList("spell__desc");

                row.Add(sn);
                row.Add(sd);
                card.Add(row);
            }

            g.badge = new Label();
            g.badge.AddToClassList("group-card__badge");
            card.Add(g.badge);

            var captured = g;
            card.RegisterCallback<ClickEvent>(_ => OnGroupClicked(captured));

            groupList.Add(card);
            g.card = card;
        }
    }

    // ───────────────────────── Logique de répartition ─────────────────────────

    /// <summary>Propriétaire d'un groupe d'après l'état actuel (null = personne / inutilisé).</summary>
    private string OwnerOf(GroupInfo g)
    {
        int i = picked.IndexOf(g);
        if (i >= 0) return pickers[i];
        if (IsRecap && hasWizard && hasElf) return wizardTitle;   // les groupes laissés par l'Elf vont au Wizard
        return null;
    }

    private bool IsAutomatic(GroupInfo g) => !picked.Contains(g) && OwnerOf(g) != null;

    private void OnGroupClicked(GroupInfo g)
    {
        if (IsRecap || OwnerOf(g) != null) return;   // rien à choisir ou déjà pris
        pending = g;
        Refresh();
    }

    private void OnBackClicked()
    {
        if (picked.Count == 0) return;
        picked.RemoveAt(picked.Count - 1);
        pending = null;
        Refresh();
    }

    private void OnConfirmClicked()
    {
        if (!IsRecap)
        {
            if (pending == null) return;
            picked.Add(pending);
            pending = null;
            Refresh();
            return;
        }

        // Validation finale : on enregistre les sorts de chaque lanceur
        SpellBook.Clear();
        if (hasWizard) SpellBook.Set(wizardTitle, SpellsOwnedBy(wizardTitle));
        if (hasElf) SpellBook.Set(elfTitle, SpellsOwnedBy(elfTitle));

        Confirmed?.Invoke();

        if (!string.IsNullOrEmpty(nextScene))
            SceneManager.LoadScene(nextScene);
    }

    private IEnumerable<SpellCard> SpellsOwnedBy(string heroTitle)
        => groups.Where(g => OwnerOf(g) == heroTitle).SelectMany(g => g.spells);

    // ───────────────────────── Affichage ─────────────────────────

    private void Refresh()
    {
        bool recap = IsRecap;

        foreach (var g in groups)
        {
            string owner = OwnerOf(g);
            bool pickable = !recap && owner == null;
            bool isPending = pending == g;
            bool unused = recap && owner == null;

            g.card.EnableInClassList("group-card--pickable", pickable);
            g.card.EnableInClassList("group-card--selected", isPending);
            g.card.EnableInClassList("group-card--dim", unused);
            g.card.EnableInClassList("group-card--owner-wizard", owner == wizardTitle);
            g.card.EnableInClassList("group-card--owner-elf", owner == elfTitle);

            // Pastille
            g.badge.RemoveFromClassList("badge--wizard");
            g.badge.RemoveFromClassList("badge--elf");
            g.badge.RemoveFromClassList("badge--unused");

            if (owner != null)
            {
                g.badge.text = owner.ToUpperInvariant() + (IsAutomatic(g) ? " (AUTOMATIC)" : "");
                g.badge.AddToClassList(OwnerClass(owner));
                g.badge.AddToClassList("badge--visible");
            }
            else if (unused)
            {
                g.badge.text = "UNUSED";
                g.badge.AddToClassList("badge--unused");
                g.badge.AddToClassList("badge--visible");
            }
            else if (isPending)
            {
                g.badge.text = "SELECTED";
                g.badge.AddToClassList("badge--visible");
            }
            else
            {
                g.badge.RemoveFromClassList("badge--visible");
            }
        }

        RefreshHeader();
        RefreshSummary();

        backButton.style.display = picked.Count > 0 ? DisplayStyle.Flex : DisplayStyle.None;

        if (recap)
        {
            confirmButton.text = "CONFIRM";
            confirmButton.SetEnabled(true);
        }
        else
        {
            confirmButton.text = "VALIDATE CHOICE";
            confirmButton.SetEnabled(pending != null);
        }
    }

    private void RefreshHeader()
    {
        banner.RemoveFromClassList("banner--wizard");
        banner.RemoveFromClassList("banner--elf");
        banner.RemoveFromClassList("banner--done");

        if (IsRecap)
        {
            titleLabel.text = "Spell distribution";
            banner.text = (hasWizard || hasElf) ? "ALL SPELLS ARE DISTRIBUTED" : "NO SPELLCASTER IN THE PARTY";
            banner.AddToClassList("banner--done");
        }
        else
        {
            string who = CurrentPicker;
            titleLabel.text = "Choose your spells";
            string counter = (hasWizard && !hasElf) ? $" ({picked.Count + 1}/{pickers.Count})" : "";
            banner.text = $"{who.ToUpperInvariant()}: CHOOSE A SPELL GROUP{counter}";
            banner.AddToClassList(OwnerBannerClass(who));
        }

        info.text = BuildInfoText();
    }

    private string BuildInfoText()
    {
        if (!hasWizard && !hasElf)
            return "There is no Wizard and no Elf in the party: nobody can cast spells, so no spell cards are given out.";

        if (hasWizard && !hasElf)
        {
            return IsRecap
                ? "The Wizard has chosen three spell groups. The remaining group is not used in this quest."
                : "The Wizard is the only spellcaster and chooses three of the four spell groups. The last group will not be used.";
        }

        if (!hasWizard && hasElf)
        {
            return IsRecap
                ? "The Elf has chosen a spell group. The three other groups are not used in this quest."
                : "The Elf is the only spellcaster and chooses one spell group. The three other groups will not be used.";
        }

        // Wizard + Elf
        if (picked.Count == 0)
            return "The Wizard chooses first. The Elf then chooses one of the three remaining groups, and the Wizard takes the last two.";
        if (!IsRecap)
            return "Now the Elf chooses one of the remaining groups. The Wizard will automatically receive the other two.";
        return "The Elf has made a choice. The Wizard automatically receives the two groups left over.";
    }

    private void RefreshSummary()
    {
        summary.Clear();
        if (hasWizard) AddSummaryRow(wizardTitle);
        if (hasElf) AddSummaryRow(elfTitle);
    }

    private void AddSummaryRow(string heroTitle)
    {
        var row = new VisualElement();
        row.AddToClassList("summary-row");

        var hero = new Label(heroTitle + ":");
        hero.AddToClassList("summary-hero");
        hero.AddToClassList(heroTitle == wizardTitle ? "summary-hero--wizard" : "summary-hero--elf");

        var owned = groups.Where(g => OwnerOf(g) == heroTitle).Select(g => g.displayName).ToList();
        string text;
        bool empty = false;

        if (pending != null && CurrentPicker == heroTitle)
        {
            owned.Add(pending.displayName + " (to validate)");
        }

        if (owned.Count > 0)
        {
            text = string.Join(", ", owned);
        }
        else
        {
            text = "—";
            empty = true;
        }

        var value = new Label(text);
        value.AddToClassList(empty ? "summary-empty" : "summary-value");

        row.Add(hero);
        row.Add(value);
        summary.Add(row);
    }

    // ───────────────────────── Utilitaires ─────────────────────────

    private string OwnerClass(string owner)
        => owner == wizardTitle ? "badge--wizard" : owner == elfTitle ? "badge--elf" : "badge--unused";

    private string OwnerBannerClass(string owner)
        => owner == wizardTitle ? "banner--wizard" : "banner--elf";

    // "base.group.fire" -> "Fire"
    private static string GetDisplayName(SpellGroup group)
    {
        string raw = !string.IsNullOrEmpty(group.id) ? group.id : group.name;
        int cut = raw.LastIndexOfAny(new[] { '.', '_' });
        if (cut >= 0 && cut < raw.Length - 1) raw = raw.Substring(cut + 1);
        raw = raw.Trim();
        if (raw.Length == 0) return group.name;
        return char.ToUpperInvariant(raw[0]) + raw.Substring(1).ToLowerInvariant();
    }

    private static string GetElementKey(string displayName)
    {
        string n = displayName.ToLowerInvariant();
        if (n.Contains("fire")) return "fire";
        if (n.Contains("water")) return "water";
        if (n.Contains("air")) return "air";
        if (n.Contains("earth") || n.Contains("stone")) return "earth";
        return "other";
    }

    private static string GetTagline(string element) => element switch
    {
        "fire" => "OFFENCE AND BRAVERY",
        "water" => "SLEEP, STEALTH AND HEALING",
        "air" => "SPEED AND CONTROL",
        "earth" => "DEFENCE, HEALING AND PASSAGE",
        _ => "SPELL GROUP"
    };
}