using System;
using System.Linq;

/// <summary>
/// Third half of HeroCard (the class is "partial"): giving an item or artifact to another hero.
/// Works on runtime copies only, like the rest of the inventory.
///
/// The item leaves the giver (and is unequipped if it was equipped) and arrives in the recipient's
/// inventory NOT equipped. The recipient's class restrictions are checked first, so nothing is lost
/// when the transfer is refused. Artifact usage limits (once per quest / single use) travel with the card.
/// </summary>
public partial class HeroCard
{
    /// <summary>Fired on the giver after a successful gift: (recipient, card).</summary>
    public event Action<HeroCard, CardData> ItemGiven;

    /// <summary>True if this hero holds the card (equipment, artifact or potion).</summary>
    public bool Owns(CardData card)
    {
        if (card == null) return false;
        if (card is ArtifactCard artifact) return artifacts.Contains(artifact);
        if (card is EquipmentCard equipment)
            return equipment.slot == EquipmentSlot.Consumable ? potions.Contains(equipment) : owned.Contains(equipment);
        return potions.Contains(card);
    }

    /// <summary>True if this hero's class is allowed to hold the card.</summary>
    public bool CanAccept(CardData card)
    {
        if (!IsRuntimeCopy || card == null) return false;
        if (card is ArtifactCard artifact) return artifact.CanBeUsedBy(heroClass);
        if (card is EquipmentCard equipment) return equipment.CanBeUsedBy(heroClass);
        return IsPotion(card);
    }

    public bool CanGive(CardData card, HeroCard recipient) =>
        IsRuntimeCopy && recipient != null && recipient != this && Owns(card) && recipient.CanAccept(card);

    /// <summary>Moves the card from this hero to the recipient. Returns false (and changes nothing) if not allowed.</summary>
    public bool GiveTo(HeroCard recipient, CardData card)
    {
        if (!CanModify() || !CanGive(card, recipient)) return false;

        if (card is ArtifactCard artifact)
        {
            // Remember which abilities were already used, so giving the card does not "refresh" it.
            var usedThisQuest = abilitiesUsedThisQuest.Where(k => k.card == artifact).ToList();
            var spent = abilitiesSpent.Where(k => k.card == artifact).ToList();

            RemoveArtifact(artifact);
            abilitiesSpent.RemoveWhere(k => k.card == artifact);

            recipient.AddArtifact(artifact);
            foreach (var k in usedThisQuest) recipient.abilitiesUsedThisQuest.Add(k);
            foreach (var k in spent) recipient.abilitiesSpent.Add(k);
        }
        else if (card is EquipmentCard equipment)
        {
            RemoveEquipment(equipment);
            recipient.AddEquipment(equipment);
        }
        else
        {
            potions.Remove(card); // removes ONE copy
            NotifyChanged();
            recipient.AddPotion(card);
        }

        ItemGiven?.Invoke(recipient, card);
        return true;
    }
}
