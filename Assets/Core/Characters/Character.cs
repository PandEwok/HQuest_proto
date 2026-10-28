using UnityEngine;

public class Character : MonoBehaviour
{
    [HideInInspector] Vector2 positionInGrid = new Vector2(0,0);

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Move(Vector2 newPos)
    {
        Vector3 worldPos = gridManager
    }
}
