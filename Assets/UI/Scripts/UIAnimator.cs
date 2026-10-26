using UnityEngine;

public class UIMenuToggler : MonoBehaviour
{
    [SerializeField] private Animator menuAnimator;
    private bool isOpen = false;

    // Link this function to your UI Button's OnClick event!
    public void ToggleMenu()
    {
        isOpen = !isOpen; // Flip the state
        menuAnimator.SetBool("IsOpen", isOpen);
    }
}