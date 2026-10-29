using System;
using TMPro;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
using UnityEngine.UI;

public class TurnHandoverPopup : MonoBehaviour
{
    [SerializeField] private GameObject panel;      // full-screen opaque panel that hides the board
    [SerializeField] private TMP_Text messageText;  // "Gimli's turn"
    [SerializeField] private Button continueButton;
    [SerializeField] private Animator TurnHandOverPanelAnim;

    public static bool IsOpen { get; private set; } // friends can check this to ignore input
    private Action onContinue;

    private void Awake()
    {
        continueButton.onClick.AddListener(OnContinueClicked);
        panel.SetActive(false);
    }

    public void Show(string who, Action onContinue = null)
    {
        this.onContinue = onContinue;
        messageText.text = $"{who}'s turn";
        ToggleHandOverPanel(true);
        IsOpen = true;
    }

    private void OnContinueClicked()
    {
        ToggleHandOverPanel(false);
        IsOpen = false;
        onContinue?.Invoke();
    }

    private void ToggleHandOverPanel(bool menuOpen)
    {
        TurnHandOverPanelAnim.SetBool("IsOpen", menuOpen);
    }
}