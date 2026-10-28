using NUnit.Framework;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;


public class GameManager : MonoBehaviour
{
    [SerializeField] private GridManager gridManager;

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

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        heroTurns = GameSession.SelectedHeroes.Count;
        Debug.Log(heroTurns + " Heroes will play.");

        Vector2 gridSpawnPos = Vector2.zero;
        foreach (HeroCard heroCard in GameSession.SelectedHeroes)
        {

            Character spawnedCharacter;
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

    // Update is called once per frame
    void Update()
    {
        endOfTurn = moved && actionPerformed;

        if (endOfTurn)
        {
            currentTurn += 1;
            if (currentTurn > heroTurns)
            {
                currentTurn = 0;
                Turn();
                endOfTurn = false;
            }
        }
        // playing turn
        else
        {

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

        } // else if Zargon's turn
        else
        {
            if (playerNameTxt.GetComponent<TMP_Text>() != null)
            {
                playerNameTxt.GetComponent<TMP_Text>().text = "Zargon's Turn";
            }
        }
    }
}
