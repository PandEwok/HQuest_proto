using UnityEngine;
using System.Collections.Generic;

public class FallingBlockTrap : ITrap
{
    public string TrapName => "Falling-Block Trap";
    public bool IsDiscovered { get; set; } = false;
    public bool IsSprung { get; set; } = false;

    public bool CanBeDisarmed => !IsSprung;
    public bool CanBeJumped => !IsSprung;

    private CombatDiceManager diceManager;
    private bool blockPlaced = false;

    public GameObject Owner { get; set; }

    public GameObject BlockPrefab { get; set; }

    public FallingBlockTrap(CombatDiceManager diceManager, GameObject owner = null, GameObject blockPrefab = null)
    {
        this.diceManager = diceManager;
        Owner = owner;
        BlockPrefab = blockPrefab;
    }

    public void OnDiscovered(Character hero)
    {
        IsDiscovered = true;
        Debug.Log("Zargon: 'The ceiling looks dangerous.' (Pointe la case du piège)");
    }

    public void OnSpring(Character hero)
    {
        if (IsSprung) return;
        IsSprung = true;
        IsDiscovered = true;
        Debug.Log("Zargon: \"You've just sprung a falling-block trap! The ceiling is caving in! Look out!\"");

        var gameManager = UnityEngine.Object.FindFirstObjectByType<GameManager>();
        if (gameManager == null || hero == null)
        {
            Debug.LogWarning("FallingBlockTrap: GameManager ou Character introuvable.");
            PlaceBlock();
            return;
        }

        int skullsRolled = RollSkulls(3);
        Debug.Log($"FallingBlockTrap: {skullsRolled} crâne(s) obtenu(s) sur 3 dés.");

        if (skullsRolled > 0)
        {
            HeroCard card = gameManager.GetCardFromCharacter(hero);
            if (card != null)
            {
                card.ModifyBodyPoints(-skullsRolled);
                Debug.Log($"Le plafond s'écroule et {hero.name} perd {skullsRolled} Point(s) de Corps.");
            }
            else
            {
                Debug.LogWarning("FallingBlockTrap: HeroCard introuvable, dégâts non appliqués.");
            }
        }
        else
        {
            Debug.Log($"{hero.name} évite les débris : aucun dégât.");
        }

        hero.InterruptMovement(() =>
        {
            Debug.Log("Le héros peut avancer ou reculer d'une case (sans dé), ou terminer son tour.");
            gameManager.GrantBonusMove(hero, 1, PlaceBlock);
        });
    }

    private int RollSkulls(int diceCount)
    {
        int skulls = 0;
        if (diceManager != null)
        {
            diceManager.ResolveCombat(true, diceCount, 0, out List<CombatDiceManager.DieFace> rolls, out _);
            foreach (var face in rolls)
            {
                if (face == CombatDiceManager.DieFace.Skull) skulls++;
            }
        }
        else
        {
            Debug.LogWarning("FallingBlockTrap: CombatDiceManager manquant, tirage aléatoire de secours.");
            skulls = Random.Range(0, diceCount + 1);
        }
        return skulls;
    }

    private void PlaceBlock()
    {
        if (blockPlaced) return;
        blockPlaced = true;

        Debug.Log($"FallingBlockTrap.PlaceBlock: Owner={(Owner != null)}, BlockPrefab={(BlockPrefab != null ? BlockPrefab.name : "NULL")}");

        if (Owner != null)
        {
            if (BlockPrefab != null)
            {
                var block = UnityEngine.Object.Instantiate(BlockPrefab, Owner.transform.position, Owner.transform.rotation);
                block.SetActive(true);
                Debug.Log($"Bloc instancié : {block.name}, pos={block.transform.position}, scale={block.transform.lossyScale}", block);
            }
            UnityEngine.Object.Destroy(Owner);
        }
    }

    public void Disarm(Character hero)
    {
        if (!CanBeDisarmed) return;
        Debug.Log($"{hero.name} désamorce le piège de bloc tombant.");
    }

    public bool Jump(Character hero)
    {
        if (!CanBeJumped)
        {
            Debug.Log("Impossible de sauter : le piège est déjà déclenché et bloque le passage.");
            return false;
        }
        Debug.Log($"{hero.name} tente de sauter par-dessus le piège à bloc.");
        return true;
    }
}