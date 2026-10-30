using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


[Serializable]
public class QuestObjective
{
    public ObjectiveType type;
    public string targetId;      // id d'entité ou de groupe visé
    public CardData card;        // objet à trouver (ex: Talisman, Spirit Blade)
    [TextArea] public string description;

    public string Describe()
    {
        string t = string.IsNullOrEmpty(description) ? type.ToString() : description;
        if (!string.IsNullOrEmpty(targetId)) t += $" [{targetId}]";
        if (card != null) t += $" [{card.title}]";
        return t;
    }
}

[Serializable]
public class KillBounty
{
    public MonsterCard monster;
    public int gold;
}

[Serializable]
public class QuestReward
{
    public RewardMode mode;
    public int gold;
    public string requiresNpcAliveId;   // ex: "sir_ragnar" : pas de récompense s'il meurt
    public KillBounty[] bounties;       // PerKillBounty (Q11)
    public CardData[] items;
    public string title;                // ex: "Champion"

    public string Describe()
    {
        var p = new List<string>();
        switch (mode)
        {
            case RewardMode.GoldSplit: p.Add($"{gold} gold coins, split among the heroes"); break;
            case RewardMode.GoldPerHero: p.Add($"{gold} gold coins for each hero"); break;
            case RewardMode.PerKillBounty: p.Add("Bounty per monster killed:"); break;
        }
        if (bounties != null) foreach (var b in bounties) if (b.monster != null) p.Add($"  - {b.monster.title}: {b.gold} gold");
        if (!string.IsNullOrEmpty(requiresNpcAliveId)) p.Add($"Lost if \"{requiresNpcAliveId}\" dies");
        if (items != null) foreach (var i in items) if (i != null) p.Add($"Item: {i.title}");
        if (!string.IsNullOrEmpty(title)) p.Add($"Title: {title}");
        return string.Join("\n", p);
    }
}

// ───────── Règles propres à une entité ─────────
public enum EntityRuleType
{
    ExtraAttackDice, ExtraDefenseDice, ImmuneToSpellGroup,
    InvulnerableUntilMovedOrAttacked, OnlyHarmedBy, CannotAttack, MovedByHero
}

[Serializable]
public class EntityRule
{
    public EntityRuleType type;
    public int value;
    public CardData item;        // OnlyHarmedBy (Spirit Blade)
    public SpellGroup group;     // ImmuneToSpellGroup (feu pour Balur)

    public string Describe()
    {
        switch (type)
        {
            case EntityRuleType.ExtraAttackDice: return $"+{value} attack die/dice";
            case EntityRuleType.ExtraDefenseDice: return $"+{value} defend die/dice";
            case EntityRuleType.ImmuneToSpellGroup: return $"Immune to \"{(group != null ? group.id : "?")}\" spells";
            case EntityRuleType.InvulnerableUntilMovedOrAttacked: return "Invulnerable until it has moved or been attacked";
            case EntityRuleType.OnlyHarmedBy: return $"Only \"{(item != null ? item.title : "?")}\" can harm it";
            case EntityRuleType.CannotAttack: return "Cannot attack";
            case EntityRuleType.MovedByHero: return $"Moved by a hero ({value} red die)";
        }
        return type.ToString();
    }
}

// Une entité = un monstre/PNJ "personnalisé" de la quête (Verag, Ulag, Sir Ragnar, Dread Warriors de pierre...).
// Plusieurs placements de la carte peuvent référencer la même entité (via entityId).
[Serializable]
public class QuestEntity
{
    public string id;                    // ex: "ulag"
    public string displayName;
    public MonsterCard baseMonster;      // modèle (stats et icône par défaut)
    public bool overrideStats;
    public int moveSquares, attackDice, defenseDice, hp, mind;
    public bool isNpc;                   // allié (Sir Ragnar)
    public bool startsDormant;           // pas sur la carte tant qu'il n'est pas activé (Witch Lord)
    public DreadSpellCard[] dreadSpells; // sorts connus (1 lancer par quête chacun)
    public CardData[] carriedItems;      // butin récupéré à sa mort
    public EntityRule[] rules;
    [TextArea] public string description;

    public string StatsText()
    {
        if (overrideStats || baseMonster == null)
            return $"Move {moveSquares} / Attack {attackDice} / Defend {defenseDice} / Body {hp} / Mind {mind}";
        return $"Move {baseMonster.moveSquares} / Attack {baseMonster.attackDice} / Defend {baseMonster.defenseDice} / Body {baseMonster.hp} / Mind {baseMonster.mind}";
    }

    public string Describe()
    {
        var sb = new StringBuilder($"{(string.IsNullOrEmpty(displayName) ? id : displayName)}{(isNpc ? " (NPC)" : "")} — {StatsText()}");
        if (startsDormant) sb.Append("\n  Dormant until activated");
        if (dreadSpells != null && dreadSpells.Length > 0) sb.Append("\n  Spells: " + string.Join(", ", dreadSpells.Where(s => s != null).Select(s => s.title)));
        if (rules != null) foreach (var r in rules) sb.Append("\n  • " + r.Describe());
        if (carriedItems != null) foreach (var c in carriedItems) if (c != null) sb.Append($"\n  Carries: {c.title}");
        if (!string.IsNullOrEmpty(description)) sb.Append("\n  " + description);
        return sb.ToString();
    }
}

// ───────── Notes de quête (A, B, C...) ─────────
[Serializable]
public class QuestNote
{
    public string key;                   // "A", "B"... (peut se répéter avec des déclencheurs différents)
    public QuestTrigger trigger;
    public string roomId;                // pièce concernée (voir rooms)
    public Vector2Int cell;              // ou case précise (OnMoveOntoCell)
    [TextArea] public string description;// texte de la note pour Zargon
    public string requiresDeadGroup;     // ex: "wardoz" : seulement une fois ce groupe/entité détruit
    public bool firstHeroOnly = true;    // "le premier héros qui fouille..."
    public bool trapped;                 // coffre/meuble piégé
    public QuestEffect[] effects;        // si pas piégé, ou après piège
    public QuestEffect[] onTrapSprung;   // fouille avant désarmorçage
    public QuestEffect[] onTrapDisarmed; // désarmorcé avant fouille

    public static string TriggerLabel(QuestTrigger t)
    {
        switch (t)
        {
            case QuestTrigger.OnQuestStart: return "Quest start";
            case QuestTrigger.OnSearchTreasure: return "Search for treasure";
            case QuestTrigger.OnSearchTraps: return "Search for traps";
            case QuestTrigger.OnSearchSecretDoors: return "Search for secret doors";
            case QuestTrigger.OnOpenDoor: return "Door opened";
            case QuestTrigger.OnEnterRoom: return "Room entered";
            case QuestTrigger.OnMoveOntoCell: return "Hero steps on square";
            case QuestTrigger.OnNpcFound: return "NPC found";
            case QuestTrigger.OnLastHeroPassed: return "Last hero has passed";
        }
        return t.ToString();
    }

    static void AddLines(StringBuilder sb, string header, QuestEffect[] list)
    {
        if (list == null || list.Length == 0) return;
        sb.Append("\n" + header);
        foreach (var e in list) sb.Append("\n  → " + e.Describe());
    }

    public string Describe()
    {
        var sb = new StringBuilder(description ?? "");
        if (!string.IsNullOrEmpty(requiresDeadGroup)) sb.Append($"\n(Once \"{requiresDeadGroup}\" is destroyed)");
        if (!string.IsNullOrEmpty(roomId)) sb.Append($"\nRoom: {roomId}");
        if (firstHeroOnly && trigger != QuestTrigger.OnQuestStart) sb.Append("\nFirst hero only");
        if (trapped) sb.Append("\nTRAPPED");
        AddLines(sb, trapped ? "If the trap is disarmed:" : "Effects:", trapped ? onTrapDisarmed : effects);
        if (trapped) { AddLines(sb, "If the trap is sprung:", onTrapSprung); AddLines(sb, "Then:", effects); }
        return sb.ToString().Trim();
    }
}

// ───────── Carte (données brutes, optionnelles) ─────────
public enum PlacementKind
{
    Monster, Door, SecretDoor, Furniture, Chest, Stairs,
    BlockedSquare, DoubleBlockedSquare, FallingBlockTrap, PitTrap, SpearTrap, Marker
}

[Serializable]
public class QuestRoom
{
    public string id;
    public RectInt area;
}

[Serializable]
public class MapPlacement
{
    public PlacementKind kind;
    public Vector2Int cell;          // (0,0) = coin haut-gauche
    public bool horizontal;          // portes / objets orientés
    public bool falseDoor;           // Q12
    public MonsterCard monster;      // kind = Monster
    public string entityId;          // entité de la quête (vide = monstre standard)
    public string subtype;           // "table", "throne", "bookcase"... pour Furniture
    public string markerKey;         // lettre de note (A, B...), "X", "XX" ou numéro de porte (Q10)
    public string groupId;           // pour cibler "tous les Dread Warriors", "les 3 coffres"...
}

// ───────── La quête ─────────
[CreateAssetMenu(menuName = "Quests/Quest")]
public class QuestData : ScriptableObject
{
    [Header("Map")]
    public TextAsset mapFile;

    [Header("Identity")]
    public string id;                // ex: "base.quest.01"
    public int number;
    public string title;
    public QuestData previousQuest;  // vide = toujours déverrouillée

    [Header("Textes")]
    [TextArea(3, 10)] public string parchmentText;   // lu à voix haute aux joueurs
    [TextArea(2, 6)] public string zargonNote;       // encadré réservé à Zargon

    [Header("Gameplay")]
    public QuestObjective[] objectives;
    public QuestReward reward;
    public MonsterCard wanderingMonster;
    public string wanderingMonsterText;              // texte de secours (ex: fantôme d'Ollar, Q10)
    public string startMarker;                       // vide = escalier
    public bool mustReturnToStairs = true;
    public CardData[] requiredItems;                 // objets à posséder au départ (Spirit Blade en Q14)
    public QuestRule[] specialRules;

    [Header("Entities and notes")]
    public QuestEntity[] entities;
    public QuestNote[] notes;

    [Header("Map (optional, to transcribe)")]
    public Vector2Int gridSize = new Vector2Int(26, 19);
    public RectInt[] solidRock;
    public QuestRoom[] rooms;
    public MapPlacement[] placements;

    public QuestEntity FindEntity(string entityId) =>
        entities == null ? null : entities.FirstOrDefault(e => e.id == entityId);

    public string WanderingMonsterName() =>
        wanderingMonster != null ? wanderingMonster.title : wanderingMonsterText;

    public string PlacementSummary() =>
        placements == null ? "" : string.Join("\n", placements.GroupBy(p => p.kind).Select(g => $"{g.Key} : {g.Count()}"));

    // ───── Accès rapides pour le gameplay ─────
    public QuestRule GetRule(QuestRuleType t) => specialRules?.FirstOrDefault(r => r.type == t);
    public bool HasRule(QuestRuleType t) => GetRule(t) != null;

    public IEnumerable<QuestNote> NotesFor(QuestTrigger trigger, string roomId = null) =>
        (notes ?? new QuestNote[0]).Where(n => n.trigger == trigger && (string.IsNullOrEmpty(roomId) || n.roomId == roomId));

    public IEnumerable<MapPlacement> PlacementsOf(PlacementKind kind) =>
        (placements ?? new MapPlacement[0]).Where(p => p.kind == kind);

    public QuestRoom RoomAt(Vector2Int cell) => rooms?.FirstOrDefault(r => r.area.Contains(cell));

    void OnValidate() { if (string.IsNullOrEmpty(id)) id = name; }
}
