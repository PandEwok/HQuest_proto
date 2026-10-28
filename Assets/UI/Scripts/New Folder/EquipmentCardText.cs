using System.Collections.Generic;
using System.Text;

/// <summary>
/// Turns an EquipmentCard into text for the UI.
/// Names come from CardData.title. Class and tag lines show their `id` strings,
/// because HeroClass / EquipmentTag / MonsterTag have no display-name field.
/// </summary>
public static class EquipmentCardText
{
    public static string DisplayName(EquipmentCard card) =>
        string.IsNullOrWhiteSpace(card.title) ? card.name : card.title;

    /// <summary>
    /// Text of an equipment button, built from a format string you write in the Inspector.
    /// Placeholders: {name} {type} {attack} {defense} {price}
    /// Example: "{name}  (ATK {attack})"  ->  "Broadsword  (ATK 3)"
    /// Rich text works too: "&lt;b&gt;{name}&lt;/b&gt;" (written as <b>{name}</b>).
    /// An empty format just shows the name.
    /// </summary>
    public static string Label(EquipmentCard card, string format)
    {
        if (string.IsNullOrWhiteSpace(format)) return DisplayName(card);

        return format
            .Replace("{name}", DisplayName(card))
            .Replace("{type}", SlotLabel(card.slot))
            .Replace("{attack}", card.attackDice.ToString())
            .Replace("{defense}", card.defenseDice.ToString())
            .Replace("{price}", card.price.ToString());
    }

    public static string SlotLabel(EquipmentSlot slot)
    {
        switch (slot)
        {
            case EquipmentSlot.BodyArmor: return "Body armor";
            default: return slot.ToString();
        }
    }

    public static string Describe(EquipmentCard c)
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
