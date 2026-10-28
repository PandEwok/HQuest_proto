using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Cards/Hero Roster")]
public class HeroRoster : ScriptableObject
{
    public List<HeroCard> heroes = new();
}