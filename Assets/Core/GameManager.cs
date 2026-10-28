using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class GameManager : MonoBehaviour
{
    int heroTurns = 0;
    int currentTurn = 0;

    bool endOfTurn = false;

    public GameObject playerNameTxt;

    [HideInInspector] public bool moved = false;
    [HideInInspector] public bool actionPerformed = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        heroTurns = GameSession.SelectedHeroes.Count;
        Debug.Log(heroTurns + " Heroes will play.");
        Turn();
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
