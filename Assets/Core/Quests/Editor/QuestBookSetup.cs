#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

// Menus :
//   Tools > QuestBook > Create the 14 core quests
//   Tools > QuestBook > Update texts and cards of the 14 core quests   (réécrit parchemin, notes, entités et liens vers les cartes)
public static partial class QuestBookSetup
{
    static readonly string[] Titles =
    {
        "The Trial", "The Rescue of Sir Ragnar", "Lair of the Orc Warlord", "Prince Magnus' Gold",
        "Melar's Maze", "Legacy of the Orc Warlord", "The Lost Wizard", "The Fire Mage",
        "Race Against Time", "Castle of Mystery", "Bastion of Dread", "Barak Tor—Barrow of the Witch Lord",
        "Quest for the Spirit Blade", "Return to Barak Tor"
    };

    static readonly string[] Wandering =
    {
        "Orc", "Orc", "Orc", "Abomination", "Zombie", "Abomination", "Mummy", "Abomination", "Abomination",
        "Ollar's ghost appears, chuckles wildly, and disappears", "Abomination", "Skeleton", "Dread Warrior", "Mummy"
    };

    // ───────── Textes du parchemin (lus à voix haute aux joueurs) ─────────
    static readonly string[] Parchment =
    {
        // 1
        "You have learned well, my friends. Now has come the time of your first trial. You must first enter the catacombs that contain Fellmarg's tomb. You must seek out and destroy Verag, a foul gargoyle that hides in the catacombs. This quest is not easy, and you must work together in order to survive. This is your first step on the road to becoming true heroes. Tread carefully, my friends.",
        // 2
        "Sir Ragnar, one of the King's most powerful knights, has been captured. There is reason to believe that he is being held prisoner by Ulag, the orc warlord. You are to find Sir Ragnar and bring him back to the stairway. Prince Magnus offers a reward of 240 gold coins, to be divided among the heroes, if they rescue Sir Ragnar. No reward is earned if Sir Ragnar is killed during the escape.",
        // 3
        "Prince Magnus has ordered that the orc warlord, Ulag, who was responsible for the imprisonment of Sir Ragnar, be sought out and destroyed. When Ulag is destroyed, the heroes are to be rewarded 180 gold coins to be divided among them. Any treasure found in Ulag's stronghold may be kept by the finder alone.",
        // 4
        "Three treasure chests have been stolen while being taken to the King. A reward of 240 gold coins has been offered to any group of heroes who return the chests and all of the gold. The thieves are a well-known band of orcs whose lair is in the Dark Mountains. They are led by Gulthor, a Dread warrior.",
        // 5
        "Long ago, a powerful wizard by the name of Melar created a Talisman of Lore that enhances the wearer's understanding of magic. It is rumored that Melar hid the talisman in an underground laboratory at the heart of his maze, fearing it might be stolen by the evil minions of Zargon. As you search for the talisman, beware of many traps and deadly monsters.",
        // 6
        "Ulag's foul offspring, Grak, has sworn revenge on those who killed his father. Although it has taken him several months, he has finally tracked you down and captured you in an ambush. Now you are all held prisoner in his dungeons while he decides when and how you will meet your fate. While the guard sleeps outside your cell, you manage to pick the lock with an old rat bone. You must find your equipment and escape to the stairway.",
        // 7
        "Wardoz, the King's personal wizard, has disappeared. His Majesty fears the wizard has been murdered or succumbed to the lures of Dread magic. You must find out what happened to Wardoz. You are each to be paid 100 gold coins, upon returning to the stairway.",
        // 8
        "The orcs of the Dark Mountains have been using Elemental fire magic in their raids. Balur, the fire mage, is thought to be responsible for helping them. No fire magic can harm Balur and the King's wizards are unable to counter his spells. You have therefore been chosen to enter his lair, deep beneath Darkfire Crag. His Majesty offers each hero a reward of 100 gold coins for Balur's destruction.",
        // 9
        "A guide has led you into an underground maze that is rumored to hold a great secret. He has led you down many dark corridors and finally you find yourself in a room with three doors. Suddenly the guide puts out his torch and in darkness you hear him laugh. 'Farewell, my heroes,' he taunts as he makes his escape. You realize to your horror that it is a trap! You must escape (make it back to the stairway) or perish in this dark, forgotten hole.",
        // 10
        "Long ago, a wizard named Ollar discovered the entrance to a gold mine. Using his great powers, he built a magic castle above the mine to protect it. The lower chamber of the castle has many magical doors and is guarded by a host of monsters who were trapped in time. Can you find the entrance to the gold mine? Others have tried, but the castle thwarted them every time.",
        // 11
        "Lands to the east have been plagued by marauding orcs and goblins who have allied themselves with Zargon. The King has ordered that a band of worthy heroes should be sent forth to destroy them. The orcs are well-protected in a strong underground fortress known as the Bastion of Dread. They are led by a small group of Dread warriors. You must fight your way in and kill all of the monsters you find. A hero earns a bounty of 10 gold coins for each goblin killed; 20 gold coins for each orc killed; 30 gold coins for each abomination killed; and 50 gold coins for each Dread warrior killed.",
        // 12
        "War with the eastern orcs is brewing and the King needs to unite the lands for the conflict to come. To do this, you must find the ancient Star of the West as worn by the Rulers of Legend in ages past. A reward of 200 gold coins is offered (to be divided evenly among all heroes) when the Star of the West is returned to safety. The Star of the West lies in Barak Tor, the resting place of the evil Witch Lord. He was defeated by the magical Spirit Blade long ago. This blade is the only weapon that could harm him.",
        // 13
        "You have awoken the Witch Lord! He poses a most serious threat to the King in the coming war. The Witch Lord must be destroyed before he can bring his army of undead to attack His Majesty's forces. Your goal in this quest is to find the Spirit Blade and return it to safety. Only this ancient weapon can harm the Witch Lord. The Spirit Blade was forged by the dwarves of the World End Mountain and cooled in the elven Fountain of Leben. The sword now lies somewhere in an ancient ruined temple.",
        // 14
        "Now that you have found the Spirit Blade, you must return to Barak Tor and defeat the Witch Lord. The King has ridden forth to meet the eastern orcs at Darkfire Pass. If you fail, the Witch Lord will lead his army of undead and attack His Majesty's forces from the rear. Then nothing remains to prevent the forces of Dread from overrunning the land!"
    };

    [MenuItem("Tools/QuestBook/Create the 14 core quests")]
    static void CreateCore()
    {
        ResetLookup();
        EnsureFolder("Assets", "Quests");
        EnsureFolder("Assets/Quests", "Core");
        const string dir = "Assets/Quests/Core";

        var quests = new QuestData[14];
        QuestData prev = null;
        for (int i = 0; i < 14; i++)
        {
            int n = i + 1;
            string path = $"{dir}/Quest{n:00}.asset";
            var q = AssetDatabase.LoadAssetAtPath<QuestData>(path);
            if (q == null)
            {
                q = ScriptableObject.CreateInstance<QuestData>();
                q.id = $"base.quest.{n:00}";
                q.number = n;
                q.title = Titles[i];
                q.wanderingMonsterText = Wandering[i];
                q.previousQuest = prev;
                ApplyPreset(q, n);
                ApplyTexts(q, n);
                LinkCards(q, n);
                AssetDatabase.CreateAsset(q, path);
            }
            quests[i] = q;
            prev = q;
        }

        var book = LoadOrCreate<QuestBook>($"{dir}/CoreBook.asset");
        book.id = "base.book.core";
        book.title = "HeroQuest — Core Game";
        book.quests = quests;
        EditorUtility.SetDirty(book);

        var lib = LoadOrCreate<QuestLibrary>("Assets/Quests/QuestLibrary.asset");
        lib.coreBook = book;
        EditorUtility.SetDirty(lib);

        AssetDatabase.SaveAssets();
        Debug.Log("QuestBook: 14 quests, core book and library created in Assets/Quests.");
    }

    // Pour les quêtes DÉJÀ créées : réécrit parchemin, note Zargon, notes et entités
    [MenuItem("Tools/QuestBook/Update texts and cards of the 14 core quests")]
    static void UpdateTexts()
    {
        const string dir = "Assets/Quests/Core";
        ResetLookup();
        int updated = 0;
        for (int n = 1; n <= 14; n++)
        {
            var q = AssetDatabase.LoadAssetAtPath<QuestData>($"{dir}/Quest{n:00}.asset");
            if (q == null) { Debug.LogWarning($"QuestBook: Quest{n:00} not found. Run \"Create the 14 core quests\" first."); continue; }
            ApplyTexts(q, n);
            LinkCards(q, n);
            EditorUtility.SetDirty(q);
            updated++;
        }
        AssetDatabase.SaveAssets();
        Debug.Log($"QuestBook: texts, notes and entities updated on {updated} quests.");
    }

    // Pré-remplit ce qui ne dépend d'aucune carte
    static void ApplyPreset(QuestData q, int n)
    {
        q.reward = new QuestReward();
        switch (n)
        {
            case 1:
                q.specialRules = new[] { Rule(QuestRuleType.NoTrapsNorSecretDoors, 0, "No traps or secret doors in this quest.") };
                break;
            case 2:
                q.reward.mode = RewardMode.GoldSplit; q.reward.gold = 240; q.reward.requiresNpcAliveId = "sir_ragnar";
                break;
            case 3:
                q.reward.mode = RewardMode.GoldSplit; q.reward.gold = 180;
                q.specialRules = new[] { Rule(QuestRuleType.TreasureKeptByFinder, 0, "Any treasure found in Ulag's stronghold may be kept by the finder alone.") };
                break;
            case 4:
                q.reward.mode = RewardMode.GoldSplit; q.reward.gold = 240;
                q.specialRules = new[] { Rule(QuestRuleType.CarryObjectLimit, 1, "A hero can carry only one chest at a time and rolls only 1 red die for movement.", 1) };
                break;
            case 6:
                q.startMarker = "Cell";
                q.specialRules = new[] { Rule(QuestRuleType.StartWithoutEquipment, 0, "Heroes have no equipment or spells until they search the cupboard (1 attack die, 2 defend dice).") };
                break;
            case 7:
            case 8:
                q.reward.mode = RewardMode.GoldPerHero; q.reward.gold = 100;
                break;
            case 9:
                q.startMarker = "A";
                break;
            case 10:
                q.specialRules = new[]
                {
                    Rule(QuestRuleType.DoorTeleportation, 1, "After passing a door: roll 2 red dice and teleport to the square with that number. Lose 1 Body Point if it is occupied. One door per turn."),
                    Rule(QuestRuleType.ExitOnDiceRoll, 0, "Exit to the stairs on a roll of 2 or 12.", 2, 12),
                    Rule(QuestRuleType.FoolsGold, 5000, "The mine gold is fool's gold (worthless); the carrier cannot attack or defend.")
                };
                break;
            case 11:
                q.reward.mode = RewardMode.PerKillBounty;
                break;
            case 12:
                q.reward.mode = RewardMode.GoldSplit; q.reward.gold = 200;
                q.specialRules = new[] { Rule(QuestRuleType.FalseDoors, 0, "Some doors are false and cannot be opened.") };
                break;
            case 13:
                q.specialRules = new[] { Rule(QuestRuleType.AltFallingBlock, 4, "Falling-block squares: roll 1 red die, lose 1 Body Point on 4-6 (helmet: on 6 only). Monsters are unaffected; no tiles are placed.", 6) };
                break;
            case 14:
                q.reward.title = "Champion";
                break;
        }
    }

    // ───────── Textes du Quest Book : parchemin, encadré Zargon, notes, entités ─────────
    static void ApplyTexts(QuestData q, int n)
    {
        q.parchmentText = Parchment[n - 1];
        q.zargonNote = "";
        q.notes = new QuestNote[0];
        q.entities = new QuestEntity[0];

        switch (n)
        {
            case 1:
                q.zargonNote = "Zargon, you may tell everyone that there are no traps or secret doors in this first quest. However, you should also warn them that this is a tough adventure. It is meant to show the players that their chances of survival depends on how well they work together.";
                q.notes = new[]
                {
                    N("A", QuestTrigger.OnSearchTreasure, "The weapons on this weapons rack are chipped, rusted, and broken. There is nothing here that the heroes would want.", Fx(QuestEffectType.NothingFound)),
                    N("B", QuestTrigger.OnSearchTreasure, "This treasure chest is empty.", Fx(QuestEffectType.NothingFound)),
                    N("C", QuestTrigger.OnQuestStart, "This mummy is the guardian of Fellmarg's tomb and was once a mighty warrior. It rolls 4 Attack dice instead of 3."),
                    N("D", QuestTrigger.OnSearchTreasure, "The first hero who searches for treasure finds 84 gold coins in this treasure chest.", Gold(84)),
                    N("E", QuestTrigger.OnSearchTreasure, "The first hero who searches for treasure finds 120 gold coins in this treasure chest.", Gold(120)),
                };
                break;

            case 2:
                q.notes = new[]
                {
                    Trap("A", "The treasure chest has a trap with a poison needle in it. If a hero searches for treasure before the trap is disarmed, they lose 1 Body Point. The chest is empty.", 1, Fx(QuestEffectType.NothingFound)),
                    N("B", QuestTrigger.OnSearchTreasure, "The first hero who searches the room for treasure finds 60 gold coins inside this chest. They also find a potion of healing, which restores up to 4 lost Body Points when consumed.", Gold(60)),
                    N("Finding Sir Ragnar", QuestTrigger.OnNpcFound, "When Sir Ragnar is found, an alarm sounds. Use the plastic Dread sorcerer figure to represent Sir Ragnar on the board. Place all remaining monsters, doors, and furniture on the board. All doors are now open. Sir Ragnar should be moved by the hero who opened the door to his cell. (Heroes cannot search for treasure in the cell.) That hero takes their regular turn first, then rolls 1 red die to move the wounded knight. Sir Ragnar must be moved back to the stairs alive in order for the heroes to collect the reward (see parchment text above). Sir Ragnar may not attack but may roll 2 Defend dice. He has 2 Body Points left.",
                        Fx(QuestEffectType.SoundAlarm), Target(QuestEffectType.ActivateEntity, "sir_ragnar"),
                        Fx(QuestEffectType.SpawnAllRemainingMonsters), Fx(QuestEffectType.OpenAllDoors)),
                };
                q.entities = new[]
                {
                    Entity("sir_ragnar", "Sir Ragnar", 0, 0, 2, 2, 0, true,
                        new EntityRule { type = EntityRuleType.CannotAttack },
                        new EntityRule { type = EntityRuleType.MovedByHero, value = 1 }),
                };
                break;

            case 3:
                q.notes = new[]
                {
                    N("A", QuestTrigger.OnSearchTreasure, "This is the orc's armory. The first hero who searches for treasure finds a staff weapon that they may add to their character sheet or may give to another hero. This staff is exactly like the one described in the armory."),
                    N("B", QuestTrigger.OnSearchTreasure, "The first hero who searches for treasure finds 24 gold coins and a potion of healing in the cupboard. The potion restores up to 4 lost Body Points when consumed.", Gold(24)),
                };
                q.entities = new[] { Entity("ulag", "Ulag", 10, 4, 5, 2, 3) };
                break;

            case 4:
                q.notes = new[]
                {
                    N("A", QuestTrigger.OnQuestStart, "These are the three chests. They are marked with the prince's royal seal. Each chest is locked and contains 250 gold coins and other items of value to Prince Magnus and the King. A hero can carry only one chest at a time. When carrying a treasure chest, a hero can only roll 1 red die for movement. The heroes cannot keep the gold inside the treasure chests."),
                };
                break;

            case 5:
                q.notes = new[]
                {
                    N("A", QuestTrigger.OnSearchTreasure, "The first hero to search for treasure finds a half-filled flask sitting on the alchemist's bench. It is a potion of healing that restores up to 2 lost Body Points when consumed."),
                    N("B", QuestTrigger.OnOpenDoor, "This chamber contains a gargoyle that appears to be a stone statue. The gargoyle does not move at first and only comes to life after one of the heroes opens the door that leads into the next room. The gargoyle cannot be harmed (takes no damage) until it has either moved or attacked a hero."),
                    Trap("C", "The chest is filled with poisonous gas—it is a trap! If a hero searches for treasure before the trap is disarmed, they lose 2 Body Points. The chest also contains 144 gold coins. There is no other treasure in this room.", 2, Gold(144)),
                    N("D", QuestTrigger.OnSearchTreasure, "The first hero who searches for treasure finds the Talisman of Lore. The talisman is a magical artifact. Its use is explained on the matching artifact card."),
                    N("E", QuestTrigger.OnSearchSecretDoors, "If a hero searches for secret doors, they find nothing on either side of the wall. However, if a hero searches for treasure they find Melar's key. Upon touching it, the key disappears and the throne slides sideways, revealing a secret door!"),
                };
                break;

            case 6:
                q.zargonNote = "Heroes begin their quest in the room marked \"Cell.\" Heroes cannot search for treasure in the cell. All of the heroes' equipment has been taken. They may not use any equipment (weapons, armor, potions, etc.) or cast any spells until they locate their equipment. A hero with no weapons or armor rolls 1 Attack die and 2 Defend dice.";
                q.notes = new[]
                {
                    N("A", QuestTrigger.OnSearchTreasure, "The heroes' equipment is stored in the cupboard in this room. When the room is searched for treasure, the equipment is found. Each remaining hero must then enter the room to collect their belongings. The heroes may again cast spells once they enter the room and reclaim their magical powers.", Fx(QuestEffectType.RestoreHeroEquipment)),
                    N("B", QuestTrigger.OnQuestStart, "These stairs lead out of the dungeon to freedom. Any hero who manages to move onto the stairway tile has escaped."),
                    N("Grak", QuestTrigger.OnQuestStart, "Use an orc figure for Grak. He is an extra tough-looking orc who holds a staff exactly like the one described in the armory. Grak also wears a Wizard's Cloak. Its use is explained on the matching artifact card. If Grak is killed, the cloak is of special interest to the wizard. Give them the matching artifact card.\nGrak knows 3 Dread spells that he may cast (one per turn) instead of attacking. The spells are: fear, sleep, and tempest. (Refer to the Dread spell cards for details.)"),
                };
                q.entities = new[] { Entity("grak", "Grak", 8, 4, 4, 3, 3) };
                break;

            case 7:
                q.notes = new[]
                {
                    N("A", QuestTrigger.OnQuestStart, "All the Dread warriors in this quest are made of stone and may roll 1 extra Defend dice."),
                    N("B", QuestTrigger.OnSearchTreasure, "This is the weapons room. The first hero who searches for treasure in this room finds the magical artifact Borin's Armor. Its use is explained on the matching artifact card."),
                    Trap("C", "This chest has a trap with a poison needle in it. If a hero searches for treasure before the trap is disarmed, they lose 2 Body Points. They then find a flask filled with an unidentifiable purple liquid. Do not tell the hero what the liquid is at this time. If they drink the liquid, it tastes terrible and causes the hero to temporarily turn into a stone statue. They are unable to move for five of their turns. However, during that time they cannot be harmed. After five of their turns, they come back to life.", 2),
                    Dead(N("D", QuestTrigger.OnSearchTreasure, "Wardoz's treasure chest contains 144 gold coins. The zombie in this room is dressed in wizard's robes. It is Wardoz. After destroying Wardoz, the first hero who searches for treasure finds the coins. They also find some papers on Wardoz's bench. These papers prove that Wardoz was consumed by the Dread magic he was experimenting with and was turned into a mindless zombie.", Gold(144)), "wardoz"),
                };
                break;

            case 8:
                q.notes = new[]
                {
                    N("Balur", QuestTrigger.OnQuestStart, "Use the Dread sorcerer figure for Balur. Fire spells do not affect him.\nBalur has the following spells, which he may use once during the quest: ball of flame, firestorm, tempest, summon orcs, fear, and escape. (Refer to the Dread spell cards for details.) The escape spell teleports him to the square marked \"XX\" in the middle room. If Balur uses the escape spell, do not put him onto that square until the heroes open the door."),
                    N("A", QuestTrigger.OnSearchTreasure, "The treasure chest contains 150 gold coins and the artifact called the Wand of Magic. Its use is explained on the matching artifact card.", Gold(150)),
                };
                q.entities = new[] { Entity("balur", "Balur", 8, 2, 5, 3, 7) };
                break;

            case 9:
                q.notes = new[]
                {
                    N("A", QuestTrigger.OnQuestStart, "This is the room where the heroes begin their quest."),
                    N("B", QuestTrigger.OnSearchTreasure, "These treasure chests each contain 100 gold coins.", Gold(100)),
                    Trap("C", "This chest has a trap with poisonous gas. If a hero searches for treasure before the trap is disarmed, they lose 3 Body Points. Inside the chest is the artifact called the Elixir of Life. Its use is explained on the matching artifact card.", 3),
                };
                break;

            case 10:
                q.zargonNote = "As soon as a hero moves through any door, they must stop immediately and roll 2 red dice. They are then teleported to the square with the same number as the dice total. If that square is already occupied, they land on the hero or monster in the square. The hero or monster that is landed on loses 1 Body point, and, if still alive, must roll 2 red dice to see where they are teleported. If the same square number is rolled, roll again. The first teleported hero remains on the square. Heroes may only pass through one door per turn!\n\nThe quest is over when all of the monsters are killed or when all of the heroes have left the quest by returning to the stairs on a roll of 2 or 12. When the game is over, tell the players that all the gold found at the mine entrance is fool's gold and is worthless. Any other treasure that is found is real.";
                q.notes = new[]
                {
                    N("A", QuestTrigger.OnSearchTreasure, "If both of these Dread warriors are defeated, the first hero to search for treasure finds that one of the warriors was wearing the magical artifact called the Ring of Return. Its use is explained on the matching artifact card."),
                    N("B", QuestTrigger.OnEnterRoom, "This square is the entrance to the mine. Any player who enters this room may take 5,000 gold coins. However, as long as they are carrying the gold, they may not attack or defend themselves. If the hero puts the gold down, it disappears back into the mine."),
                };
                break;

            case 11:
                q.notes = new[]
                {
                    N("A", QuestTrigger.OnSearchTreasure, "This is the armory. The first hero to search for treasure finds a shield exactly like the one described in the armory. All the other weapons here are unusable."),
                    N("B", QuestTrigger.OnSearchTreasure, "The gargoyle appears to be a stone statue that does not move. The treasure chest is a trap. If a hero searches for treasure before the trap is disarmed, the gargoyle springs to life and immediately attacks. If a hero disarms the trap first, they discover (are told by you) what would have happened if they had searched for treasure. The gargoyle cannot be harmed until it has either moved or attacked a hero."),
                    N("C", QuestTrigger.OnQuestStart, "This Dread warrior has a magic sword. Whoever kills the Dread warrior may take the sword as a prize. The sword is the artifact known as Orc's Bane. Its use is explained on the matching artifact card."),
                };
                break;

            case 12:
                q.notes = new[]
                {
                    N("A", QuestTrigger.OnOpenDoor, "These are false doors and cannot be opened at all."),
                    N("B", QuestTrigger.OnQuestStart, "The Star of the West is in the zombie's hand."),
                    N("C", QuestTrigger.OnLastHeroPassed, "This is a special falling-block trap that collapses automatically after the last hero passes by it. It is not sprung by the heroes as they move onto the square. After it falls, it forever blocks that path back to the stairs."),
                    N("D", QuestTrigger.OnEnterRoom, "This is the tomb of the Witch Lord. The Witch Lord is released from his imprisonment when the first hero enters the room. Use the Dread sorcerer figure to represent the Witch Lord on the board. Read the text that follows to the players.",
                        new QuestEffect { type = QuestEffectType.ReadText, text = "You have broken the magic seal that kept the Witch Lord imprisoned. Now he has awoken, and you must run. Only the Spirit Blade can harm him." }),
                    N("E", QuestTrigger.OnSearchTreasure, "The first hero to search for treasure finds a magical staff hidden behind the bookcase. It is the artifact known as the Wizard's Staff. Its use is described on the matching artifact card."),
                    N("The Witch Lord", QuestTrigger.OnQuestStart, "The Witch Lord is not affected by any weapon or spell. Only the Spirit Blade can harm him. The heroes must try to recover the Spirit Blade in the next quest. The Witch Lord moves 1 space per turn and rolls 2 Attack dice. He also knows the following spells: summon undead, fear, command, and ball of flame. (Refer to the Dread spell cards for details.)"),
                };
                break;

            case 13:
                q.zargonNote = "In this quest, the rules for falling-block squares are different. Any player who moves onto one of these squares must roll 1 red die. If they roll a 4, 5, or 6, they lose 1 Body Point. If the player is equipped with a helmet, they only lose 1 Body Point on a roll of 6. Monsters are not affected by these squares. Do not place the falling-block tiles onto the gameboard. Heroes are not blocked by these squares in this quest.";
                q.notes = new[]
                {
                    N("A", QuestTrigger.OnSearchTreasure, "The first hero who searches for treasure finds the Spirit Blade. It is a magical artifact. Its use is explained on the matching artifact card."),
                    N("B", QuestTrigger.OnSearchTreasure, "The treasure chest contains 200 gold coins.", Gold(200)),
                };
                break;

            case 14:
                q.notes = new[]
                {
                    N("A", QuestTrigger.OnQuestStart, "The Witch Lord's tomb is now empty."),
                    N("The Witch Lord", QuestTrigger.OnQuestStart, "Use the Dread sorcerer figure for the Witch Lord. The Witch Lord is not affected by any weapon or spell other than the Spirit Blade. The Witch Lord has grown stronger! (See stats.) Also, he knows the following spells: summon undead, fear (which he can cast twice), ball of flame, command, and tempest. Refer to the Dread spell cards for details."),
                    Dead(N("If the Witch Lord is defeated", QuestTrigger.OnQuestStart, "If the Witch Lord is defeated, he vanishes in a puff of foul, black smoke. When the smoke clears, the heroes hear the tinkle of metal skittering across the floor. On the floor, they find an artifact—the magical Spell Ring—that the Witch Lord was wearing. Its use is explained on the matching artifact card.\nAfter this quest is successfully completed, all surviving heroes are awarded the title of \"Champion\" by the King."), "witch_lord"),
                };
                q.entities = new[] { Entity("witch_lord", "The Witch Lord", 10, 5, 6, 4, 6) };
                break;
        }
    }

    // ───────── Petits utilitaires de construction ─────────
    static QuestNote N(string key, QuestTrigger trigger, string text, params QuestEffect[] effects) =>
        new QuestNote { key = key, trigger = trigger, description = text, effects = effects.Length > 0 ? effects : null };

    // Coffre piégé : fouille avant désarmorçage = dégâts, puis effets normaux
    static QuestNote Trap(string key, string text, int damage, params QuestEffect[] effects) =>
        new QuestNote
        {
            key = key,
            trigger = QuestTrigger.OnSearchTreasure,
            description = text,
            trapped = true,
            onTrapSprung = new[] { new QuestEffect { type = QuestEffectType.DamageHero, value = damage } },
            effects = effects.Length > 0 ? effects : null
        };

    static QuestNote Dead(QuestNote note, string group) { note.requiresDeadGroup = group; return note; }

    static QuestEffect Gold(int amount) => new QuestEffect { type = QuestEffectType.GiveGold, value = amount };
    static QuestEffect Fx(QuestEffectType t) => new QuestEffect { type = t };
    static QuestEffect Target(QuestEffectType t, string targetId) => new QuestEffect { type = t, targetId = targetId };

    static QuestEntity Entity(string id, string name, int move, int attack, int defend, int body, int mind, bool npc = false, params EntityRule[] rules) =>
        new QuestEntity
        {
            id = id,
            displayName = name,
            overrideStats = true,
            moveSquares = move,
            attackDice = attack,
            defenseDice = defend,
            hp = body,
            mind = mind,
            isNpc = npc,
            rules = rules.Length > 0 ? rules : null
        };

    static QuestRule Rule(QuestRuleType t, int value, string desc, params int[] extra) =>
        new QuestRule { type = t, value = value, description = desc, extraValues = extra };

    static T LoadOrCreate<T>(string path) where T : ScriptableObject
    {
        var a = AssetDatabase.LoadAssetAtPath<T>(path);
        if (a == null) { a = ScriptableObject.CreateInstance<T>(); AssetDatabase.CreateAsset(a, path); }
        return a;
    }

    static void EnsureFolder(string parent, string name)
    {
        if (!AssetDatabase.IsValidFolder($"{parent}/{name}")) AssetDatabase.CreateFolder(parent, name);
    }
}
#endif