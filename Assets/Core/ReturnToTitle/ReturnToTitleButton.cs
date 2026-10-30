using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

/// <summary>
/// Bouton en haut à gauche qui renvoie à l'écran titre, après confirmation
/// (les données non sauvegardées ne peuvent pas être récupérées).
///
/// Mise en place (dans la scène de l'éditeur de niveaux) :
///  1. Créer un GameObject "ReturnToTitle" avec un UIDocument (Source Asset = ReturnToTitle.uxml,
///     Panel Settings = idéalement le MÊME asset que l'UI de l'éditeur).
///  2. Ajouter ce script dessus.
///  3. Renseigner "Title Scene Name" avec le nom exact de la scène titre
///     (présente dans la liste des scènes de la build).
///
/// Le script se place automatiquement au-dessus des autres UIDocument de la scène
/// (même panneau ou panneau différent) pour que le bouton reçoive bien les clics.
/// </summary>
[RequireComponent(typeof(UIDocument))]
public class ReturnToTitleButton : MonoBehaviour
{
    [Tooltip("Nom exact de la scène de l'écran titre (doit être dans la liste des scènes de la build).")]
    [SerializeField] string titleSceneName = "TitleScreen";

    [Tooltip("Ordre minimal d'affichage dans le panneau. Relevé automatiquement si un autre UIDocument du même panneau est au-dessus.")]
    [SerializeField] int sortingOrder = 100;

    [Tooltip("Affiche dans la console des infos de diagnostic (panneaux, survol, EventSystem).")]
    [SerializeField] bool debugLogs = true;

    UIDocument _doc;
    PanelSettings _panelClone;
    Button _homeButton;
    Button _cancelButton;
    Button _confirmButton;
    VisualElement _modal;
    bool _hoverLogged;

    void OnEnable()
    {
        _doc = GetComponent<UIDocument>();
        EnsureOnTop();

        var root = _doc.rootVisualElement;
        _homeButton = root.Q<Button>("rtt-button");
        _cancelButton = root.Q<Button>("rtt-cancel");
        _confirmButton = root.Q<Button>("rtt-confirm");
        _modal = root.Q<VisualElement>("rtt-modal");

        _homeButton.clicked += OpenConfirmation;
        _cancelButton.clicked += CloseConfirmation;
        _confirmButton.clicked += GoToTitle;
        _homeButton.RegisterCallback<PointerEnterEvent>(OnHover);

        CloseConfirmation();
    }

    void OnDisable()
    {
        if (_homeButton != null)
        {
            _homeButton.clicked -= OpenConfirmation;
            _homeButton.UnregisterCallback<PointerEnterEvent>(OnHover);
        }
        if (_cancelButton != null) _cancelButton.clicked -= CloseConfirmation;
        if (_confirmButton != null) _confirmButton.clicked -= GoToTitle;
    }

    void OnDestroy()
    {
        if (_panelClone != null) Destroy(_panelClone);
    }

    /// <summary>
    /// Place ce document devant tous les autres :
    ///  - panneau (PanelSettings) différent : l'ordre des panneaux vient de PanelSettings.sortingOrder,
    ///    pas de UIDocument.sortingOrder. On utilise une COPIE du PanelSettings (l'asset n'est pas modifié).
    ///  - même panneau : on passe au-dessus des autres UIDocument via UIDocument.sortingOrder.
    /// </summary>
    void EnsureOnTop()
    {
        var others = FindObjectsByType<UIDocument>(FindObjectsSortMode.None)
            .Where(d => d != _doc && d.isActiveAndEnabled)
            .ToList();

        var ps = _doc.panelSettings;
        if (ps == null)
        {
            Debug.LogError("ReturnToTitleButton : le UIDocument n'a pas de Panel Settings assigné.", this);
            return;
        }

        // 1) Autres panneaux plus hauts que le nôtre ?
        var otherPanels = others.Where(d => d.panelSettings != null && d.panelSettings != ps).ToList();
        if (otherPanels.Count > 0)
        {
            float maxOther = otherPanels.Max(d => d.panelSettings.sortingOrder);
            if (maxOther >= ps.sortingOrder)
            {
                _panelClone = Instantiate(ps);
                _panelClone.name = ps.name + " (ReturnToTitle)";
                _panelClone.sortingOrder = maxOther + 1;
                _doc.panelSettings = _panelClone;
                ps = _panelClone;
            }
        }

        // 2) Même panneau : au-dessus des autres documents
        int docOrder = sortingOrder;
        foreach (var d in others.Where(d => d.panelSettings == ps))
            docOrder = Mathf.Max(docOrder, Mathf.RoundToInt(d.sortingOrder) + 1);
        _doc.sortingOrder = docOrder;

        if (debugLogs)
        {
            Debug.Log($"ReturnToTitleButton : panneau '{ps.name}' (sort {ps.sortingOrder}), document (sort {_doc.sortingOrder}). " +
                      "Autres UIDocument : " +
                      (others.Count == 0
                          ? "aucun"
                          : string.Join(" | ", others.Select(d =>
                              $"{d.name} -> panneau '{(d.panelSettings != null ? d.panelSettings.name : "null")}' " +
                              $"(sort {(d.panelSettings != null ? d.panelSettings.sortingOrder : 0)}), doc sort {d.sortingOrder}"))),
                      this);

            if (FindFirstObjectByType<EventSystem>() == null)
                Debug.LogWarning("ReturnToTitleButton : aucun EventSystem dans la scène (nécessaire pour les clics UI Toolkit).", this);
        }
    }

    void OnHover(PointerEnterEvent _)
    {
        if (debugLogs && !_hoverLogged)
        {
            _hoverLogged = true;
            Debug.Log("ReturnToTitleButton : le survol est bien reçu par le bouton.", this);
        }
    }

    void OpenConfirmation() => _modal.style.display = DisplayStyle.Flex;

    void CloseConfirmation() => _modal.style.display = DisplayStyle.None;

    void GoToTitle()
    {
        CloseConfirmation();

        if (string.IsNullOrEmpty(titleSceneName) || !Application.CanStreamedLevelBeLoaded(titleSceneName))
        {
            Debug.LogError($"ReturnToTitleButton : la scène '{titleSceneName}' est introuvable. " +
                           "Vérifie son nom et sa présence dans la liste des scènes de la build.", this);
            return;
        }

        SceneManager.LoadScene(titleSceneName);
    }
}