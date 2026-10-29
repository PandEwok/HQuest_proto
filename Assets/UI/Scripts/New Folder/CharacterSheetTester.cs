using UnityEngine;

/// <summary>
/// TEST ONLY - put this on any GameObject in your scene to try the character sheet
/// without the creation screen. Delete it when your friends' creation system is connected.
///
/// How to use: fill the fields in the Inspector, press Play, then open the three-dots menu
/// (or right-click) on this component's header: the "Test: ..." commands change the hero live.
/// The public methods can also be wired to normal UI Buttons.
/// With a second hero template assigned you can test giving items between two heroes;
/// "Switch shown hero" shows the other hero on the sheet.
/// </summary>
public class CharacterSheetTester : MonoBehaviour
{
    [SerializeField] private CharacterSheetPanel sheet;

    [Header("Hero 1 (shown on the sheet at start)")]
    [SerializeField] private HeroCard heroTemplate;   // a HeroCard ASSET from your project
    [SerializeField] private string heroName = "Test Hero";
    [SerializeField, TextArea] private string heroDescription = "A brave adventurer.";

    [Header("Hero 2 (optional, needed to test giving items)")]
    [SerializeField] private HeroCard secondHeroTemplate;
    [SerializeField] private string secondHeroName = "Test Hero 2";

    [Header("Extra items given to hero 1 at start (class restrictions still apply)")]
    [SerializeField] private EquipmentCard[] extraEquipment;
    [SerializeField] private ArtifactCard[] extraArtifacts;
    [SerializeField] private CardData[] extraPotions;

    public HeroCard Hero { get; private set; }
    public HeroCard SecondHero { get; private set; }
    /// <summary>The hero currently displayed on the sheet. The "Test:" commands act on this one.</summary>
    public HeroCard Shown { get; private set; }

    private void Start()
    {
        if (sheet == null || heroTemplate == null)
        {
            Debug.LogError("CharacterSheetTester: assign the Sheet and the Hero Template.", this);
            return;
        }

        // The runtime copy is the player's own hero; the asset is never modified.
        Hero = HeroCard.CreateRuntimeCopy(heroTemplate, heroName, heroDescription);
        GiveStartItems(Hero);
        HeroParty.Register(Hero);

        if (secondHeroTemplate != null)
        {
            SecondHero = HeroCard.CreateRuntimeCopy(secondHeroTemplate, secondHeroName, "");
            HeroParty.Register(SecondHero);
        }

        Shown = Hero;
        sheet.Bind(Hero);
    }

    private void OnDestroy()
    {
        HeroParty.Unregister(Hero);
        HeroParty.Unregister(SecondHero);
    }

    private void GiveStartItems(HeroCard target)
    {
        foreach (var card in extraEquipment)
            if (card != null && !target.Equip(card))
                Debug.LogWarning($"CharacterSheetTester: '{card.title}' cannot be used by this hero class.");

        foreach (var artifact in extraArtifacts)
        {
            if (artifact == null) continue;
            bool ok = artifact.slot == ArtifactSlot.Consumable ? target.AddArtifact(artifact) : target.EquipArtifact(artifact);
            if (!ok) Debug.LogWarning($"CharacterSheetTester: '{artifact.title}' cannot be used by this hero class.");
        }

        foreach (var potion in extraPotions)
            if (potion != null && !target.AddPotion(potion))
                Debug.LogWarning($"CharacterSheetTester: '{potion.title}' is not a valid potion for this hero.");
    }

    [ContextMenu("Test: Switch shown hero")]
    public void SwitchHero()
    {
        if (SecondHero == null)
        {
            Debug.LogWarning("CharacterSheetTester: assign a Second Hero Template first.", this);
            return;
        }
        Shown = Shown == Hero ? SecondHero : Hero;
        sheet.HideDetails();
        sheet.Bind(Shown);
    }

    [ContextMenu("Test: Lose 1 Body")]   public void LoseBody()   { if (Shown != null) Shown.ModifyBodyPoints(-1); }
    [ContextMenu("Test: Gain 1 Body")]   public void GainBody()   { if (Shown != null) Shown.ModifyBodyPoints(1); }
    [ContextMenu("Test: Lose 1 Mind")]   public void LoseMind()   { if (Shown != null) Shown.ModifyMindPoints(-1); }
    [ContextMenu("Test: Gain 1 Mind")]   public void GainMind()   { if (Shown != null) Shown.ModifyMindPoints(1); }
    [ContextMenu("Test: Restore Body and Mind")] public void Restore() { if (Shown != null) Shown.RestoreBodyAndMind(); }

    [ContextMenu("Test: +10 Gold")]      public void AddGold()    { if (Shown != null) Shown.AddGold(10); }
    [ContextMenu("Test: Spend 5 Gold")]  public void SpendGold()  { if (Shown != null) Shown.TrySpendGold(5); }

    [ContextMenu("Test: Attack dice +1")]  public void AttackUp()   { if (Shown != null) Shown.ModifyAttackDice(1); }
    [ContextMenu("Test: Attack dice -1")]  public void AttackDown() { if (Shown != null) Shown.ModifyAttackDice(-1); }
    [ContextMenu("Test: Defend dice +1")]  public void DefendUp()   { if (Shown != null) Shown.ModifyDefenseDice(1); }
    [ContextMenu("Test: Defend dice -1")]  public void DefendDown() { if (Shown != null) Shown.ModifyDefenseDice(-1); }
    [ContextMenu("Test: Reset dice modifiers")] public void ResetDice() { if (Shown != null) Shown.ResetDiceModifiers(); }

    [ContextMenu("Test: Complete next quest")]
    public void CompleteNextQuest()
    {
        if (Shown == null) return;
        for (int q = 1; q <= HeroCard.TotalQuests; q++)
            if (!Shown.IsQuestCompleted(q)) { Shown.CompleteQuest(q); return; }
    }
}
