using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Popup (a PREFAB) listing the other heroes so the player can choose who receives an item.
/// Spawned by EquipmentDetailPanel when "Give" is pressed.
/// Suggested layout: root = full-screen Image (dark, semi-transparent) with this script;
/// child "Box" with a title TMP_Text and a container (Vertical Layout Group) for the hero buttons.
/// Clicking anywhere that is not a hero button cancels.
/// </summary>
[RequireComponent(typeof(Image))]
public class GiveTargetPanel : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private TMP_Text titleText;               // optional
    [SerializeField] private string titleFormat = "Give {item} to...";
    [SerializeField] private Transform container;              // where the hero buttons go
    [SerializeField] private Button recipientButtonPrefab;     // a Button with a TMP_Text inside

    private static GiveTargetPanel current;

    public static GiveTargetPanel Open(GiveTargetPanel prefab, Transform parent,
                                       HeroCard giver, CardData card, Action onGiven)
    {
        if (prefab == null || giver == null || card == null) return null;

        CloseCurrent();
        current = Instantiate(prefab, parent);
        EquipmentDetailPanel.BringToFront(current.gameObject, 1100); // above the item popup
        current.Fill(giver, card, onGiven);
        return current;
    }

    public static void CloseCurrent()
    {
        if (current != null) Destroy(current.gameObject);
        current = null;
    }

    private void Fill(HeroCard giver, CardData card, Action onGiven)
    {
        if (titleText != null)
            titleText.text = titleFormat.Replace("{item}", EquipmentCardText.DisplayName(card));

        if (container == null || recipientButtonPrefab == null)
        {
            Debug.LogError("GiveTargetPanel: assign the Container and the Recipient Button Prefab.", this);
            return;
        }

        foreach (var hero in HeroParty.OthersThan(giver))
        {
            var recipient = hero; // copy for the click handler
            Button button = Instantiate(recipientButtonPrefab, container);

            bool allowed = giver.CanGive(card, recipient);
            Debug.Log($"GiveTargetPanel: button for {recipient.PlayerName}, allowed={allowed} " +
                      $"(giver owns card: {giver.Owns(card)}, recipient accepts: {recipient.CanAccept(card)})");
            button.interactable = allowed;

            string heroClassName = string.IsNullOrWhiteSpace(recipient.title) ? recipient.name : recipient.title;
            string text = $"{recipient.PlayerName} ({heroClassName})";
            if (!allowed) text += " - cannot use";

            var label = button.GetComponentInChildren<TMP_Text>();
            if (label != null) label.text = text;

            button.onClick.AddListener(() =>
            {
                Debug.Log($"GiveTargetPanel: clicked {recipient.PlayerName}");
                bool ok = giver.GiveTo(recipient, card);
                Debug.Log($"GiveTargetPanel: GiveTo returned {ok}");
                if (ok)
                {
                    onGiven?.Invoke();
                    CloseCurrent();
                }
            });
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log($"GiveTargetPanel root clicked. Object under pointer: {eventData.pointerCurrentRaycast.gameObject?.name}");
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        if (current == this) current = null;
    }
}
