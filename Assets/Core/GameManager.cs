using NUnit.Framework;
using System.Collections.Generic;
using System.IO;
using TMPro;
using Unity.VisualScripting;
using UnityEditor.ShaderGraph.Serialization;
using UnityEngine;
using UnityEngine.UI;


public class GameManager : MonoBehaviour
{
    [SerializeField] public GridManager gridManager;
    [SerializeField] private CharacterSheetController sheetController;

    [SerializeField] List<TextAsset> QuestMaps;

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

    System.Random random;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (QuestMaps.Count > 0)
        {
            MapSaveData mapData = JsonUtility.FromJson<MapSaveData>(QuestMaps[0].text);
            //gridManager.ApplyMap(mapData);
            Debug.Log($"[Quest] v{mapData.version} walls:{mapData.walls.Count} props:{mapData.props.Count}");
            bool ok = gridManager.ApplyMap(mapData);
            Debug.Log($"[Quest] ApplyMap = {ok}");
        }

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
        random = new System.Random();
        Turn();
    }

    Character SpawnCharacter(Character character, Vector2 gridPosition) {
        Tile spawnTile;
        gridManager._tiles.TryGetValue(gridPosition, out spawnTile);
        Vector3 spawnWorldPos = spawnTile.surface.transform.position;

        Character spawnedCharacter = Instantiate(character, spawnWorldPos, Quaternion.identity, transform);
        return spawnedCharacter;
    }

    //To be sure that the end turn is triggered by the right player (not used for the button, only for the pit)
    public void TriggerEndTurnFor(Character requester)
    {
        if (currentTurn >= heroCharacters.Count || heroCharacters[currentTurn] != requester) return;
        TriggerEndTurn();
    }
    public void TriggerEndTurn()
    {
        if (endOfTurn) return; //Anti double call
        CombatUI.GetComponent<TurnHandoverPopup>().ToggleHandOverPanel(true);
        endOfTurn = true;
    }

    // Update is called once per frame
    void Update()
    {
        random = new System.Random();
        //Debug.Log(currentTurn);
        if (endOfTurn)
        {
            if (currentTurn < heroTurns)
                GameSession.SelectedHeroes[currentTurn].OnTurnEnded();
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
            if (currentTurn < heroTurns) // if an hero is playing
            {
                if (heroCharacters.Count > currentTurn)
                {
                    Character hero = GetCurrentCharacter();
                    hero.ClearMovementRange();
                    if (!moved)
                    { hero.ShowMovementRange(hero.movementRange); }

                    // LEFT CLICK BEGIN
                    if (Input.GetMouseButtonDown(0) && !moved)
                    {
                        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                        if (Physics.Raycast(ray, out RaycastHit hit))
                        {
                            Tile targetTile = hit.collider.GetComponentInParent<Tile>();
                            Tile tile;
                            gridManager._tiles.TryGetValue(hero.positionInGrid, out tile);
                            List<Tile> reachableTiles = hero.GetReachableTiles(tile, hero.movementRange);

                            if (targetTile != null && reachableTiles.Contains(targetTile))
                            {
                                List<Tile> path = hero.FindPath(tile, targetTile);
                                if (path != null)
                                {
                                    StartCoroutine(hero.MoveAlongPath(path));
                                    moved = true;
                                    hero.ClearMovementRange();
                                }
                            }
                        }
                    }
                }
            }
            else // else if Zargon is playing
            {

            }
        }
    }

    void Turn()
    {
        // if hero's turn
        if (currentTurn < heroTurns)
        {
            HeroCard hero = GameSession.SelectedHeroes[currentTurn];
            hero.OnTurnStarted();
            if (playerNameTxt.GetComponent<TMP_Text>() != null)
            {
                playerNameTxt.GetComponent<TMP_Text>().text = hero.title + "'s Turn";
            }

            if (sheetController != null )
            {
                sheetController.BindCurrentHero();
            }
            heroCharacters[currentTurn].movementRange = random.Next(1, 6) + random.Next(1, 6);


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
