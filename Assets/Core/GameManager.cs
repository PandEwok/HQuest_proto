using UnityEngine;

public class GameManager : MonoBehaviour
{
    int heroTurns = 0;
    int currentTurn = 0;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        heroTurns = GameSession.SelectedHeroes.Count;
        Debug.Log(heroTurns);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
