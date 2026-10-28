using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text;
using System.Collections.Generic;
using System.Reflection;

public class CardInfoDisplay : MonoBehaviour
{
    [Header("Testing in Inspector")]
    [Tooltip("Drag any CardData asset here (like BattleAxe) to display it automatically on Play")]
    [SerializeField] private CardData cardToDisplay;

    [Header("UI References")]
    [SerializeField] private Image cardImage;           // Card -> Image
    [SerializeField] private TextMeshProUGUI cardText;  // Card -> CardText (Name)
    [SerializeField] private TextMeshProUGUI debugText; // DebugText on CardInfo

    private void Start()
    {
        if (cardToDisplay != null)
        {
            DisplayCard(cardToDisplay);
        }
        else
        {
            ClearCardDisplay();
        }
    }

    /// <summary>
    /// Reads and displays any CardData asset onto the UI elements and DebugText.
    /// </summary>
    public void DisplayCard(CardData cardData)
    {
        if (cardData == null)
        {
            ClearCardDisplay();
            return;
        }

        // 1. Get Name / Title (fallback to ScriptableObject asset name if needed)
        string cardName = cardData.name;
        string cardTitle = GetFieldValue(cardData, "cardName") as string ??
                           GetFieldValue(cardData, "title") as string ??
                           cardName;

        // 2. Set the Name in Card -> CardText
        if (cardText != null) cardText.text = cardTitle;

        // 3. Handle Illustration / Sprite (checks fields like illustration, sprite, or icon)
        Sprite cardSprite = GetCardSprite(cardData);
        if (cardImage != null)
        {
            if (cardSprite != null)
            {
                cardImage.gameObject.SetActive(true);
                cardImage.sprite = cardSprite;
            }
            else
            {
                cardImage.gameObject.SetActive(false); // Hide image if none assigned
            }
        }

        // 4. Generate the full formatted breakdown for DebugText
        string fullDataDebug = BuildCardDebugString(cardData, cardTitle);
        if (debugText != null)
        {
            debugText.text = fullDataDebug;
        }
    }

    private Sprite GetCardSprite(CardData cardData)
    {
        // Check specific card types first
        if (cardData is MonsterCard monster) return monster.icon;

        // Use reflection to safely check common asset sprite property/field names
        Sprite sprite = (Sprite)GetFieldValue(cardData, "illustration") ??
                        (Sprite)GetFieldValue(cardData, "sprite") ??
                        (Sprite)GetFieldValue(cardData, "icon");
        return sprite;
    }

    private object GetFieldValue(object obj, string fieldName)
    {
        var type = obj.GetType();
        FieldInfo field = type.GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        if (field != null) return field.GetValue(obj);

        PropertyInfo prop = type.GetProperty(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        if (prop != null) return prop.GetValue(obj);

        return null;
    }

    private string BuildCardDebugString(CardData cardData, string cardTitle)
    {
        StringBuilder sb = new StringBuilder();

        // Safely extract common base fields if they exist in CardData
        string cardId = GetFieldValue(cardData, "id") as string ?? "N/A";
        string cardDesc = GetFieldValue(cardData, "description") as string ?? "N/A";

        sb.AppendLine($"<b>Title:</b> {cardTitle}");
        sb.AppendLine($"<b>ID:</b> {cardId}");
        sb.AppendLine($"<b>Description:</b> {cardDesc}");
        sb.AppendLine($"<b>Type:</b> {cardData.GetType().Name}");
        sb.AppendLine("-----------------------------------");

        // Specific formatting based on card type
        switch (cardData)
        {
            case EquipmentCard eq:
                sb.AppendLine($"<b>Slot:</b> {eq.slot}");
                sb.AppendLine($"<b>Price:</b> {eq.price} Gold");
                sb.AppendLine($"<b>Attack Dice:</b> {eq.attackDice}");
                sb.AppendLine($"<b>Defense Dice:</b> {eq.defenseDice}");
                sb.AppendLine($"<b>Diagonal Attack:</b> {eq.diagonalAttack}");
                sb.AppendLine($"<b>Blocks Shield:</b> {eq.blocksShield}");
                sb.AppendLine($"<b>Max Move Dice:</b> {eq.maxMoveDice}");

                if (eq.onlyFor != null && eq.onlyFor.Length > 0)
                    sb.AppendLine($"<b>Only For:</b> {string.Join(", ", (IEnumerable<object>)eq.onlyFor)}");
                if (eq.forbiddenFor != null && eq.forbiddenFor.Length > 0)
                    sb.AppendLine($"<b>Forbidden For:</b> {string.Join(", ", (IEnumerable<object>)eq.forbiddenFor)}");

                if (eq.abilities != null && eq.abilities.Length > 0)
                {
                    sb.AppendLine("<b>Abilities:</b>");
                    foreach (var ab in eq.abilities)
                    {
                        sb.AppendLine($" - {ab.type} (Value: {ab.value})");
                    }
                }
                break;

            case ArtifactCard art:
                sb.AppendLine($"<b>Slot:</b> {art.slot}");
                sb.AppendLine($"<b>Attack Dice:</b> {art.attackDice}");
                sb.AppendLine($"<b>Defense Dice:</b> {art.defenseDice}");
                sb.AppendLine($"<b>Bonus HP:</b> {art.bonusHP} | <b>Bonus Mind:</b> {art.bonusMind}");
                if (art.abilities != null && art.abilities.Length > 0)
                {
                    sb.AppendLine("<b>Abilities:</b>");
                    foreach (var ab in art.abilities)
                        sb.AppendLine($" - {ab.type} (Usage: {ab.usage})");
                }
                break;

            case MonsterCard mon:
                sb.AppendLine($"<b>Move Squares:</b> {mon.moveSquares}");
                sb.AppendLine($"<b>Attack Dice:</b> {mon.attackDice}");
                sb.AppendLine($"<b>Defense Dice:</b> {mon.defenseDice}");
                sb.AppendLine($"<b>HP:</b> {mon.hp} | <b>Mind:</b> {mon.mind}");
                if (mon.tags != null && mon.tags.Length > 0)
                {
                    sb.AppendLine($"<b>Tags:</b> {string.Join(", ", (IEnumerable<object>)mon.tags)}");
                }
                break;

            case HeroCard hero:
                sb.AppendLine($"<b>Hero Class:</b> {hero.heroClass}");
                sb.AppendLine($"<b>Attack Dice:</b> {hero.attackDice} | <b>Defense Dice:</b> {hero.defenseDice}");
                sb.AppendLine($"<b>Start HP:</b> {hero.startHP} | <b>Start Mind:</b> {hero.startMind}");
                sb.AppendLine($"<b>Move Dice:</b> {hero.moveDice}");
                sb.AppendLine($"<b>Start Weapon:</b> {(hero.startWeapon != null ? hero.startWeapon.name : "None")}");
                sb.AppendLine($"<b>Start Armor:</b> {(hero.startArmor != null ? hero.startArmor.name : "None")}");
                break;

            default:
                sb.AppendLine("Generic Card Data loaded.");
                break;
        }

        return sb.ToString();
    }

    private void ClearCardDisplay()
    {
        if (cardText != null) cardText.text = "";
        if (debugText != null) debugText.text = "No card assigned.";
        if (cardImage != null)
        {
            cardImage.sprite = null;
            cardImage.gameObject.SetActive(false);
        }
    }
}