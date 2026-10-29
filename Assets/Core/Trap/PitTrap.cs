using UnityEngine;

public class PitTrap : ITrap
{
    public string TrapName => "Pit Trap";
    public bool IsDiscovered { get; set; } = false;
    public bool IsSprung { get; set; } = false;

    public bool CanBeDisarmed => !IsSprung; // Une fois déclenché, un pit trap ne peut plus être désamorcé
    public bool CanBeJumped => true;

    public void OnDiscovered(Character hero)
    {
        IsDiscovered = true;
        Debug.Log("Zargon: 'The floor looks weak.' (Pointe la case de la fosse)");
    }

    public void OnSpring(Character hero)
    {
        if (IsSprung) return;
        IsSprung = true;
        Debug.Log("Zargon: 'You have just stumbled into a pit!'");

        var gameManager = UnityEngine.Object.FindFirstObjectByType<GameManager>();
        if (gameManager == null || hero == null)
        {
            Debug.LogWarning("PitTrap: GameManager ou Character introuvable.");
            return;
        }

        HeroCard card = gameManager.GetCardFromCharacter(hero);
        if (card != null)
        {
            card.ModifyBodyPoints(-1);
            card.ApplyPitPenalty();
        }
        hero.InterruptMovement(() => gameManager.TriggerEndTurnFor(hero));
        gameManager.TriggerEndTurn();
    }

    public void Disarm(Character hero)
    {
        if (!CanBeDisarmed)
        {
            Debug.Log("Ce piège a déjà été déclenché, il ne peut plus être désamorcé.");
            return;
        }

        Debug.Log($"{hero.name} désamorce le piège à fosse avec succès.");
        // Logique de désamorçage (ex: via un Tool Kit)
    }

    public bool Jump(Character hero)
    {
        Debug.Log($"{hero.name} tente de sauter par-dessus la fosse.");
        // Logique de saut (jet de dés de mouvement ou autre selon les règles de saut)
        return true;
    }
}