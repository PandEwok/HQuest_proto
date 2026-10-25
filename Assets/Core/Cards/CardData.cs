using UnityEngine;

public abstract class CardData : ScriptableObject
{
    public string id;           
    public string title;
    [TextArea] public string description;
    public Sprite illustration;
}
