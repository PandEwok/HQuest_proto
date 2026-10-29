using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Inventory half of HeroCard (artifacts + potions). Same rule as HeroCardRuntime.cs:
/// it only works on the runtime copy (HeroCard.CreateRuntimeCopy), never on the asset.
///
/// ARTIFACTS  (ArtifactCard)
///   - Owned in a list. Slots Weapon / Armor / Ring / Accessory can be EQUIPPED (one per slot).
///   - Slot Consumable is never equipped: it is active as long as it is owned.
///   - Equipped artifacts add dice, max Body/Mind and diagonal attack (see the properties below).
///   - Abilities are NOT applied here (resurrect, teleport...): the game code does that when
///     ArtifactAbilityUsed fires. This class only tracks the usage limits (Unlimited / OncePerQuest / SingleUse).
///
/// POTIONS  (kept TreasureCards + EquipmentCards of slot Consumable)
///   - The same potion can be owned several times (it is a list, not a set).
///   - UsePotion() applies the effect, removes the card and fires ItemConsumed (put it in your discard pile).
///   - "Next attack / next defense / next move" effects are stored as pending bonuses.
///     Your combat / movement code reads them with the Consume...() methods.
/// </summary>
public partial class HeroCard
{
    // ------------------------------------------------------------------ state
    private readonly List<ArtifactCard> artifacts = new List<ArtifactCard>();
    private readonly Dictionary<ArtifactSlot, ArtifactCard> equippedArtifacts = new Dictionary<ArtifactSlot, ArtifactCard>();
    private readonly List<CardData> potions = new List<CardData>();

    private readonly HashSet<(ArtifactCard card, int ability)> abilitiesUsedThisQuest = new HashSet<(ArtifactCard card, int ability)>();
    private readonly HashSet<(ArtifactCard card, int ability)> abilitiesSpent = new HashSet<(ArtifactCard card, int ability)>();

    private int pendingAttackBonusDice;
    private int pendingDefenseBonusDice;
    private int pendingAttackCount = 1;
    private int pendingMoveMultiplier = 1;

    // ------------------------------------------------------------------ events

    /// <summary>A potion / consumable was used up. Send the card to the discard pile.</summary>
    public event Action<CardData> ItemConsumed;

    /// <summary>An artifact ability was triggered. The game code applies the actual effect.</summary>
    public event Action<ArtifactCard, ArtifactAbility> ArtifactAbilityUsed;

    /// <summary>A potion effect that needs the game (EndTurn, SpawnWanderingMonster).</summary>
    public event Action<TreasureEffect> GameEffectRequested;

    /// <summary>A consumable equipment ability other than MoveDiceMultiplier (handled by the game).</summary>
    public event Action<EquipmentAbility> ConsumableAbilityRequested;

    private void InitInventory()
    {
        artifacts.Clear();
        equippedArtifacts.Clear();
        potions.Clear();
        abilitiesUsedThisQuest.Clear();
        abilitiesSpent.Clear();
        ClearQuestState();
    }

    // ------------------------------------------------------------------ read access (for the UI)

    public IReadOnlyList<ArtifactCard> Artifacts => artifacts;
    public IEnumerable<ArtifactCard> EquippedArtifacts => equippedArtifacts.Values;

    /// <summary>Kept treasure cards and consumable equipment (duplicates included).</summary>
    public IReadOnlyList<CardData> Potions => potions;

    public ArtifactCard GetEquippedArtifact(ArtifactSlot slot) =>
        equippedArtifacts.TryGetValue(slot, out var card) ? card : null;

    public bool IsArtifactEquipped(ArtifactCard card) =>
        card != null && equippedArtifacts.TryGetValue(card.slot, out var current) && current == card;

    /// <summary>How many copies of this potion the hero holds.</summary>
    public int CountPotion(CardData potion) => potions.Count(p => p == potion);

    // ------------------------------------------------------------------ passive bonuses from equipped artifacts

    public int ArtifactAttackDice => EquippedArtifacts.Sum(a => a.attackDice);
    public int ArtifactDefenseDice => EquippedArtifacts.Sum(a => a.defenseDice);

    /// <summary>Starting Body points + equipped artifact bonuses.</summary>
    public int MaxBody => Mathf.Max(1, startHP + EquippedArtifacts.Sum(a => a.bonusHP));

    /// <summary>Starting Mind points + equipped artifact bonuses.</summary>
    public int MaxMind => Mathf.Max(0, startMind + EquippedArtifacts.Sum(a => a.bonusMind));

    public bool CanAttackDiagonally =>
        (EquippedWeapon != null && EquippedWeapon.diagonalAttack) || EquippedArtifacts.Any(a => a.diagonalAttack);

    /// <summary>Extra attack dice from BonusAttackDice abilities against this target (usage limits are ignored).</summary>
    public int GetArtifactBonusAttackDice(ICollection<MonsterTag> targetTags = null) =>
        GetArtifactAbilities(AbilityType.BonusAttackDice, targetTags).Sum(x => x.ability.value);

    // ------------------------------------------------------------------ artifacts

    /// <summary>Adds an artifact to the inventory without equipping it.</summary>
    public bool AddArtifact(ArtifactCard card)
    {
        if (!CanModify() || !TryOwnArtifact(card)) return false;
        NotifyChanged();
        return true;
    }

    /// <summary>Adds the artifact (if needed) and equips it. It replaces whatever is in the same slot.</summary>
    public bool EquipArtifact(ArtifactCard card)
    {
        if (!CanModify() || card == null) return false;
        if (card.slot == ArtifactSlot.Consumable)
        {
            Debug.LogWarning($"{card.title}: consumable artifacts are not equipped, they are active while owned.");
            return false;
        }
        if (!TryOwnArtifact(card)) return false;

        equippedArtifacts[card.slot] = card;
        ClampToMax();
        NotifyChanged();
        return true;
    }

    public void UnequipArtifact(ArtifactSlot slot)
    {
        if (!CanModify() || !equippedArtifacts.Remove(slot)) return;
        ClampToMax();
        NotifyChanged();
    }

    /// <summary>Removes an artifact from the inventory (lost, stolen, discarded...).</summary>
    public void RemoveArtifact(ArtifactCard card)
    {
        if (!CanModify() || card == null || !artifacts.Remove(card)) return;

        if (equippedArtifacts.TryGetValue(card.slot, out var current) && current == card)
            equippedArtifacts.Remove(card.slot);
        abilitiesUsedThisQuest.RemoveWhere(k => k.card == card);

        ClampToMax();
        NotifyChanged();
    }

    private bool TryOwnArtifact(ArtifactCard card)
    {
        if (card == null || !card.CanBeUsedBy(heroClass)) return false;
        if (!artifacts.Contains(card)) artifacts.Add(card);
        return true;
    }

    /// <summary>Unequipping a +HP artifact must not leave Body/Mind above the new maximum.</summary>
    private void ClampToMax()
    {
        CurrentBody = Mathf.Min(CurrentBody, MaxBody);
        CurrentMind = Mathf.Min(CurrentMind, MaxMind);
    }

    // ------------------------------------------------------------------ artifact abilities

    private bool IsArtifactActive(ArtifactCard card) =>
        card.slot == ArtifactSlot.Consumable ? artifacts.Contains(card) : IsArtifactEquipped(card);

    /// <summary>
    /// Abilities of the active artifacts (equipped, or owned for consumables) of a given type.
    /// An ability with a required target tag only matches if that tag is in targetTags.
    /// </summary>
    public List<(ArtifactCard card, int index, ArtifactAbility ability)> GetArtifactAbilities(
        AbilityType type, ICollection<MonsterTag> targetTags = null)
    {
        var result = new List<(ArtifactCard card, int index, ArtifactAbility ability)>();
        foreach (var card in artifacts)
        {
            if (!IsArtifactActive(card) || card.abilities == null) continue;

            for (int i = 0; i < card.abilities.Length; i++)
            {
                var ability = card.abilities[i];
                if (ability.type != type) continue;
                if (ability.requiredTargetTag != null
                    && (targetTags == null || !targetTags.Contains(ability.requiredTargetTag))) continue;

                result.Add((card, i, ability));
            }
        }
        return result;
    }

    public bool CanUseArtifactAbility(ArtifactCard card, int abilityIndex)
    {
        if (card == null || !artifacts.Contains(card) || !IsArtifactActive(card)) return false;
        if (card.abilities == null || abilityIndex < 0 || abilityIndex >= card.abilities.Length) return false;

        var key = (card, abilityIndex);
        switch (card.abilities[abilityIndex].usage)
        {
            case UsageLimit.OncePerQuest: return !abilitiesUsedThisQuest.Contains(key);
            case UsageLimit.SingleUse: return !abilitiesSpent.Contains(key);
            default: return true;
        }
    }

    /// <summary>
    /// Registers the use of an ability (checks and records the usage limit) and fires ArtifactAbilityUsed.
    /// A consumable artifact with no usable ability left is discarded.
    /// </summary>
    public bool TryUseArtifactAbility(ArtifactCard card, int abilityIndex)
    {
        if (!CanModify() || !CanUseArtifactAbility(card, abilityIndex)) return false;

        ArtifactAbility ability = card.abilities[abilityIndex];
        var key = (card, abilityIndex);
        if (ability.usage == UsageLimit.OncePerQuest) abilitiesUsedThisQuest.Add(key);
        else if (ability.usage == UsageLimit.SingleUse) abilitiesSpent.Add(key);

        ArtifactAbilityUsed?.Invoke(card, ability);

        if (card.slot == ArtifactSlot.Consumable && !HasUsableAbility(card))
        {
            artifacts.Remove(card);
            ItemConsumed?.Invoke(card);
        }
        NotifyChanged();
        return true;
    }

    private bool HasUsableAbility(ArtifactCard card)
    {
        if (card.abilities == null) return false;
        for (int i = 0; i < card.abilities.Length; i++)
            if (CanUseArtifactAbility(card, i)) return true;
        return false;
    }

    // ------------------------------------------------------------------ potions

    /// <summary>True for a TreasureCard the hero keeps, or an EquipmentCard of slot Consumable.</summary>
    public static bool IsPotion(CardData card)
    {
        if (card is TreasureCard treasure) return treasure.timing == TreasureTiming.KeptByHero;
        if (card is EquipmentCard equipment) return equipment.slot == EquipmentSlot.Consumable;
        return false;
    }

    /// <summary>Adds a potion (TreasureCard kept by the hero, or consumable EquipmentCard).</summary>
    public bool AddPotion(CardData card)
    {
        if (!CanModify() || !IsPotion(card)) return false;
        if (card is EquipmentCard equipment && !equipment.CanBeUsedBy(heroClass)) return false;

        potions.Add(card);
        NotifyChanged();
        return true;
    }

    /// <summary>
    /// Drinks / uses a potion: applies its effects, removes the card and fires ItemConsumed.
    /// rollDie lets you plug your own dice (default: one 6-sided die).
    /// </summary>
    public bool UsePotion(CardData card, Func<int> rollDie = null)
    {
        if (!CanModify() || card == null || !potions.Contains(card)) return false;

        bool consumed;
        if (card is TreasureCard treasure)
        {
            if (treasure.effects != null)
                foreach (var effect in treasure.effects) ApplyTreasureEffect(effect, rollDie);
            consumed = treasure.singleUse;
        }
        else if (card is EquipmentCard equipment)
        {
            if (equipment.abilities != null)
                foreach (var ability in equipment.abilities) ApplyConsumableAbility(ability);
            consumed = true;   // "once used, the card must be discarded"
        }
        else return false;

        if (consumed)
        {
            potions.Remove(card);   // removes ONE copy
            ItemConsumed?.Invoke(card);
        }
        NotifyChanged();
        return true;
    }

    private void ApplyTreasureEffect(TreasureEffect effect, Func<int> rollDie)
    {
        switch (effect.type)
        {
            case TreasureEffectType.GainGold:
                AddGold(effect.value);
                break;
            case TreasureEffectType.Damage:
                ModifyBodyPoints(-effect.value);
                break;
            case TreasureEffectType.HealByDiceRoll:
                {
                    // value = number of dice; 0 (as in Healing_Potion.asset) is treated as 1 die
                    int dice = Mathf.Max(1, effect.value);
                    int total = 0;
                    for (int i = 0; i < dice; i++)
                        total += rollDie != null ? rollDie() : UnityEngine.Random.Range(1, 7);
                    ModifyBodyPoints(total);   // clamped to MaxBody
                    break;
                }
            case TreasureEffectType.NextAttackBonusDice:
                pendingAttackBonusDice += effect.value;
                break;
            case TreasureEffectType.NextDefenseBonusDice:
                pendingDefenseBonusDice += effect.value;
                break;
            case TreasureEffectType.NextAttackExtraAttacks:
                // value = total number of attacks ("attack twice instead of once" = 2)
                pendingAttackCount = Mathf.Max(pendingAttackCount, effect.value);
                break;
            default: // EndTurn, SpawnWanderingMonster
                GameEffectRequested?.Invoke(effect);
                break;
        }
    }

    private void ApplyConsumableAbility(EquipmentAbility ability)
    {
        if (ability.type == EquipmentAbilityType.MoveDiceMultiplier)
            pendingMoveMultiplier = Mathf.Max(pendingMoveMultiplier, ability.value);
        else
            ConsumableAbilityRequested?.Invoke(ability);
    }

    // ------------------------------------------------------------------ pending potion bonuses

    public int PendingAttackBonusDice => pendingAttackBonusDice;
    public int PendingDefenseBonusDice => pendingDefenseBonusDice;
    public int PendingAttackCount => pendingAttackCount;
    public int PendingMoveDiceMultiplier => pendingMoveMultiplier;

    /// <summary>Call when the hero attacks: returns the bonus dice and the number of attacks, then clears them.</summary>
    public (int bonusDice, int attackCount) ConsumeAttackBonuses()
    {
        if (!CanModify()) return (0, 1);
        var result = (pendingAttackBonusDice, Mathf.Max(1, pendingAttackCount));
        pendingAttackBonusDice = 0;
        pendingAttackCount = 1;
        NotifyChanged();
        return result;
    }

    /// <summary>Call when the hero defends: returns the bonus dice, then clears them.</summary>
    public int ConsumeDefenseBonusDice()
    {
        if (!CanModify()) return 0;
        int bonus = pendingDefenseBonusDice;
        pendingDefenseBonusDice = 0;
        NotifyChanged();
        return bonus;
    }

    /// <summary>Call when the hero moves: returns the multiplier to apply to the movement dice (1 = none), then clears it.</summary>
    public int ConsumeMoveDiceMultiplier()
    {
        if (!CanModify()) return 1;
        int multiplier = Mathf.Max(1, pendingMoveMultiplier);
        pendingMoveMultiplier = 1;
        NotifyChanged();
        return multiplier;
    }

    // ------------------------------------------------------------------ quest reset

    /// <summary>Resets "once per quest" abilities and unused pending potion bonuses.</summary>
    public void ResetQuestState()
    {
        if (!CanModify()) return;
        ClearQuestState();
        NotifyChanged();
    }

    private void ClearQuestState()
    {
        abilitiesUsedThisQuest.Clear();
        pendingAttackBonusDice = 0;
        pendingDefenseBonusDice = 0;
        pendingAttackCount = 1;
        pendingMoveMultiplier = 1;
    }
}