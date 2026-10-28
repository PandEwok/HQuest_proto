using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Prefab script: one entry (weapon, armor piece or item) in the sheet.
/// Clicking it spawns the item detail popup (a prefab) on top of the UI.
/// Prefab layout: row (this script + a Button + an Image) > TMP_Text label, optional "equipped" marker.
/// If you add a separate Details Button, assign it; otherwise the Button on this same object is used.
/// </summary>
public class EquipmentButton : MonoBehaviour
{
    [SerializeField] private TMP_Text label;
    [SerializeField] private Button detailsButton;      // optional, see above
    [SerializeField] private GameObject equippedMarker; // optional (small icon / star)
    [SerializeField] private EquipmentDetailPanel detailPanelPrefab;

    private EquipmentCard card;

    /// <param name="labelFormat">Button text, see EquipmentCardText.Label. Empty = just the name.</param>
    public void Setup(EquipmentCard newCard, bool equipped, string labelFormat = "")
    {
        card = newCard;

        if (label != null) label.text = EquipmentCardText.Label(card, labelFormat);
        else Debug.LogWarning("EquipmentButton: assign the Label (a TMP_Text) on the prefab.", this);

        if (equippedMarker != null) equippedMarker.SetActive(equipped);

        Button button = detailsButton != null ? detailsButton : GetComponent<Button>();
        if (button == null)
        {
            Debug.LogError("EquipmentButton: assign a Details Button or put a Button on this object.", this);
            return;
        }

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OpenDetails);
    }

    private void OpenDetails()
    {
        // This line proves the click arrived. If you never see it in the Console, the problem is
        // the Button / raycasts (see the checklist). Delete it once everything works.
        Debug.Log($"EquipmentButton: clicked '{EquipmentCardText.DisplayName(card)}'", this);

        if (detailPanelPrefab == null)
        {
            Debug.LogError("EquipmentButton: assign the Detail Panel Prefab on the equipment button prefab.", this);
            return;
        }

        // Spawn under the top-level Canvas so the popup is not clipped by the sliding sheet.
        Canvas canvas = GetComponentInParent<Canvas>();
        Transform parent = canvas != null ? canvas.rootCanvas.transform : transform;
        EquipmentDetailPanel.Open(detailPanelPrefab, parent, card);
    }
}
