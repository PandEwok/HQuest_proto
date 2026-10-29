using NUnit.Framework;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;


public class GameManager : MonoBehaviour
{
    [SerializeField] public GridManager gridManager;
    [SerializeField] private CharacterSheetController sheetController;

    int heroTurns = 0;
    int currentTurn = 0;

    bool endOfTurn = false;

    public GameObject playerNameTxt;

    [HideInInspector] public bool moved = false;
    [HideInInspector] public bool actionPerformed = false;

    [SerializeField] Character barbarian;
    [SerializeField] Character dwarf;
    [SerializeField] Character elf;
    [SerializeField] Character wizard;

    List<Character> heroCharacters = new List<Character>();

    [SerializeField] private GameObject CombatUI;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        heroTurns = GameSession.SelectedHeroes.Count;
        Debug.Log(heroTurns + " Heroes will play.");

        Vector2 gridSpawnPos = new Vector2(10,0);
        foreach (HeroCard heroCard in GameSession.SelectedHeroes)
        {

            Character spawnedCharacter = null;
            if (heroCard.title == "Dwarf")
            {
                spawnedCharacter = SpawnCharacter(dwarf, gridSpawnPos);
            }
            else if (heroCard.title == "Wizard")
            {
                spawnedCharacter = SpawnCharacter(wizard, gridSpawnPos);
            }
            else if (heroCard.title == "Elf")
            {
                spawnedCharacter = SpawnCharacter(elf, gridSpawnPos);
            }
            else if (heroCard.title == "Barbarian")
            {
                spawnedCharacter = SpawnCharacter(barbarian, gridSpawnPos);
            }
            if (spawnedCharacter != null)
            {
                spawnedCharacter.positionInGrid = gridSpawnPos;
                heroCharacters.Add(spawnedCharacter);
            }

            gridSpawnPos.x += 1;
        }

        Turn();
    }

    Character SpawnCharacter(Character character, Vector2 gridPosition) {
        Tile spawnTile;
        gridManager._tiles.TryGetValue(gridPosition, out spawnTile);
        Vector3 spawnWorldPos = spawnTile.surface.transform.position;

        Character spawnedCharacter = Instantiate(character, spawnWorldPos, Quaternion.identity, transform);
        return spawnedCharacter;
    }

    public void TriggerEndTurn()
    {
        CombatUI.GetComponent<TurnHandoverPopup>().ToggleHandOverPanel(true);
        endOfTurn = true;
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log(currentTurn);
        if (endOfTurn)
        {
            moved = false; actionPerformed = false;
            currentTurn += 1;
            if (currentTurn > heroTurns)
            {
                currentTurn = 0;
            }
            Turn();
            endOfTurn = false;
        }
        // playing turn
        else
        {
            if (currentTurn < heroTurns)
            {
                if (heroCharacters.Count > currentTurn)
                { heroCharacters[currentTurn].ShowMovementRange(3); }
            }
        }
    }

    void Turn()
    {
        // if hero's turn
        if (currentTurn < heroTurns)
        {
            HeroCard hero = GameSession.SelectedHeroes[currentTurn];
            if (playerNameTxt.GetComponent<TMP_Text>() != null)
            {
                playerNameTxt.GetComponent<TMP_Text>().text = hero.title + "'s Turn";
            }

            if (sheetController != null )
            {
                sheetController.BindCurrentHero();
            }

        } // else if Zargon's turn
        else
        {
            if (playerNameTxt.GetComponent<TMP_Text>() != null)
            {
                playerNameTxt.GetComponent<TMP_Text>().text = "Zargon's Turn";
            }
        }
    }

    // utility methods (get characters and cards)
    public HeroCard GetCardFromCharacter(Character character)
    {
        int index = heroCharacters.IndexOf(character);

        return GameSession.SelectedHeroes[index];
    }
    public Character GetCharacterFromCcard(HeroCard card)
    {
        int index = GameSession.SelectedHeroes.IndexOf(card);

        return heroCharacters[index];
    }
    public Character GetCurrentCharacter()
    {
        return heroCharacters[currentTurn];
    }
    public HeroCard GetCurrentHeroCard()
    {
        return GameSession.SelectedHeroes[currentTurn];
    }
}
