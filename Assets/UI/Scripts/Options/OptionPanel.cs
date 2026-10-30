using UnityEngine;

public class OptionPanel : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] private GameObject OptionsPanel;

    private bool isToggled = false;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void TogglePanel()
    {
        isToggled = !isToggled;
        OptionsPanel.SetActive(isToggled);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
