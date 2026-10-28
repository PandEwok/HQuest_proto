using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Save / Load menu (UI Toolkit, built entirely in code).
/// Place it on a GameObject that also has a UIDocument.
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(UIDocument))]
public class SaveLoadMenu : MonoBehaviour
{
    /// <summary>Number of "native" slots (gold style).</summary>
    public const int NativeSlotCount = 14;

    // ------------------------------------------------------------------ statique
    public static SaveLoadMenu Instance { get; private set; }

    /// <summary>The menu is open.</summary>
    public static bool IsOpen => Instance != null && Instance.isOpen;

    /// <summary>
    /// True if the menu is open OR the mouse is over a UI element (open button...).
    /// Check it in any script that reacts to the mouse in the world (grid, camera...).
    /// </summary>
    public static bool IsBlockingInput => Instance != null && Instance.ComputeBlocking();

    // ------------------------------------------------------------------ inspecteur
    [Header("References")]
    [SerializeField] private GridManager grid;
    [Tooltip("Ideally a copy of PS_SelChar (Scale With Screen Size, 1920x1080, match 0.5).")]
    [SerializeField] private PanelSettings panelSettings;
    [SerializeField] private StyleSheet styleSheet;
    [SerializeField] private int sortingOrder = 100;

    [Header("Display")]
    [Tooltip("GameObjects hidden while the menu is open, shown again when it closes.")]
    [SerializeField] private GameObject[] hideWhileOpen;

    [Header("Slots")]
    [Tooltip("Additional slots beyond the 14 native ones (shown in blue). Use the buttons below.")]
    [Min(0)][SerializeField] private int extraSlots = 0;
    [Tooltip("Shows an \"+ Add a slot\" card at the end of the list in the menu.")]
    [SerializeField] private bool allowAddSlotInMenu = false;

    // ------------------------------------------------------------------ state
    private enum MenuTab { Save, Load }
    private enum Pending { None, Overwrite, Delete }
    private enum MessageKind { Info, Ok, Warning, Error }

    private class SlotView
    {
        public int index;
        public bool isExtra;
        public bool exists;
        public MapSaveData data;
        public VisualElement root;
        public Label nameLabel;
        public Label metaLabel;
    }

    private UIDocument doc;
    private VisualElement root, overlay;
    private Button toggleButton, tabSave, tabLoad, btnClose, btnDelete, btnAction;
    private TextField nameField;
    private VisualElement nameRow;
    private Label subtitle, message, counter;
    private ScrollView scroll;

    private readonly List<SlotView> slotViews = new List<SlotView>();
    private int selected = -1;
    private MenuTab tab = MenuTab.Save;
    private Pending pending = Pending.None;
    private bool isOpen;
    private bool built;
    private bool[] hiddenWasActive;   // state of the objects before opening (null = nothing hidden)

    public int ExtraSlots => Mathf.Max(0, TotalSlots - NativeSlotCount);

    /// <summary>Native slots + extras (+ any existing files beyond them).</summary>
    public int TotalSlots
    {
        get
        {
            int total = NativeSlotCount + Mathf.Max(0, extraSlots);
            int highest = MapSaveSystem.HighestExistingSlot();
            if (highest + 1 > total) total = highest + 1;
            return total;
        }
    }

    // ------------------------------------------------------------------ lifecycle
#if UNITY_EDITOR
    private void Reset()
    {
        grid = FindFirstObjectByType<GridManager>();

        var guids = UnityEditor.AssetDatabase.FindAssets("SaveLoadMenu t:StyleSheet");
        if (guids.Length > 0)
            styleSheet = UnityEditor.AssetDatabase.LoadAssetAtPath<StyleSheet>(UnityEditor.AssetDatabase.GUIDToAssetPath(guids[0]));

        guids = UnityEditor.AssetDatabase.FindAssets("PS_SelChar t:PanelSettings");
        if (guids.Length > 0)
        {
            panelSettings = UnityEditor.AssetDatabase.LoadAssetAtPath<PanelSettings>(UnityEditor.AssetDatabase.GUIDToAssetPath(guids[0]));
            var d = GetComponent<UIDocument>();
            if (d != null && d.panelSettings == null) d.panelSettings = panelSettings;
        }
    }
#endif

    private void Awake()
    {
        doc = GetComponent<UIDocument>();
        if (doc.panelSettings == null)
        {
            doc.panelSettings = panelSettings != null ? panelSettings : CreateFallbackPanelSettings();
        }
        doc.sortingOrder = sortingOrder;
    }

    private void OnEnable()
    {
        Instance = this;
        // Wait one frame: the UIDocument must have created its rootVisualElement.
        StartCoroutine(BuildNextFrame());
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        if (Instance == this) Instance = null;
        RestoreHiddenObjects();
        built = false;
        isOpen = false;
    }

    private System.Collections.IEnumerator BuildNextFrame()
    {
        yield return null;
        Build();
    }

    private void Update()
    {
        if (isOpen && Input.GetKeyDown(KeyCode.Escape)) Close();
    }

    private static PanelSettings CreateFallbackPanelSettings()
    {
        var ps = ScriptableObject.CreateInstance<PanelSettings>();
        ps.scaleMode = PanelScaleMode.ScaleWithScreenSize;
        ps.referenceResolution = new Vector2Int(1920, 1080);
        ps.screenMatchMode = PanelScreenMatchMode.MatchWidthOrHeight;
        ps.match = 0.5f;
        return ps;
    }

    // ------------------------------------------------------------------ public API
    public void Open()
    {
        if (!built) return;
        isOpen = true;
        HideObjects();
        overlay.AddToClassList("sl-overlay--open");
        pending = Pending.None;
        SetTab(tab);
        RefreshSlots();
        ShowMessage("", MessageKind.Info);
    }

    public void Close()
    {
        if (!built) return;
        isOpen = false;
        pending = Pending.None;
        overlay.RemoveFromClassList("sl-overlay--open");
        RestoreHiddenObjects();
    }

    // ------------------------------------------------------------------ objects to hide
    private void HideObjects()
    {
        if (hideWhileOpen == null || hiddenWasActive != null) return; // already hidden

        hiddenWasActive = new bool[hideWhileOpen.Length];
        for (int i = 0; i < hideWhileOpen.Length; i++)
        {
            GameObject go = hideWhileOpen[i];
            if (go == null) continue;

            // safety: never disable the menu itself or one of its parents
            if (transform.IsChildOf(go.transform)) { Debug.LogWarning("[SaveLoadMenu] Cannot hide " + go.name + ": it contains the menu.", this); continue; }

            hiddenWasActive[i] = go.activeSelf;
            go.SetActive(false);
        }
    }

    private void RestoreHiddenObjects()
    {
        if (hideWhileOpen == null || hiddenWasActive == null) return;

        for (int i = 0; i < hideWhileOpen.Length && i < hiddenWasActive.Length; i++)
        {
            // only re-enable what was active before opening
            if (hideWhileOpen[i] != null && hiddenWasActive[i]) hideWhileOpen[i].SetActive(true);
        }
        hiddenWasActive = null;
    }

    public void Toggle()
    {
        if (isOpen) Close(); else Open();
    }

    /// <summary>Adds an "extra" slot (also usable at runtime).</summary>
    public void AddSlot()
    {
        extraSlots = TotalSlots - NativeSlotCount + 1;
        if (built) RefreshSlots();
    }

    // ------------------------------------------------------------------ input blocking
    private bool ComputeBlocking()
    {
        if (!built || root == null || root.panel == null) return false;
        if (isOpen) return true;

        Vector2 mouse = Input.mousePosition;
        Vector2 p = RuntimePanelUtils.ScreenToPanel(root.panel, new Vector2(mouse.x, Screen.height - mouse.y));
        return root.panel.Pick(p) != null;
    }

    // ------------------------------------------------------------------ UI construction
    private void Build()
    {
        root = doc.rootVisualElement;
        if (root == null)
        {
            Debug.LogError("[SaveLoadMenu] rootVisualElement not found: check the UIDocument's PanelSettings.", this);
            return;
        }

        if (grid == null) grid = FindFirstObjectByType<GridManager>();

        root.Clear();
        root.pickingMode = PickingMode.Ignore;
        root.style.position = Position.Absolute;
        root.style.left = 0; root.style.right = 0; root.style.top = 0; root.style.bottom = 0;

        if (styleSheet != null) root.styleSheets.Add(styleSheet);
        else Debug.LogWarning("[SaveLoadMenu] No StyleSheet assigned (SaveLoadMenu.uss).", this);

        var container = new VisualElement { name = "sl-container", pickingMode = PickingMode.Ignore };
        container.AddToClassList("sl-container");
        root.Add(container);

        // --- open button
        toggleButton = new Button(Toggle) { text = "SAVE / LOAD" };
        toggleButton.AddToClassList("sl-toggle");
        container.Add(toggleButton);

        // --- backdrop + window
        overlay = new VisualElement { name = "sl-overlay" };
        overlay.AddToClassList("sl-overlay");
        container.Add(overlay);

        var window = new VisualElement { name = "sl-window" };
        window.AddToClassList("sl-window");
        overlay.Add(window);

        // header
        var header = new VisualElement();
        header.AddToClassList("sl-header");
        header.Add(MakeLabel("SAVES", "sl-title"));
        subtitle = MakeLabel("", "sl-subtitle");
        header.Add(subtitle);
        window.Add(header);

        // tabs
        var tabs = new VisualElement();
        tabs.AddToClassList("sl-tabs");
        tabSave = new Button(() => SetTab(MenuTab.Save)) { text = "SAVE" };
        tabSave.AddToClassList("sl-tab");
        tabLoad = new Button(() => SetTab(MenuTab.Load)) { text = "LOAD" };
        tabLoad.AddToClassList("sl-tab");
        tabs.Add(tabSave);
        tabs.Add(tabLoad);
        window.Add(tabs);

        // map name
        nameRow = new VisualElement();
        nameRow.AddToClassList("sl-name-row");
        nameRow.Add(MakeLabel("Map name", "sl-name-label"));
        nameField = new TextField { maxLength = 40 };
        nameField.AddToClassList("sl-name-field");
        nameRow.Add(nameField);
        window.Add(nameRow);

        // slots
        scroll = new ScrollView(ScrollViewMode.Vertical) { name = "sl-slots" };
        scroll.AddToClassList("sl-slots");
        scroll.contentContainer.style.flexDirection = FlexDirection.Row;
        scroll.contentContainer.style.flexWrap = Wrap.Wrap;
        window.Add(scroll);

        // status message
        message = MakeLabel("", "sl-message");
        window.Add(message);

        // footer
        var footer = new VisualElement();
        footer.AddToClassList("sl-footer");
        counter = MakeLabel("", "sl-counter");
        footer.Add(counter);

        var buttons = new VisualElement();
        buttons.AddToClassList("sl-buttons");
        btnClose = new Button(Close) { text = "CLOSE" };
        btnClose.AddToClassList("sl-btn");
        btnDelete = new Button(OnDeleteClicked) { text = "DELETE" };
        btnDelete.AddToClassList("sl-btn");
        btnAction = new Button(OnActionClicked) { text = "SAVE" };
        btnAction.AddToClassList("sl-btn");
        btnAction.AddToClassList("sl-btn--primary");
        buttons.Add(btnClose);
        buttons.Add(btnDelete);
        buttons.Add(btnAction);
        footer.Add(buttons);
        window.Add(footer);

        built = true;
        SetTab(MenuTab.Save);
        RefreshSlots();
    }

    private static Label MakeLabel(string text, params string[] classes)
    {
        var l = new Label(text);
        foreach (string c in classes) l.AddToClassList(c);
        return l;
    }

    // ------------------------------------------------------------------ slot list
    private void RefreshSlots()
    {
        scroll.contentContainer.Clear();
        slotViews.Clear();

        int total = TotalSlots;
        for (int i = 0; i < total; i++)
        {
            var view = new SlotView
            {
                index = i,
                isExtra = i >= NativeSlotCount,
                exists = MapSaveSystem.Exists(i)
            };
            view.data = view.exists ? MapSaveSystem.Load(i) : null;

            view.root = new VisualElement();
            view.root.AddToClassList("sl-slot");
            if (view.isExtra) view.root.AddToClassList("sl-slot--extra");

            view.root.Add(MakeLabel((i + 1).ToString("00"), "sl-slot__index"));

            var info = new VisualElement();
            info.AddToClassList("sl-slot__info");
            view.nameLabel = MakeLabel("", "sl-slot__name");
            view.metaLabel = MakeLabel("", "sl-slot__meta");
            info.Add(view.nameLabel);
            info.Add(view.metaLabel);
            view.root.Add(info);

            view.root.Add(MakeLabel("EXTRA", "sl-slot__badge"));

            FillSlotTexts(view);

            int index = i; // copy for the closure
            view.root.RegisterCallback<ClickEvent>(_ => SelectSlot(index));

            scroll.contentContainer.Add(view.root);
            slotViews.Add(view);
        }

        if (allowAddSlotInMenu)
        {
            var add = new VisualElement();
            add.AddToClassList("sl-slot");
            add.AddToClassList("sl-slot--add");
            add.Add(MakeLabel("+ ADD A SLOT", "sl-slot__name"));
            add.RegisterCallback<ClickEvent>(_ =>
            {
                AddSlot();
                ShowMessage($"Slot {TotalSlots:00} added.", MessageKind.Ok);
            });
            scroll.contentContainer.Add(add);
        }

        if (selected >= slotViews.Count) selected = -1;
        UpdateSelectionVisuals();
        UpdateCounter();
        UpdateButtons();
    }

    private static void FillSlotTexts(SlotView v)
    {
        if (v.exists && v.data != null)
        {
            v.root.RemoveFromClassList("sl-slot--empty");
            v.nameLabel.text = string.IsNullOrWhiteSpace(v.data.name) ? $"Map {v.index + 1}" : v.data.name;
            v.metaLabel.text = $"{v.data.rooms.Count} room(s) · {v.data.doors.Count} door(s) · {v.data.width}x{v.data.height} · {v.data.SavedAtLocal:yyyy-MM-dd HH:mm}";
        }
        else if (v.exists)
        {
            v.root.AddToClassList("sl-slot--empty");
            v.nameLabel.text = "Unreadable file";
            v.metaLabel.text = "This save is corrupted. You can overwrite or delete it.";
        }
        else
        {
            v.root.AddToClassList("sl-slot--empty");
            v.nameLabel.text = "— Empty —";
            v.metaLabel.text = "No save";
        }
    }

    private void SelectSlot(int index)
    {
        if (index < 0 || index >= slotViews.Count) return;

        selected = index;
        pending = Pending.None;

        var v = slotViews[index];
        nameField.SetValueWithoutNotify(v.data != null && !string.IsNullOrWhiteSpace(v.data.name) ? v.data.name : $"Map {index + 1}");

        ShowMessage("", MessageKind.Info);
        UpdateSelectionVisuals();
        UpdateButtons();
    }

    private void UpdateSelectionVisuals()
    {
        foreach (var v in slotViews)
        {
            v.root.EnableInClassList("sl-slot--selected", v.index == selected);
        }
    }

    private void UpdateCounter()
    {
        int used = 0;
        foreach (var v in slotViews) if (v.exists) used++;
        counter.text = $"{used} / {slotViews.Count} slots used";
    }

    private void SetTab(MenuTab newTab)
    {
        tab = newTab;
        pending = Pending.None;

        tabSave.EnableInClassList("sl-tab--active", tab == MenuTab.Save);
        tabLoad.EnableInClassList("sl-tab--active", tab == MenuTab.Load);
        nameRow.EnableInClassList("sl-hidden", tab != MenuTab.Save);
        subtitle.text = tab == MenuTab.Save
            ? "Pick a slot to save the current map"
            : "Pick a save to load (the current map will be replaced)";

        ShowMessage("", MessageKind.Info);
        UpdateButtons();
    }

    private void UpdateButtons()
    {
        if (btnAction == null) return;

        SlotView v = (selected >= 0 && selected < slotViews.Count) ? slotViews[selected] : null;
        bool occupied = v != null && v.exists;

        if (tab == MenuTab.Save)
        {
            btnAction.text = pending == Pending.Overwrite ? "OVERWRITE?" : "SAVE";
            btnAction.SetEnabled(v != null && grid != null);
        }
        else
        {
            btnAction.text = "LOAD";
            btnAction.SetEnabled(v != null && v.data != null && grid != null);
        }
        btnAction.EnableInClassList("sl-btn--confirm", pending == Pending.Overwrite);

        btnDelete.text = pending == Pending.Delete ? "CONFIRM?" : "DELETE";
        btnDelete.SetEnabled(occupied);
        btnDelete.EnableInClassList("sl-btn--danger", pending == Pending.Delete);
    }

    // ------------------------------------------------------------------ actions
    private void OnActionClicked()
    {
        if (selected < 0 || selected >= slotViews.Count) return;
        var v = slotViews[selected];

        if (tab == MenuTab.Save)
        {
            if (v.exists && pending != Pending.Overwrite)
            {
                pending = Pending.Overwrite;
                ShowMessage($"Slot {selected + 1:00} already contains a map. Press again to overwrite it.", MessageKind.Warning);
                UpdateButtons();
                return;
            }
            DoSave(selected);
        }
        else
        {
            DoLoad(v);
        }
    }

    private void OnDeleteClicked()
    {
        if (selected < 0 || selected >= slotViews.Count || !slotViews[selected].exists) return;

        if (pending != Pending.Delete)
        {
            pending = Pending.Delete;
            ShowMessage($"Permanently delete slot {selected + 1:00}? Press again to confirm.", MessageKind.Warning);
            UpdateButtons();
            return;
        }

        int slot = selected;
        pending = Pending.None;
        if (MapSaveSystem.Delete(slot))
        {
            RefreshSlots();
            ShowMessage($"Slot {slot + 1:00} deleted.", MessageKind.Ok);
        }
        else
        {
            ShowMessage("Could not delete (see the console).", MessageKind.Error);
        }
    }

    private void DoSave(int slot)
    {
        if (grid == null) { ShowMessage("GridManager not found.", MessageKind.Error); return; }

        string mapName = nameField.value != null ? nameField.value.Trim() : "";
        if (mapName.Length == 0) mapName = $"Map {slot + 1}";

        MapSaveData data = grid.CaptureMap(mapName);
        pending = Pending.None;

        if (MapSaveSystem.TrySave(slot, data, out string error))
        {
            RefreshSlots();
            ShowMessage($"Map “{mapName}” saved to slot {slot + 1:00}.", MessageKind.Ok);
        }
        else
        {
            ShowMessage($"Save failed: {error}", MessageKind.Error);
            UpdateButtons();
        }
    }

    private void DoLoad(SlotView v)
    {
        if (v == null || v.data == null) { ShowMessage("This slot does not contain a valid save.", MessageKind.Error); return; }
        if (grid == null) { ShowMessage("GridManager not found.", MessageKind.Error); return; }

        if (grid.ApplyMap(v.data))
        {
            Close();
        }
        else
        {
            ShowMessage("Could not load.", MessageKind.Error);
        }
    }

    private void ShowMessage(string text, MessageKind kind)
    {
        if (message == null) return;
        message.text = text;
        message.EnableInClassList("sl-message--ok", kind == MessageKind.Ok);
        message.EnableInClassList("sl-message--warning", kind == MessageKind.Warning);
        message.EnableInClassList("sl-message--error", kind == MessageKind.Error);
    }
}