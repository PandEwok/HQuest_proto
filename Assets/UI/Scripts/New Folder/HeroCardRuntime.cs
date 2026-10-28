using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Runtime half of HeroCard (the class must be declared "partial" in HeroCard.cs).
///
/// A HeroCard asset is the TEMPLATE and must never be changed during play.
/// HeroCard.CreateRuntimeCopy() makes a per-player COPY of it; the copy holds the
/// live values: current body/mind, dice modifiers, gold, equipment, quests.
/// The copy keeps every original field (startHP, startWeapon, heroClass, ...), so the
/// rest of your code can keep reading it as a normal HeroCard.
/// </summary>
public partial class HeroCard
{
    public const int TotalQuests = 14;

    public string PlayerName { get; private set; }
    public string PlayerDescription { get; private set; }
    public bool IsRuntimeCopy { get; private set; }

    public int CurrentBody { get; private set; }
    public int CurrentMind { get; private set; }
    public int Gold { get; private set; }
    public EquipmentCard EquippedWeapon { get; private set; }

    private readonly List<EquipmentCard> owned = new List<EquipmentCard>();
    private readonly Dictionary<EquipmentSlot, EquipmentCard> worn = new Dictionary<EquipmentSlot, EquipmentCard>();
    private readonly bool[] questsCompleted = new bool[TotalQuests];
    private int attackModifier;
    private int defenseModifier;

    /// <summary>Fired after any change, so the sheet can refresh itself.</summary>
    public event Action Changed;

    // ------------------------------------------------------------ creation

    /// <summary>Makes the player's own copy of a HeroCard asset and fills in the starting values.</summary>
    public static HeroCard CreateRuntimeCopy(HeroCard template, string playerName, string playerDescription)
    {
        var copy = UnityEngine.Object.Instantiate(template);
        copy.name = template.name;
        copy.InitRuntime(playerName, playerDescription);
        return copy;
    }

    private void InitRuntime(string playerName, string playerDescription)
    {
        IsRuntimeCopy = true;
        PlayerName = string.IsNullOrWhiteSpace(playerName) ? "Nameless Hero" : playerName.Trim();
        PlayerDescription = playerDescription == null ? "" : playerDescription.Trim();

        CurrentBody = startHP;
        CurrentMind = startMind;
        Gold = 0;
        attackModifier = 0;
        defenseModifier = 0;
        EquippedWeapon = null;
        owned.Clear();
        worn.Clear();
        Array.Clear(questsCompleted, 0, questsCompleted.Length);
        InitInventory();   // artifacts + potions (HeroCardInventory.cs)

        if (startWeapon != null && !Equip(startWeapon))
            Debug.LogWarning($"{title}: start weapon '{startWeapon.title}' cannot be used by this hero class.");
        if (startArmor != null && !Equip(startArmor))
            Debug.LogWarning($"{title}: start armor '{startArmor.title}' cannot be used by this hero class.");
    }

    /// <summary>Stops accidental edits of the asset itself (they would persist in the editor).</summary>
    private bool CanModify()
    {
        if (IsRuntimeCopy) return true;
        Debug.LogError($"{name} is a HeroCard asset, not a runtime copy. Use HeroCard.CreateRuntimeCopy() first.", this);
        return false;
    }

    private void NotifyChanged() => Changed?.Invoke();

    // ---------------------------------------------------------------- dice

    /// <summary>Equipped weapon's attack dice (or the card value), plus temporary modifiers. Minimum 1.</summary>
    public int CurrentAttackDice
    {
        get
        {
            int dice = EquippedWeapon != null ? Mathf.Max(1, EquippedWeapon.attackDice) : attackDice;
            return Mathf.Max(1, dice + attackModifier + ArtifactAttackDice);
        }
    }

    /// <summary>Card value + worn armor + temporary modifiers. A shield is ignored if the weapon blocks shields. Minimum 1.</summary>
    public int CurrentDefenseDice
    {
        get
        {
            int dice = defenseDice + defenseModifier + ArtifactDefenseDice;
            foreach (var piece in worn.Values)
            {
                bool shieldBlocked = piece.slot == EquipmentSlot.Shield
                                     && EquippedWeapon != null
                                     && EquippedWeapon.blocksShield;
                if (!shieldBlocked) dice += piece.defenseDice;
            }
            return Mathf.Max(1, dice);
        }
    }

    /// <summary>Temporary change to attack dice (spells, pit penalty...). Use a negative number to lower.</summary>
    public void ModifyAttackDice(int delta)
    {
        if (!CanModify()) return;
        attackModifier += delta;
        NotifyChanged();
    }

    public void ModifyDefenseDice(int delta)
    {
        if (!CanModify()) return;
        defenseModifier += delta;
        NotifyChanged();
    }

    public void ResetDiceModifiers()
    {
        if (!CanModify()) return;
        attackModifier = 0;
        defenseModifier = 0;
        NotifyChanged();
    }

    // ---------------------------------------------------------- body / mind

    public void ModifyBodyPoints(int delta) => SetBodyPoints(CurrentBody + delta);

    public void SetBodyPoints(int value)
    {
        if (!CanModify()) return;
        CurrentBody = Mathf.Clamp(value, 0, MaxBody);
        NotifyChanged();
    }

    public void ModifyMindPoints(int delta) => SetMindPoints(CurrentMind + delta);

    public void SetMindPoints(int value)
    {
        if (!CanModify()) return;
        CurrentMind = Mathf.Clamp(value, 0, MaxMind);
        NotifyChanged();
    }

    /// <summary>Between quests, starting Body and Mind points are restored.</summary>
    public void RestoreBodyAndMind()
    {
        if (!CanModify()) return;
        CurrentBody = MaxBody;
        CurrentMind = MaxMind;
        ClearQuestState();
        NotifyChanged();
    }

    // ----------------------------------------------------------------- gold

    public void AddGold(int amount)
    {
        if (!CanModify() || amount <= 0) return;
        Gold += amount;
        NotifyChanged();
    }

    public bool TrySpendGold(int amount)
    {
        if (!CanModify() || amount < 0 || amount > Gold) return false;
        Gold -= amount;
        NotifyChanged();
        return true;
    }

    public void SetGold(int value)
    {
        if (!CanModify()) return;
        Gold = Mathf.Max(0, value);
        NotifyChanged();
    }

    // ----------------------------------------------------------- equipment

    public List<EquipmentCard> GetOwned(params EquipmentSlot[] slots) =>
        owned.Concat(potions.OfType<EquipmentCard>()).Where(c => Array.IndexOf(slots, c.slot) >= 0).ToList();

    public bool IsEquipped(EquipmentCard card) =>
        card != null && (card == EquippedWeapon || worn.ContainsValue(card));

    /// <summary>Adds to the inventory without equipping (potions, tool kits, spare weapons).</summary>
    public bool AddEquipment(EquipmentCard card)
    {
        if (!CanModify() || !TryOwn(card)) return false;
        NotifyChanged();
        return true;
    }

    /// <summary>Adds to the inventory and equips. Weapons: one at a time. Armor: one per slot.</summary>
    public bool Equip(EquipmentCard card)
    {
        if (!CanModify() || !TryOwn(card)) return false;

        switch (card.slot)
        {
            case EquipmentSlot.Weapon:
                EquippedWeapon = card;
                break;
            case EquipmentSlot.BodyArmor:
            case EquipmentSlot.Helmet:
            case EquipmentSlot.Shield:
                worn[card.slot] = card;
                break;
        }
        NotifyChanged();
        return true;
    }

    /// <summary>Removes an item (e.g. a potion that was drunk).</summary>
    public void RemoveEquipment(EquipmentCard card)
    {
        if (!CanModify() || card == null) return;
        if (card.slot == EquipmentSlot.Consumable)
        {
            if (potions.Remove(card)) NotifyChanged();   // removes ONE copy
            return;
        }
        if (!owned.Remove(card)) return;
        if (EquippedWeapon == card) EquippedWeapon = null;
        if (worn.TryGetValue(card.slot, out var current) && current == card) worn.Remove(card.slot);
        NotifyChanged();
    }

    private bool TryOwn(EquipmentCard card)
    {
        if (card == null || !card.CanBeUsedBy(heroClass)) return false;
        if (card.slot == EquipmentSlot.Consumable)
        {
            potions.Add(card);   // potions can be owned several times
            return true;
        }
        if (!owned.Contains(card)) owned.Add(card);
        return true;
    }

    // --------------------------------------------------------------- quests

    public int QuestsCompletedCount => questsCompleted.Count(done => done);

    public bool IsQuestCompleted(int questNumber) =>
        questNumber >= 1 && questNumber <= TotalQuests && questsCompleted[questNumber - 1];

    /// <summary>Call this when the heroes finish a quest (1 to 14).</summary>
    public void CompleteQuest(int questNumber)
    {
        if (!CanModify()) return;
        if (questNumber < 1 || questNumber > TotalQuests)
        {
            Debug.LogWarning($"CompleteQuest: quest number {questNumber} is out of range (1-{TotalQuests}).");
            return;
        }
        if (questsCompleted[questNumber - 1]) return;

        questsCompleted[questNumber - 1] = true;
        ClearQuestState();
        NotifyChanged();
    }
}