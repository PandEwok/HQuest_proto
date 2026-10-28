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
        IsSprung = true;
        Debug.Log("Zargon: 'You have just stumbled into a pit!'");

        // Inflige 1 Point de Corps (Body Point)
        if (GameSession.ActiveHero != null)
        {
            GameSession.ActiveHero.ModifyBodyPoints(-1);
        }

        // Fin du tour du héros pour ce déplacement
        // TODO: Placer le tile visuel de fosse sous la figurine du héros
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