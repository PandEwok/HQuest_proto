using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Popup showing the details of one card (equipment, artifact or potion).
/// This is a PREFAB: it is spawned by EquipmentButton when an item is clicked,
/// and destroyed when the player clicks on the panel itself (anywhere except the Give button).
/// Only one popup exists at a time.
/// The root object needs an Image (background) so it can receive clicks - it is added automatically.
/// Optional: a "Give" Button + the Give Picker prefab let the hero hand the item to another hero.
/// </summary>
[RequireComponent(typeof(Image))]
public class EquipmentDetailPanel : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private TMP_Text titleText;         // name
    [SerializeField] private TMP_Text bodyText;          // stats (EquipmentCardText.Describe)
    [SerializeField] private TMP_Text descriptionText;   // CardData.description (optional)
    [SerializeField] private Image illustrationImage;    // CardData.illustration (optional)

    [Header("Give (optional)")]
    [SerializeField] private Button giveButton;              // hidden when nobody can receive the item
    [SerializeField] private GiveTargetPanel givePickerPrefab;

    private static EquipmentDetailPanel current;

    private CardData card;
    private HeroCard owner;
    private Transform spawnParent;

    /// <summary>Spawns the popup under 'parent' (closing any popup already open) and fills it.</summary>
    /// <param name="owner">The hero who holds the card. Needed for the Give button.</param>
    public static EquipmentDetailPanel Open(EquipmentDetailPanel prefab, Transform parent, CardData card, HeroCard owner = null)
    {
        if (prefab == null || card == null) return null;

        CloseCurrent();
        current = Instantiate(prefab, parent);
        BringToFront(current.gameObject, 1000);
        current.Fill(card, owner, parent);
        return current;
    }

    /// <summary>Closes the popup (and the "give to" list) if open. Safe to call at any time.</summary>
    public static void CloseCurrent()
    {
        GiveTargetPanel.CloseCurrent();
        if (current != null) Destroy(current.gameObject);
        current = null;
    }

    /// <summary>Makes a popup visible and clickable above everything else on screen.</summary>
    public static void BringToFront(GameObject popup, int sortingOrder)
    {
        var rect = (RectTransform)popup.transform;
        rect.localScale = Vector3.one;
        var pos = rect.anchoredPosition3D;
        rect.anchoredPosition3D = new Vector3(pos.x, pos.y, 0f);
        rect.SetAsLastSibling();

        // Own sorting layer: guarantees it is drawn above the sheet even if the sheet has its own Canvas.
        var canvas = popup.GetComponent<Canvas>();
        if (canvas == null) canvas = popup.AddComponent<Canvas>();
        canvas.overrideSorting = true;
        canvas.sortingOrder = sortingOrder;
        if (popup.GetComponent<GraphicRaycaster>() == null) popup.AddComponent<GraphicRaycaster>();

        popup.SetActive(true);
    }

    private void Fill(CardData newCard, HeroCard newOwner, Transform parent)
    {
        card = newCard;
        owner = newOwner;
        spawnParent = parent;

        if (titleText != null) titleText.text = EquipmentCardText.DisplayName(card);
        if (bodyText != null) bodyText.text = EquipmentCardText.Describe(card);
        if (descriptionText != null) descriptionText.text = card.description;
        if (illustrationImage != null)
        {
            illustrationImage.sprite = card.illustration;
            illustrationImage.enabled = card.illustration != null;
        }

        if (giveButton != null)
        {
            if (givePickerPrefab == null)
                Debug.LogWarning("EquipmentDetailPanel: assign the Give Picker Prefab to enable the Give button.", this);

            bool canGive = owner != null && givePickerPrefab != null && owner.Owns(card)
                           && HeroParty.OthersThan(owner).Count > 0;

            giveButton.gameObject.SetActive(canGive);
            giveButton.onClick.RemoveAllListeners();
            if (canGive) giveButton.onClick.AddListener(OnGiveClicked);
        }
    }

    private void OnGiveClicked()
    {
        // When the gift is made, both popups close.
        GiveTargetPanel.Open(givePickerPrefab, spawnParent, owner, card, CloseCurrent);
    }

    // A click anywhere on the panel (or on its child texts/images) closes it.
    // The Give button handles its own click, so it does not close the panel.
    public void OnPointerClick(PointerEventData eventData) => Destroy(gameObject);

    private void OnDestroy()
    {
        if (current == this) current = null;
    }
}
