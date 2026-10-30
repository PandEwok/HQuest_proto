using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// "Start game" button -> creation form (name, hero, optional description) -> game starts.
/// The dropdown lists your HeroCard assets. On confirm, a runtime copy of the chosen
/// HeroCard is made for the player and handed to GameSession.
/// Put this script on an object that stays active (e.g. the Canvas), not on the form itself.
/// </summary>
public class CharacterCreationUI : MonoBehaviour
{
    [Header("Screens")]
    [SerializeField] private GameObject menuRoot;       // contains the "start game" button
    [SerializeField] private GameObject creationPanel;  // the form
    [SerializeField] private GameObject hudRoot;        // sheet + your open-sheet button, shown once playing

    [Header("Buttons")]
    [SerializeField] private Button newGameButton;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton;       // optional

    [Header("Form fields")]
    [SerializeField] private TMP_InputField nameInput;
    [SerializeField] private TMP_Dropdown classDropdown;
    [SerializeField] private TMP_InputField descriptionInput;
    [SerializeField] private TMP_Text classPreviewText; // shows the card description + starting points
    [SerializeField] private TMP_Text errorText;        // optional

    [Header("Data")]
    [SerializeField] private HeroCard[] heroCards;
    [SerializeField] private CharacterSheetPanel sheet;

    private void Awake()
    {
        if (heroCards == null || heroCards.Length == 0)
        {
            Debug.LogError("CharacterCreationUI: assign at least one HeroCard.", this);
            return;
        }

        var names = new List<string>();
        foreach (var card in heroCards) names.Add(CardName(card));
        classDropdown.ClearOptions();
        classDropdown.AddOptions(names);

        classDropdown.onValueChanged.AddListener(UpdatePreview);
        newGameButton.onClick.AddListener(OpenForm);
        confirmButton.onClick.AddListener(Confirm);
        if (cancelButton != null) cancelButton.onClick.AddListener(CloseForm);

        creationPanel.SetActive(false);
        hudRoot.SetActive(false);
        menuRoot.SetActive(true);
    }

    private void OpenForm()
    {
        nameInput.text = "";
        descriptionInput.text = "";
        classDropdown.value = 0;
        classDropdown.RefreshShownValue();
        UpdatePreview(0);
        SetError("");
        creationPanel.SetActive(true);
    }

    private void CloseForm() => creationPanel.SetActive(false);

    private void UpdatePreview(int index)
    {
        if (classPreviewText == null) return;

        var card = heroCards[index];
        classPreviewText.text =
            $"{card.description}\n\n" +
            $"Body points: {card.startHP}    Mind points: {card.startMind}\n" +
            $"Attack dice: {card.attackDice}    Defend dice: {card.defenseDice}";
    }

    private void Confirm()
    {
        string heroName = nameInput.text.Trim();
        if (heroName.Length == 0)
        {
            SetError("Please give your hero a name.");
            return;
        }

        var template = heroCards[classDropdown.value];
        var hero = HeroCard.CreateRuntimeCopy(template, heroName, descriptionInput.text);
        GameSession.StartWithCharacter(hero);

        creationPanel.SetActive(false);
        menuRoot.SetActive(false);
        hudRoot.SetActive(true);   // activate first so the sheet's Awake has run
        sheet.Bind(hero);
    }

    private static string CardName(HeroCard card) =>
        string.IsNullOrWhiteSpace(card.title) ? card.name : card.title;

    private void SetError(string message)
    {
        if (errorText != null) errorText.text = message;
    }
}
