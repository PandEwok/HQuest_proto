using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

/// <summary>
/// Turns a card (EquipmentCard, ArtifactCard or a potion/TreasureCard) into text for the UI.
/// Names come from CardData.title. Class and tag lines show their `id` strings,
/// because HeroClass / EquipmentTag / MonsterTag have no display-name field.
/// </summary>
public static class EquipmentCardText
{
    public static string DisplayName(CardData card) =>
        string.IsNullOrWhiteSpace(card.title) ? card.name : card.title;

    /// <summary>
    /// Text of an equipment button, built from a format string you write in the Inspector.
    /// Placeholders: {name} {type} {attack} {defense} {price}
    /// Example: "{name}  (ATK {attack})"  ->  "Broadsword  (ATK 3)"
    /// Rich text works too, for example bold around {name}. An empty format just shows the name.
    /// Artifacts and potions have no price here: {price} shows "-".
    /// </summary>
    public static string Label(CardData card, string format)
    {
        if (string.IsNullOrWhiteSpace(format)) return DisplayName(card);

        string type = "", attack = "0", defense = "0", price = "-";
        switch (card)
        {
            case EquipmentCard e:
                type = SlotLabel(e.slot);
                attack = e.attackDice.ToString();
                defense = e.defenseDice.ToString();
                price = e.price.ToString();
                break;
            case ArtifactCard a:
                type = "Artifact";
                attack = a.attackDice.ToString();
                defense = a.defenseDice.ToString();
                break;
            case TreasureCard _:
                type = "Potion";
                break;
        }

        return format
            .Replace("{name}", DisplayName(card))
            .Replace("{type}", type)
            .Replace("{attack}", attack)
            .Replace("{defense}", defense)
            .Replace("{price}", price);
    }

    public static string SlotLabel(EquipmentSlot slot)
    {
        switch (slot)
        {
            case EquipmentSlot.BodyArmor: return "Body armor";
            default: return slot.ToString();
        }
    }

    /// <summary>The stats block of the detail popup.</summary>
    public static string Describe(CardData card)
    {
        switch (card)
        {
            case EquipmentCard e: return DescribeEquipment(e);
            case ArtifactCard a: return DescribeArtifact(a);
            case TreasureCard t: return DescribeTreasure(t);
            default: return "";
        }
    }

    // ------------------------------------------------------------ equipment

    private static string DescribeEquipment(EquipmentCard c)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"<b>Type:</b> {SlotLabel(c.slot)}");
        sb.AppendLine($"<b>Price:</b> {c.price} gold coins");

        if (c.attackDice > 0) sb.AppendLine($"<b>Attack dice:</b> {c.attackDice}");
        if (c.defenseDice > 0) sb.AppendLine($"<b>Defend dice:</b> +{c.defenseDice}");
        if (c.diagonalAttack) sb.AppendLine("Can attack diagonally");
        if (c.blocksShield) sb.AppendLine("Cannot be used with a shield");
        if (c.maxMoveDice > 0) sb.AppendLine($"Limits movement to {c.maxMoveDice} red die");

        if (c.onlyFor != null && c.onlyFor.Length > 0)
            sb.AppendLine($"<b>Only for:</b> {JoinIds(c.onlyFor)}");
        if (c.forbiddenFor != null && c.forbiddenFor.Length > 0)
            sb.AppendLine($"<b>Not usable by:</b> {JoinIds(c.forbiddenFor)}");
        if (c.tags != null && c.tags.Length > 0)
            sb.AppendLine($"<b>Tags:</b> {JoinIds(c.tags)}");

        if (c.abilities != null && c.abilities.Length > 0)
        {
            sb.AppendLine();
            sb.AppendLine("<b>Powers</b>");
            foreach (var ability in c.abilities)
                sb.AppendLine("• " + DescribeAbility(ability));
        }

        return sb.ToString().TrimEnd();
    }

    private static string DescribeAbility(EquipmentAbility a)
    {
        string text;
        switch (a.type)
        {
            case EquipmentAbilityType.RangedAttack: text = "Ranged attack"; break;
            case EquipmentAbilityType.ThrowableWeapon: text = "Can be thrown"; break;
            case EquipmentAbilityType.DestroyMonster: text = "Can destroy a monster"; break;
            case EquipmentAbilityType.MoveDiceMultiplier: text = $"Movement dice x{a.value}"; break;
            case EquipmentAbilityType.DisarmTrap: text = "Can disarm traps"; break;
            default: text = a.type.ToString(); break;
        }

        if (a.value != 0 && a.type != EquipmentAbilityType.MoveDiceMultiplier)
            text += $" ({a.value})";

        // requiredTargetTag is a MonsterTag asset; empty means "no condition".
        if (a.requiredTargetTag != null)
            text += $", only against {a.requiredTargetTag.id}";

        return text;
    }

    // ------------------------------------------------------------ artifacts

    private static string DescribeArtifact(ArtifactCard c)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"<b>Type:</b> Artifact ({c.slot})");

        if (c.attackDice != 0) sb.AppendLine($"<b>Attack dice:</b> {Signed(c.attackDice)}");
        if (c.defenseDice != 0) sb.AppendLine($"<b>Defend dice:</b> {Signed(c.defenseDice)}");
        if (c.bonusHP != 0) sb.AppendLine($"<b>Body points:</b> {Signed(c.bonusHP)}");
        if (c.bonusMind != 0) sb.AppendLine($"<b>Mind points:</b> {Signed(c.bonusMind)}");
        if (c.diagonalAttack) sb.AppendLine("Can attack diagonally");

        if (c.abilities != null && c.abilities.Length > 0)
        {
            sb.AppendLine();
            sb.AppendLine("<b>Powers</b>");
            foreach (var ability in c.abilities)
                sb.AppendLine("• " + DescribeArtifactAbility(ability));
        }

        return sb.ToString().TrimEnd();
    }

    private static string DescribeArtifactAbility(ArtifactAbility a)
    {
        string text = Nice(a.type.ToString());
        if (a.value != 0) text += $" ({a.value})";
        if (a.requiredTargetTag != null) text += $", only against {a.requiredTargetTag.id}";

        switch (a.usage)
        {
            case UsageLimit.OncePerQuest: text += " - once per quest"; break;
            case UsageLimit.SingleUse: text += " - single use"; break;
        }
        return text;
    }

    // ------------------------------------------------------------ potions

    private static string DescribeTreasure(TreasureCard t)
    {
        var sb = new StringBuilder();
        sb.AppendLine("<b>Type:</b> Potion");
        if (t.singleUse) sb.AppendLine("Single use");

        bool header = false;
        if (t.effects != null)
        {
            foreach (var effect in t.effects)
            {
                if (!header)
                {
                    sb.AppendLine();
                    sb.AppendLine("<b>Effects</b>");
                    header = true;
                }
                string line = "• " + Nice(effect.type.ToString());
                if (effect.value != 0) line += $" ({effect.value})";
                sb.AppendLine(line);
            }
        }

        return sb.ToString().TrimEnd();
    }

    // ------------------------------------------------------------ helpers

    private static string Signed(int value) => value > 0 ? "+" + value : value.ToString();

    /// <summary>"BonusAttackDice" -> "Bonus Attack Dice".</summary>
    private static string Nice(string enumName) => Regex.Replace(enumName, "(?<!^)([A-Z])", " $1");

    private static string JoinIds(HeroClass[] items)
    {
        var ids = new List<string>();
        foreach (var item in items) if (item != null) ids.Add(item.id);
        return string.Join(", ", ids);
    }

    private static string JoinIds(EquipmentTag[] items)
    {
        var ids = new List<string>();
        foreach (var item in items) if (item != null) ids.Add(item.id);
        return string.Join(", ", ids);
    }
}
