using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Popup showing the details of one EquipmentCard.
/// This is a PREFAB: it is spawned by EquipmentButton when an item is clicked,
/// and destroyed when the player clicks anywhere on the panel itself.
/// Only one popup exists at a time.
/// The root object needs an Image (background) so it can receive clicks - it is added automatically.
/// Do NOT put a Button inside this prefab, or it would swallow the click.
/// </summary>
[RequireComponent(typeof(Image))]
public class EquipmentDetailPanel : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private TMP_Text titleText;         // name
    [SerializeField] private TMP_Text bodyText;          // stats (EquipmentCardText.Describe)
    [SerializeField] private TMP_Text descriptionText;   // CardData.description (optional)
    [SerializeField] private Image illustrationImage;    // CardData.illustration (optional)

    private static EquipmentDetailPanel current;

    /// <summary>Spawns the popup under 'parent' (closing any popup already open) and fills it.</summary>
    public static EquipmentDetailPanel Open(EquipmentDetailPanel prefab, Transform parent, EquipmentCard card)
    {
        if (prefab == null || card == null) return null;

        CloseCurrent();
        current = Instantiate(prefab, parent);
        current.PrepareOnTop();
        current.Fill(card);
        return current;
    }

    /// <summary>Closes the popup if one is open. Safe to call at any time.</summary>
    public static void CloseCurrent()
    {
        if (current != null) Destroy(current.gameObject);
        current = null;
    }

    /// <summary>Makes sure the popup is visible and clickable above everything else on screen.</summary>
    private void PrepareOnTop()
    {
        var rect = (RectTransform)transform;
        rect.localScale = Vector3.one;
        var pos = rect.anchoredPosition3D;
        rect.anchoredPosition3D = new Vector3(pos.x, pos.y, 0f);
        rect.SetAsLastSibling();

        // Own sorting layer: guarantees it is drawn above the sheet even if the sheet has its own Canvas.
        var canvas = GetComponent<Canvas>();
        if (canvas == null) canvas = gameObject.AddComponent<Canvas>();
        canvas.overrideSorting = true;
        canvas.sortingOrder = 1000;
        if (GetComponent<GraphicRaycaster>() == null) gameObject.AddComponent<GraphicRaycaster>();

        gameObject.SetActive(true);
    }

    private void Fill(EquipmentCard card)
    {
        if (titleText != null) titleText.text = EquipmentCardText.DisplayName(card);
        if (bodyText != null) bodyText.text = EquipmentCardText.Describe(card);
        if (descriptionText != null) descriptionText.text = card.description;
        if (illustrationImage != null)
        {
            illustrationImage.sprite = card.illustration;
            illustrationImage.enabled = card.illustration != null;
        }
    }

    // A click anywhere on the panel (or on its child texts/images) closes it.
    public void OnPointerClick(PointerEventData eventData) => Destroy(gameObject);

    private void OnDestroy()
    {
        if (current == this) current = null;
    }
}
