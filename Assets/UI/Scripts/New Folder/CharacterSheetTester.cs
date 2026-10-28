using UnityEngine;

/// <summary>
/// TEST ONLY - put this on any GameObject in your scene to try the character sheet
/// without the creation screen. Delete it when your friends' creation system is connected.
///
/// How to use: fill the fields in the Inspector, press Play, then open the three-dots menu
/// (or right-click) on this component's header: the "Test: ..." commands change the hero live.
/// The public methods can also be wired to normal UI Buttons.
/// </summary>
public class CharacterSheetTester : MonoBehaviour
{
    [SerializeField] private CharacterSheetPanel sheet;
    [SerializeField] private HeroCard heroTemplate;   // a HeroCard ASSET from your project
    [SerializeField] private string heroName = "Test Hero";
    [SerializeField, TextArea] private string heroDescription = "A brave adventurer.";
    [Tooltip("Extra items given at start (weapons, armor, potions...). Class restrictions still apply.")]
    [SerializeField] private EquipmentCard[] extraEquipment;

    public HeroCard Hero { get; private set; }

    private void Start()
    {
        if (sheet == null || heroTemplate == null)
        {
            Debug.LogError("CharacterSheetTester: assign the Sheet and the Hero Template.", this);
            return;
        }

        // The runtime copy is the player's own hero; the asset is never modified.
        Hero = HeroCard.CreateRuntimeCopy(heroTemplate, heroName, heroDescription);

        foreach (var card in extraEquipment)
            if (card != null && !Hero.Equip(card))
                Debug.LogWarning($"CharacterSheetTester: '{card.title}' cannot be used by this hero class.");

        sheet.Bind(Hero);
    }

    [ContextMenu("Test: Lose 1 Body")]   public void LoseBody()   { if (Hero != null) Hero.ModifyBodyPoints(-1); }
    [ContextMenu("Test: Gain 1 Body")]   public void GainBody()   { if (Hero != null) Hero.ModifyBodyPoints(1); }
    [ContextMenu("Test: Lose 1 Mind")]   public void LoseMind()   { if (Hero != null) Hero.ModifyMindPoints(-1); }
    [ContextMenu("Test: Gain 1 Mind")]   public void GainMind()   { if (Hero != null) Hero.ModifyMindPoints(1); }
    [ContextMenu("Test: Restore Body and Mind")] public void Restore() { if (Hero != null) Hero.RestoreBodyAndMind(); }

    [ContextMenu("Test: +10 Gold")]      public void AddGold()    { if (Hero != null) Hero.AddGold(10); }
    [ContextMenu("Test: Spend 5 Gold")]  public void SpendGold()  { if (Hero != null) Hero.TrySpendGold(5); }

    [ContextMenu("Test: Attack dice +1")]  public void AttackUp()   { if (Hero != null) Hero.ModifyAttackDice(1); }
    [ContextMenu("Test: Attack dice -1")]  public void AttackDown() { if (Hero != null) Hero.ModifyAttackDice(-1); }
    [ContextMenu("Test: Defend dice +1")]  public void DefendUp()   { if (Hero != null) Hero.ModifyDefenseDice(1); }
    [ContextMenu("Test: Defend dice -1")]  public void DefendDown() { if (Hero != null) Hero.ModifyDefenseDice(-1); }
    [ContextMenu("Test: Reset dice modifiers")] public void ResetDice() { if (Hero != null) Hero.ResetDiceModifiers(); }

    [ContextMenu("Test: Complete next quest")]
    public void CompleteNextQuest()
    {
        if (Hero == null) return;
        for (int q = 1; q <= HeroCard.TotalQuests; q++)
            if (!Hero.IsQuestCompleted(q)) { Hero.CompleteQuest(q); return; }
    }
}
