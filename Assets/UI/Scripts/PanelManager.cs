using UnityEngine;
using UnityEngine.UI;

public class PanelManager : MonoBehaviour
{
    [System.Serializable]
    public class PanelEntry
    {
        public string name;                    // just a label in the Inspector
        public Button openButton;              // the button that opens this panel
        public Animator animator;              // the Animator that plays the open/close animation
        public string boolName = "IsOpen";     // the bool parameter in that Animator
        public bool visibleToHeroes = true;
        public bool visibleToZargon = true;
    }

    [SerializeField] private PanelEntry[] panels;

    private int openIndex = -1;   // -1 means no panel is open
    private bool isZargonTurn;

    private void Start() => Refresh();

    // Wired to each open button's On Click(), with the panel's index (0, 1, ...)
    // Wired to each panel's button On Click(), with the panel's index (0, 1, ...)
    public void Toggle(int index)
    {
        if (index < 0 || index >= panels.Length) return;

        if (openIndex == index)
        {
            // Same button pressed again: close its panel
            SetAnimation(index, false);
            openIndex = -1;
        }
        else
        {
            // Safety: never two panels at once
            if (openIndex >= 0) SetAnimation(openIndex, false);
            openIndex = index;
            SetAnimation(index, true);
        }
        Refresh();
    }

    private void Refresh()
    {
        for (int i = 0; i < panels.Length; i++)
        {
            var p = panels[i];
            if (p.openButton == null) continue;

            bool allowedNow = isZargonTurn ? p.visibleToZargon : p.visibleToHeroes;
            bool nothingOpen = openIndex == -1;
            bool isTheOpenOne = openIndex == i;

            // Visible if the player is allowed to see it, and either nothing is open
            // or this button belongs to the panel that is open (so it can close it).
            p.openButton.gameObject.SetActive(allowedNow && (nothingOpen || isTheOpenOne));
        }
    }

    // Wired to each panel's own close button
    public void CloseAll()
    {
        if (openIndex >= 0) SetAnimation(openIndex, false);
        openIndex = -1;
        Refresh();
    }

    // To be called when the turn changes
    public void SetTurn(bool zargonsTurn)
    {
        isZargonTurn = zargonsTurn;
        CloseAll(); // also stops the previous player's sheet staying open for the next one
    }

    private void SetAnimation(int index, bool open)
    {
        var p = panels[index];
        if (p.animator != null) p.animator.SetBool(p.boolName, open);
    }
}