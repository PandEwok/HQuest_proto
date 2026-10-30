using UnityEngine;
using UnityEngine.UI;

public class ClickToggleAnimator : MonoBehaviour
{
    [SerializeField] private Button toggleButton;
    [SerializeField] private Animator animator;
    [SerializeField] private string triggerName = "Toggle";

    private int triggerHash;

    private void Awake()
    {
        triggerHash = Animator.StringToHash(triggerName);
        toggleButton.onClick.AddListener(OnClick);
    }

    private void OnDestroy()
    {
        toggleButton.onClick.RemoveListener(OnClick);
    }

    private void OnClick()
    {
        animator.SetTrigger(triggerHash);
    }
}