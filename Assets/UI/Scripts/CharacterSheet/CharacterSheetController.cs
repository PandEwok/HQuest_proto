using UnityEngine;

public class CharacterSheetController : MonoBehaviour
{
    [SerializeField] private CharacterSheetPanel sheetPanel;
    [SerializeField] private GameManager gameManager;

    private void OnEnable()
    {
        BindCurrentHero();
    }

    /// <summary>
    /// Met à jour la fiche de personnage avec les données du joueur en cours.
    /// </summary>
    public void BindCurrentHero()
    {
        if (sheetPanel == null) return;

        // Si vous jouez en tour par tour avec GameManager :
        if (gameManager != null)
        {
            HeroCard currentHero = gameManager.GetCurrentHeroCard();
            if (currentHero != null)
            {
                sheetPanel.Bind(currentHero);
                return;
            }
        }

        // Alternative : Si un seul héros actif est défini dans GameSession
        if (GameSession.ActiveHero != null)
        {
            sheetPanel.Bind(GameSession.ActiveHero);
        }
    }
}