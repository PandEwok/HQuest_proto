using UnityEngine;
using UnityEngine.UIElements;

public class GameUI : MonoBehaviour
{
    [SerializeField] private GridManager gridManager;
    [SerializeField] private GameManager gameManager;

    private Button spellButton;
    private Button sTreasureButton;
    private Button disarmButton;


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
        spellButton = root.Q<Button>("Spell");

        spellButton.clicked += Spell;
    }

    void Spell()
    {
        gameManager.actionPerformed = true;
    }
}
