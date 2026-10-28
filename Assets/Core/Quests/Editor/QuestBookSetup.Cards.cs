#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

// Relie les cartes existantes du projet (potions, artefacts, sorts, monstres...) aux 14 quêtes de base.
// Les cartes sont cherchées par titre, nom d'asset ou fin d'id (ex: "base.artifact.borins_armor" -> "Borin's Armor"),
// sans tenir compte des majuscules, espaces, apostrophes ni ponctuation.
// Une carte introuvable est ignorée (avertissement dans la Console) : crée-la, puis relance le menu de mise à jour.
public static partial class QuestBookSetup
{
    static readonly Dictionary<Type, List<ScriptableObject>> cache = new Dictionary<Type, List<ScriptableObject>>();
    static readonly HashSet<string> warned = new HashSet<string>();

    // Noms du livre -> noms possibles dans TON projet (le premier trouvé gagne)
    static readonly string[] SpiritBlade = { "Spirit Blade", "Mystic Sword" };
    static readonly string[] Talisman = { "Talisman of Lore", "Talisman of Knowledge" };
    static readonly string[] Wand = { "Wand of Magic", "Magic Wand" };
    static readonly string[] Potion = { "Potion of Healing", "Healing Potion" };
    static readonly string[] Potion2 = { "Potion of Healing (2)", "Potion of Healing 2", "Potion of Healing", "Healing Potion" };   // Q5 : 2 PV
    static readonly string[] OrcsBane = { "Orc's Bane", "Orc Slayer" };

    static void ResetLookup() { cache.Clear(); warned.Clear(); }

    // ───────── Recherche d'assets ─────────
    static string Norm(string s) =>
        string.IsNullOrEmpty(s) ? "" : new string(s.ToLowerInvariant().Where(char.IsLetterOrDigit).ToArray());

    static IEnumerable<string> Keys(ScriptableObject a)
    {
        yield return Norm(a.name);
        string id = null;
        if (a is CardData c) { yield return Norm(c.title); id = c.id; }
        else if (a is SpellGroup g) id = g.id;
        if (!string.IsNullOrEmpty(id)) yield return Norm(id.Split('.').Last());
    }

    // Plusieurs noms possibles : le premier trouvé gagne
    static T Find<T>(params string[] names) where T : ScriptableObject
    {
        if (!cache.TryGetValue(typeof(T), out var all))
        {
            all = AssetDatabase.FindAssets("t:" + typeof(T).Name)
                .Select(g => AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(g)))
                .Where(a => a != null).Cast<ScriptableObject>().ToList();
            cache[typeof(T)] = all;
        }
        foreach (var name in names)
        {
            string key = Norm(name);
            var hit = all.FirstOrDefault(a => Keys(a).Contains(key));
            if (hit != null) return (T)hit;
        }
        if (warned.Add(names[0]))
            Debug.LogWarning($"QuestBook: \"{names[0]}\" not found ({typeof(T).Name}), skipped.");
        return null;
    }

    // Noms des sorts dans le livre -> noms de TES cartes de sorts de Dread
    static readonly Dictionary<string, string[]> SpellAliases = new Dictionary<string, string[]>
    {
        { "Fear",          new[] { "Fear", "Horror" } },
        { "Tempest",       new[] { "Tempest", "Tornado" } },
        { "Ball of Flame", new[] { "Ball of Flame", "Fireball" } },
        { "Escape",        new[] { "Escape", "Teleportation" } },
        { "Command",       new[] { "Command", "Control" } },
    };

    static DreadSpellCard[] Spells(params string[] titles) =>
        titles.Select(t => Find<DreadSpellCard>(SpellAliases.TryGetValue(t, out var a) ? a : new[] { t }))
              .Where(x => x != null).ToArray();

    // ───────── Accès aux données déjà générées ─────────
    static QuestNote NoteOf(QuestData q, string key) => q.notes?.FirstOrDefault(n => n.key == key);
    static QuestEntity EntityOf(QuestData q, string id) => q.entities?.FirstOrDefault(e => e.id == id);

    // Ajoute un effet "GiveCard" à une note (si la note et la carte existent)
    static void Give(QuestData q, string noteKey, params string[] cardNames)
    {
        var note = NoteOf(q, noteKey);
        var card = Find<CardData>(cardNames);
        if (note == null || card == null) return;
        note.effects = (note.effects ?? new QuestEffect[0])
            .Append(new QuestEffect { type = QuestEffectType.GiveCard, card = card }).ToArray();
    }

    // Ajoute une entité basée sur une carte de monstre (ignorée si la carte n'existe pas)
    static void AddEntity(QuestData q, string id, string displayName, string monsterName)
    {
        var m = Find<MonsterCard>(monsterName);
        if (m == null) return;
        q.entities = (q.entities ?? new QuestEntity[0])
            .Append(new QuestEntity { id = id, displayName = displayName, baseMonster = m }).ToArray();
    }

    static void AddRule(QuestEntity e, EntityRule rule) =>
        e.rules = (e.rules ?? new EntityRule[0]).Append(rule).ToArray();

    static QuestObjective Obj(ObjectiveType type, string description, string target = null, CardData card = null) =>
        new QuestObjective { type = type, description = description, targetId = target, card = card };

    static KillBounty Bounty(string monsterName, int gold)
    {
        var m = Find<MonsterCard>(monsterName);
        return m == null ? null : new KillBounty { monster = m, gold = gold };
    }

    // ───────── Liaison quête par quête ─────────
    static void LinkCards(QuestData q, int n)
    {
        // Monstre errant (Q10 : texte seulement)
        if (n != 10)
        {
            var wm = Find<MonsterCard>(Wandering[n - 1]);
            if (wm != null) q.wanderingMonster = wm;
        }

        switch (n)
        {
            case 1:
                AddEntity(q, "verag", "Verag", "Gargoyle");
                q.objectives = new[] { Obj(ObjectiveType.KillEntity, "Seek out and destroy Verag", "verag") };
                break;

            case 2:
                Give(q, "B", Potion);
                q.objectives = new[] { Obj(ObjectiveType.RescueNpc, "Find Sir Ragnar and bring him back to the stairway alive", "sir_ragnar") };
                break;

            case 3:
                {
                    Give(q, "A", "Staff");
                    Give(q, "B", Potion);
                    var ulag = EntityOf(q, "ulag");
                    if (ulag != null) ulag.baseMonster = Find<MonsterCard>("Orc");
                    q.objectives = new[] { Obj(ObjectiveType.KillEntity, "Destroy Ulag, the orc warlord", "ulag") };
                    break;
                }

            case 4:
                AddEntity(q, "gulthor", "Gulthor", "Dread Warrior");
                q.objectives = new[] { Obj(ObjectiveType.CarryToStairs, "Bring the three treasure chests back to the stairway", "chests") };
                break;

            case 5:
                {
                    Give(q, "A", Potion2);
                    Give(q, "D", Talisman);
                    var talisman = Find<CardData>(Talisman);
                    q.objectives = new[] { Obj(ObjectiveType.FindCard, "Find the Talisman of Lore", null, talisman) };
                    break;
                }

            case 6:
                {
                    var grak = EntityOf(q, "grak");
                    if (grak != null)
                    {
                        grak.baseMonster = Find<MonsterCard>("Orc");
                        grak.dreadSpells = Spells("Fear", "Sleep", "Tempest");
                        var cloak = Find<CardData>("Wizard's Cloak");
                        if (cloak != null) grak.carriedItems = new[] { cloak };
                    }
                    q.objectives = new[] { Obj(ObjectiveType.Escape, "Find your equipment and escape to the stairway") };
                    break;
                }

            case 7:
                AddEntity(q, "wardoz", "Wardoz", "Zombie");
                Give(q, "B", "Borin's Armor");
                q.objectives = new[] { Obj(ObjectiveType.Custom, "Find out what happened to Wardoz and return to the stairway") };
                break;

            case 8:
                {
                    Give(q, "A", Wand);
                    var balur = EntityOf(q, "balur");
                    if (balur != null)
                    {
                        balur.dreadSpells = Spells("Ball of Flame", "Firestorm", "Tempest", "Summon Orcs", "Fear", "Escape");
                        var fire = Find<SpellGroup>("Fire");
                        if (fire != null) AddRule(balur, new EntityRule { type = EntityRuleType.ImmuneToSpellGroup, group = fire });
                    }
                    q.objectives = new[] { Obj(ObjectiveType.KillEntity, "Destroy Balur, the fire mage", "balur") };
                    break;
                }

            case 9:
                Give(q, "C", "Elixir of Life");
                q.objectives = new[] { Obj(ObjectiveType.Escape, "Escape the trap: make it back to the stairway") };
                break;

            case 10:
                Give(q, "A", "Ring of Return");
                q.objectives = new[] { Obj(ObjectiveType.Custom, "Kill all the monsters, or leave by the stairs on a roll of 2 or 12") };
                break;

            case 11:
                Give(q, "A", "Shield");
                Give(q, "C", OrcsBane);
                q.reward.bounties = new[] { Bounty("Goblin", 10), Bounty("Orc", 20), Bounty("Abomination", 30), Bounty("Dread Warrior", 50) }
                    .Where(b => b != null).ToArray();
                q.objectives = new[] { Obj(ObjectiveType.KillAllMonsters, "Fight your way in and kill all of the monsters you find") };
                break;

            case 12:
                Give(q, "E", "Wizard's Staff");
                q.objectives = new[] { Obj(ObjectiveType.Custom, "Find the Star of the West and return it to the stairway") };
                break;

            case 13:
                {
                    Give(q, "A", SpiritBlade);
                    var blade = Find<CardData>(SpiritBlade);
                    q.objectives = new[] { Obj(ObjectiveType.FindCard, "Find the Spirit Blade and return it to safety", null, blade) };
                    break;
                }

            case 14:
                {
                    var blade = Find<CardData>(SpiritBlade);
                    if (blade != null) q.requiredItems = new[] { blade };
                    Give(q, "If the Witch Lord is defeated", "Spell Ring");

                    var wl = EntityOf(q, "witch_lord");
                    if (wl != null)
                    {
                        wl.dreadSpells = Spells("Summon Undead", "Fear", "Fear", "Ball of Flame", "Command", "Tempest");   // Fear : 2 lancers
                        if (blade != null) AddRule(wl, new EntityRule { type = EntityRuleType.OnlyHarmedBy, item = blade });
                        var ring = Find<CardData>("Spell Ring");
                        if (ring != null) wl.carriedItems = new[] { ring };
                    }
                    q.objectives = new[] { Obj(ObjectiveType.KillEntity, "Defeat the Witch Lord with the Spirit Blade", "witch_lord") };
                    break;
                }
        }
    }
}
#endif