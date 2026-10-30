using System.Collections.Generic;

/// <summary>
/// Sorts attribués à chaque héros après l'écran de sélection des sorts.
/// Clé = HeroCard.title (même convention que GameManager : "Wizard", "Elf"...).
/// </summary>
public static class SpellBook
{
    private static readonly Dictionary<string, List<SpellCard>> spells = new();
    private static readonly List<SpellCard> empty = new();

    public static void Clear() => spells.Clear();

    public static void Set(string heroTitle, IEnumerable<SpellCard> list)
    {
        spells[heroTitle] = new List<SpellCard>(list);
    }

    public static IReadOnlyList<SpellCard> GetSpells(string heroTitle)
        => spells.TryGetValue(heroTitle, out var list) ? list : empty;

    public static IReadOnlyList<SpellCard> GetSpells(HeroCard hero)
        => hero == null ? empty : GetSpells(hero.title);
}
