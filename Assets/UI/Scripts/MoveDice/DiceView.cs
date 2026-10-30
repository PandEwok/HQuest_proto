using UnityEngine;
using TMPro;

public class DiceView : MonoBehaviour
{
    [SerializeField] private TMP_Text valueText;

    public void SetValue(int value)
    {
        valueText.text = value.ToString();
    }
}