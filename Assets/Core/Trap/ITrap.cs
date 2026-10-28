using UnityEngine;

public interface ITrap
{
    string TrapName { get; }
    bool IsDiscovered { get; set; }
    bool IsSprung { get; set; }

    /// <summary>
    /// Appelé lorsqu'un héros cherche les pièges et découvre celui-ci.
    /// Zargon prévient et pointe la case.
    /// </summary>
    void OnDiscovered(Character hero);

    /// <summary>
    /// Appelé lorsqu'un personnage marche sur la case du piège sans l'avoir désamorcé/évité.
    /// </summary>
    void OnSpring(Character hero);

    /// <summary>
    /// Permet de tenter de désamorcer le piège (sur son tour, si découvert).
    /// </summary>
    bool CanBeDisarmed { get; }
    void Disarm(Character hero);

    /// <summary>
    /// Permet de tenter de sauter par-dessus le piège (si déjà découvert ou ouvert).
    /// </summary>
    bool CanBeJumped { get; }
    bool Jump(Character hero);
}