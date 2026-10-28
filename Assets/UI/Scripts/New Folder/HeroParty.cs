using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The list of heroes currently playing. The "Give" button uses it to know who can receive an item.
/// Whoever creates a hero (your friends' creation system, or GameSession) must call:
///     HeroParty.Register(hero);
/// right after HeroCard.CreateRuntimeCopy(...). Call Unregister(hero) if a hero leaves the game.
/// </summary>
public static class HeroParty
{
    private static readonly List<HeroCard> members = new List<HeroCard>();

    public static event Action Changed;
    public static IReadOnlyList<HeroCard> Members => members;

    public static void Register(HeroCard hero)
    {
        if (hero == null || members.Contains(hero)) return;
        members.Add(hero);
        Changed?.Invoke();
    }

    public static void Unregister(HeroCard hero)
    {
        if (hero != null && members.Remove(hero)) Changed?.Invoke();
    }

    public static void Clear()
    {
        members.Clear();
        Changed?.Invoke();
    }

    /// <summary>Every registered hero except the given one.</summary>
    public static List<HeroCard> OthersThan(HeroCard hero)
    {
        members.RemoveAll(h => h == null); // heroes destroyed since registration
        var result = new List<HeroCard>();
        foreach (var h in members)
            if (h != hero) result.Add(h);
        return result;
    }

    // Keeps the list clean when "Enter Play Mode Options" (no domain reload) is on in the editor.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetOnPlay() => members.Clear();
}
