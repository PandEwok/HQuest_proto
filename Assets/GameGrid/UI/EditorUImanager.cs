using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class EditorUImanager : MonoBehaviour
{
    [SerializeField] private GridManager gridManager;

    private Button addButton;
    private Button selectButton;
    private Button RemoveButton;
    private Button RotateButton;

    private Button roomButton;
    private Button doorButton;
    private Button SecretDoorButton;
    private Button spikeTrapButton;
    private Button pitTrapButton;
    private Button fallingBlockTrapButton;
    private Button rockButton;
    private Button WeaponRackButton;
    private Button CupboardButton;
    private Button TableButton;
    private Button ChestButton;
    private Button AlchemistBenchButton;
    private Button SorcererTableButton;
    private Button ThroneButton;
    private Button TombButton;
    private Button StairsButton;
    private Button FireplaceButton;

    public List<Placeable> placeableList = new List<Placeable>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnEnable()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;


        // EDIT MODES
        addButton = root.Q<Button>("AddButton");

        addButton.clicked += AddMode;

        selectButton = root.Q<Button>("SelectButton");

        selectButton.clicked += SelectMode;

        RemoveButton = root.Q<Button>("RemoveButton");

        RemoveButton.clicked += RemoveMode;

        RotateButton = root.Q<Button>("RotateButton");

        RotateButton.clicked += RotateMode;

        // ADD SELECTIONS
        roomButton = root.Q<Button>("PlaceRoom");

        roomButton.clicked += RoomSelection;

        doorButton = root.Q<Button>("PlaceDoor");

        doorButton.clicked += DoorSelection;

        SecretDoorButton = root.Q<Button>("PlaceSecretDoor");

        SecretDoorButton.clicked += SecretDoorSelection;

        rockButton = root.Q<Button>("PlaceRock");

        rockButton.clicked += RockSelection;

        spikeTrapButton = root.Q<Button>("PlaceSpikeTrap");

        spikeTrapButton.clicked += SpikeTrapSelection;

        pitTrapButton = root.Q<Button>("PlacePitTrap");

        pitTrapButton.clicked += PitTrapSelection;

        fallingBlockTrapButton = root.Q<Button>("PlaceFallingBlockTrap");

        fallingBlockTrapButton.clicked += FallingBlockTrapSelection;

        WeaponRackButton = root.Q<Button>("PlaceWeaponRack");

        WeaponRackButton.clicked += WeaponRackSelection;

        CupboardButton = root.Q<Button>("PlaceCupboard");

        CupboardButton.clicked += CupboardSelection;

        TableButton = root.Q<Button>("PlaceTable");

        TableButton.clicked += TableSelection;

        ChestButton = root.Q<Button>("PlaceChest");

        ChestButton.clicked += ChestSelection;

        SorcererTableButton = root.Q<Button>("PlaceSorcererTable");

        SorcererTableButton.clicked += SorcererTableSelection;

        AlchemistBenchButton = root.Q<Button>("PlaceAlchemistBench");

        AlchemistBenchButton.clicked += AlchemistBenchSelection;

        TombButton = root.Q<Button>("PlaceTomb");

        TombButton.clicked += TombSelection;

        ThroneButton = root.Q<Button>("PlaceThrone");

        ThroneButton.clicked += ThroneSelection;

        StairsButton = root.Q<Button>("PlaceStairs");

        StairsButton.clicked += StairsSelection;

        FireplaceButton = root.Q<Button>("PlaceFireplace");

        FireplaceButton.clicked += FireplaceSelection;
    }

    //MODES
    private void AddMode()
    {
        gridManager.currentMode = GridManager.Mode.ADD;
    }
    private void SelectMode()
    {
        gridManager.currentMode = GridManager.Mode.SELECT;
    }
    private void RemoveMode()
    {
        gridManager.currentMode = GridManager.Mode.REMOVE;
    }
    private void RotateMode()
    {
        gridManager.currentMode = GridManager.Mode.ROTATE;
    }


    //SELECTIONS
    private void RoomSelection()
    {
        gridManager.currentSelection = GridManager.Selection.ROOM;
    }
    private void DoorSelection()
    {
        gridManager.currentSelection = GridManager.Selection.DOOR;
    }
    private void SecretDoorSelection()
    {
        gridManager.currentSelection = GridManager.Selection.SCRTDOOR;
    }
    private void RockSelection()
    {
        gridManager.currentSelection = GridManager.Selection.ROCK;
    }
    private void SpikeTrapSelection()
    {
        gridManager.currentSelection = GridManager.Selection.SPKTRAP;
    }
    private void PitTrapSelection()
    {
        gridManager.currentSelection = GridManager.Selection.PITTRAP;
    }
    private void FallingBlockTrapSelection()
    {
        gridManager.currentSelection = GridManager.Selection.FLLTRAP;
    }
    private void WeaponRackSelection()
    {
        gridManager.currentSelection = GridManager.Selection.WR;
    }
    private void CupboardSelection()
    {
        gridManager.currentSelection = GridManager.Selection.CUP;
    }
    private void TableSelection()
    {
        gridManager.currentSelection = GridManager.Selection.TABLE;
    }
    private void ChestSelection()
    {
        gridManager.currentSelection = GridManager.Selection.CHEST;
    }
    private void AlchemistBenchSelection()
    {
        gridManager.currentSelection = GridManager.Selection.ALCH;
    }
    private void SorcererTableSelection()
    {
        gridManager.currentSelection = GridManager.Selection.SORC;
    }
    private void TombSelection()
    {
        gridManager.currentSelection = GridManager.Selection.TOMB;
    }
    private void ThroneSelection()
    {
        gridManager.currentSelection = GridManager.Selection.THRONE;
    }
    private void StairsSelection()
    {
        gridManager.currentSelection = GridManager.Selection.STAIRS;
    }
    private void FireplaceSelection()
    {
        gridManager.currentSelection = GridManager.Selection.FIRE;
    }
}
