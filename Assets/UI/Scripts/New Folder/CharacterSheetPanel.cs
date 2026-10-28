using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Fills the character sheet with the data of a runtime copy of a HeroCard.
/// It does NOT move or animate anything: opening and closing the sheet is handled by your own
/// animation. Every field below is optional, so you can build the sheet piece by piece.
/// </summary>
public class CharacterSheetPanel : MonoBehaviour
{
    [Header("Identity")]
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text classText;
    [SerializeField] private TMP_Text descriptionText;

    [Header("Dice")]
    [SerializeField] private TMP_Text attackDiceText;
    [SerializeField] private TMP_Text defendDiceText;

    [Header("Body / Mind")]
    [SerializeField] private TMP_Text startBodyText;      // fixed: HeroCard.startHP
    [SerializeField] private TMP_Text startMindText;      // fixed: HeroCard.startMind
    [SerializeField] private TMP_Text bodyPointsText;     // current, changes during play
    [SerializeField] private TMP_Text mindPointsText;     // current, changes during play

    [Header("Gold")]
    [SerializeField] private TMP_Text goldText;

    [Header("Equipment lists (containers with a Vertical Layout Group)")]
    [SerializeField] private Transform weaponsContainer;
    [SerializeField] private Transform armorContainer;
    [SerializeField] private Transform itemsContainer;
    [SerializeField] private EquipmentButton equipmentButtonPrefab; // it knows which detail popup to open

    [Header("Button text (placeholders: {name} {type} {attack} {defense} {price}; empty = just the name)")]
    [SerializeField, TextArea(1, 3)] private string weaponLabelFormat = "{name}  (ATK {attack})";
    [SerializeField, TextArea(1, 3)] private string armorLabelFormat = "{name}  (DEF +{defense})";
    [SerializeField, TextArea(1, 3)] private string itemLabelFormat = "{name}";

    [Header("Quests (one marker per quest, in order 1 to 14)")]
    [SerializeField] private Image[] questMarkers;
    [SerializeField] private TMP_Text questsSummaryText; // optional, e.g. "3 / 14"
    [SerializeField] private Color completedColor = new Color(0.85f, 0.7f, 0.2f);
    [SerializeField] private Color pendingColor = new Color(0.3f, 0.3f, 0.3f, 0.6f);

    private HeroCard hero;

    private void OnDestroy()
    {
        if (hero != null) hero.Changed -= Refresh;
    }

    // ---------------------------------------------------------------- binding

    /// <summary>Connects the sheet to a runtime copy of a HeroCard. It refreshes whenever the hero changes.</summary>
    public void Bind(HeroCard newHero)
    {
        if (hero != null) hero.Changed -= Refresh;
        hero = newHero;
        if (hero != null) hero.Changed += Refresh;
        Refresh();
    }

    /// <summary>Optional: call this (e.g. from your close animation or button) to close the item popup.</summary>
    public void HideDetails() => EquipmentDetailPanel.CloseCurrent();

    private void Refresh()
    {
        if (hero == null) return;

        SetText(nameText, hero.PlayerName);
        SetText(classText, string.IsNullOrWhiteSpace(hero.title) ? hero.name : hero.title);
        SetText(descriptionText, hero.PlayerDescription);

        SetText(attackDiceText, hero.CurrentAttackDice.ToString());
        SetText(defendDiceText, hero.CurrentDefenseDice.ToString());

        SetText(startBodyText, hero.startHP.ToString());
        SetText(startMindText, hero.startMind.ToString());
        SetText(bodyPointsText, hero.CurrentBody.ToString());
        SetText(mindPointsText, hero.CurrentMind.ToString());

        SetText(goldText, hero.Gold.ToString());

        if (questMarkers != null)
            for (int i = 0; i < questMarkers.Length; i++)
                if (questMarkers[i] != null)
                    questMarkers[i].color = hero.IsQuestCompleted(i + 1) ? completedColor : pendingColor;
        SetText(questsSummaryText, $"{hero.QuestsCompletedCount} / {HeroCard.TotalQuests}");

        Populate(weaponsContainer, hero.GetOwned(EquipmentSlot.Weapon), weaponLabelFormat);
        Populate(armorContainer, hero.GetOwned(EquipmentSlot.BodyArmor, EquipmentSlot.Helmet, EquipmentSlot.Shield), armorLabelFormat);
        Populate(itemsContainer, hero.GetOwned(EquipmentSlot.Tool, EquipmentSlot.Consumable), itemLabelFormat);
    }

    private static void SetText(TMP_Text field, string value)
    {
        if (field != null) field.text = value;
    }

    private void Populate(Transform container, List<EquipmentCard> cards, string labelFormat)
    {
        if (container == null || equipmentButtonPrefab == null) return;

        for (int i = container.childCount - 1; i >= 0; i--)
            Destroy(container.GetChild(i).gameObject);

        foreach (var card in cards)
        {
            var entry = Instantiate(equipmentButtonPrefab, container);
            entry.Setup(card, hero.IsEquipped(card), labelFormat);
        }
    }
}
