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
}
