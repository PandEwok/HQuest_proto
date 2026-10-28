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

    private Button roomButton;
    private Button doorButton;
    private Button trapButton;
    private Button rockButton;
    private Button WRVButton;
    private Button WRHButton;
    private Button CupboardVButton;
    private Button CupboardHButton;
    private Button TableVButton;
    private Button TableHButton;
    private Button ChestButton;

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

        // ADD SELECTIONS
        roomButton = root.Q<Button>("PlaceRoom");

        roomButton.clicked += RoomSelection;

        doorButton = root.Q<Button>("PlaceDoor");

        doorButton.clicked += DoorSelection;

        rockButton = root.Q<Button>("PlaceRock");

        rockButton.clicked += RockSelection;

        trapButton = root.Q<Button>("PlaceTrap");

        trapButton.clicked += TrapSelection;

        WRVButton = root.Q<Button>("PlaceWeaponRackVertical");

        WRVButton.clicked += WRVSelection;

        WRHButton = root.Q<Button>("PlaceWeaponRackHorizontal");

        WRHButton.clicked += WRHSelection;

        CupboardVButton = root.Q<Button>("PlaceCupboardVertical");

        CupboardVButton.clicked += CupboardVSelection;

        CupboardHButton = root.Q<Button>("PlaceCupboardHorizontal");

        CupboardHButton.clicked += CupboardHSelection;

        TableVButton = root.Q<Button>("PlaceTableVertical");

        TableVButton.clicked += TableVSelection;

        TableHButton = root.Q<Button>("PlaceTableHorizontal");

        TableHButton.clicked += TableHSelection;

        ChestButton = root.Q<Button>("PlaceChest");

        ChestButton.clicked += ChestSelection;
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


    //SELECTIONS
    private void RoomSelection()
    {
        gridManager.currentSelection = GridManager.Selection.ROOM;
    }
    private void DoorSelection()
    {
        gridManager.currentSelection = GridManager.Selection.DOOR;
    }
    private void RockSelection()
    {
        gridManager.currentSelection = GridManager.Selection.ROCK;
    }
    private void TrapSelection()
    {
        gridManager.currentSelection = GridManager.Selection.TRAP;
    }
    private void WRVSelection()
    {
        gridManager.currentSelection = GridManager.Selection.WRV;
    }
    private void WRHSelection()
    {
        gridManager.currentSelection = GridManager.Selection.WRH;
    }
    private void CupboardVSelection()
    {
        gridManager.currentSelection = GridManager.Selection.CUPV;
    }
    private void CupboardHSelection()
    {
        gridManager.currentSelection = GridManager.Selection.CUPH;
    }
    private void TableVSelection()
    {
        gridManager.currentSelection = GridManager.Selection.TABLEV;
    }
    private void TableHSelection()
    {
        gridManager.currentSelection = GridManager.Selection.TABLEH;
    }
    private void ChestSelection()
    {
        gridManager.currentSelection = GridManager.Selection.CHEST;
    }
}
