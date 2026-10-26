using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class HeroQuestCombatUI : MonoBehaviour
{
    public enum Role { Hero, Enemy }
    public enum DiceFace { Skull, HeroShield, EnemyShield }

    [System.Serializable]
    public struct HeroClassSprite
    {
        public string className;
        public Sprite classSprite;
    }

    [Header("UI Containers & Prefabs")]
    [SerializeField] private GameObject diePrefab;
    [SerializeField] private Transform attackerDiceContainer;
    [SerializeField] private Transform defenderDiceContainer;

    [Header("UI Panels & Layouts (For Shrinking/Clashing)")]
    [SerializeField] private RectTransform attackerSidePanel;
    [SerializeField] private RectTransform defenderSidePanel;

    [Header("Text & Info Fields")]
    [SerializeField] private TextMeshProUGUI attackerTitleText;
    [SerializeField] private TextMeshProUGUI defenderTitleText;
    [SerializeField] private TextMeshProUGUI attackerScoreText;
    [SerializeField] private TextMeshProUGUI defenderScoreText;
    [SerializeField] private TextMeshProUGUI resultText;

    [Header("Icons & Sprites")]
    [SerializeField] private Image attackerRoleIcon;
    [SerializeField] private Image defenderRoleIcon;

    [Header("Hero Class Sprites")]
    [SerializeField] private List<HeroClassSprite> heroClassSprites = new List<HeroClassSprite>();
    [SerializeField] private Sprite defaultHeroIconSprite;

    [SerializeField] private Sprite enemyIconSprite;
    [SerializeField] private Sprite skullSprite;
    [SerializeField] private Sprite heroShieldSprite;
    [SerializeField] private Sprite enemyShieldSprite;

    [Header("Combat Configuration")]
    [SerializeField] private Role attackerRole = Role.Hero;
    [SerializeField] private Role defenderRole = Role.Enemy;

    [Header("Combat Identities")]
    [SerializeField] private string attackerHeroClass = "Barbarian";
    [SerializeField] private string defenderHeroClass = "";
    [SerializeField] private string specificEnemyName = "";

    [Header("Dice Quantities")]
    [SerializeField] private int attackDiceCount = 3;
    [SerializeField] private int defenseDiceCount = 2;

    [Header("Animation Timings & Pacing")]
    [SerializeField] private float rollDuration = 0.6f;
    [SerializeField] private float faceFlickerRate = 0.05f;

    [Tooltip("Time to let players inspect the dice values before the clashing animation starts")]
    [SerializeField] private float readDiceDelay = 1.0f;

    [Tooltip("Total duration of the clash movement (higher = slower movement)")]
    [SerializeField] private float clashDuration = 0.4f;

    [Tooltip("How far inward the panels travel toward each other during the clash")]
    [SerializeField] private float clashDistance = 120f;

    [Header("HQ Dice Ratios")]
    [SerializeField] private int skullFaces = 3;
    [SerializeField] private int heroShieldFaces = 2;
    [SerializeField] private int enemyShieldFaces = 1;

    private void Start()
    {
        if (resultText != null) resultText.text = "";
    }

    [ContextMenu("Run Full Combat Sequence")]
    public void StartCombatSequence()
    {
        StopAllCoroutines();
        StartCoroutine(CombatSequenceRoutine());
    }

    private IEnumerator CombatSequenceRoutine()
    {
        gameObject.SetActive(true);
        if (attackerSidePanel != null) { attackerSidePanel.localScale = Vector3.one; }
        if (defenderSidePanel != null) { defenderSidePanel.localScale = Vector3.one; }
        if (resultText != null)
        {
            resultText.text = "";
            resultText.GetComponent<RectTransform>().localScale = Vector3.zero;
        }

        SetupHeaderInfo();

        ClearContainer(attackerDiceContainer);
        ClearContainer(defenderDiceContainer);
        attackerScoreText.text = "";
        defenderScoreText.text = "";

        List<DiceFace> dicePool = new List<DiceFace>();
        for (int i = 0; i < skullFaces; i++) dicePool.Add(DiceFace.Skull);
        for (int i = 0; i < heroShieldFaces; i++) dicePool.Add(DiceFace.HeroShield);
        for (int i = 0; i < enemyShieldFaces; i++) dicePool.Add(DiceFace.EnemyShield);

        List<Image> attackerDieImages = CreateDiceUI(attackerDiceContainer, attackDiceCount);
        List<Image> defenderDieImages = CreateDiceUI(defenderDiceContainer, defenseDiceCount);

        // 1. Roll Animation (Flickering Sprites)
        float elapsedTime = 0f;
        while (elapsedTime < rollDuration)
        {
            SetRandomSprites(attackerDieImages);
            SetRandomSprites(defenderDieImages);
            elapsedTime += faceFlickerRate;
            yield return new WaitForSeconds(faceFlickerRate);
        }

        // 2. Resolve Final Results
        int skullsRolled = ResolveDiceGroup(attackerDieImages, attackDiceCount, dicePool);
        int blocksRolled = ResolveDiceGroup(defenderDieImages, defenseDiceCount, dicePool, defenderRole);

        // Update Text Tallies underneath
        attackerScoreText.text = $"Skulls: {skullsRolled}";
        defenderScoreText.text = $"Blocks: {blocksRolled}";

        // 3. Pause so players can read the dice before clashing
        yield return new WaitForSeconds(readDiceDelay);

        // 4. Clashing Visual Effect
        yield return StartCoroutine(PlayClashAnimation());

        // 5. Calculate Damage and Show Result Text
        int unblockedDamage = Mathf.Max(0, skullsRolled - blocksRolled);
        if (resultText != null)
        {
            resultText.text = unblockedDamage > 0
                ? $"{unblockedDamage} Unblocked Damage Dealt!"
                : "Attack Completely Blocked!";

            yield return StartCoroutine(PopUpResultText());
        }

        // 6. Wait and Clean Up
        yield return new WaitForSeconds(1.5f);
        yield return StartCoroutine(ShrinkAndCleanLayouts());
    }

    private void SetupHeaderInfo()
    {
        if (attackerRole == Role.Hero)
        {
            attackerTitleText.text = string.IsNullOrEmpty(attackerHeroClass) ? "Hero" : attackerHeroClass;
            if (attackerRoleIcon) attackerRoleIcon.sprite = GetHeroSprite(attackerHeroClass);
        }
        else
        {
            attackerTitleText.text = string.IsNullOrEmpty(specificEnemyName) ? "Enemy" : specificEnemyName;
            if (attackerRoleIcon) attackerRoleIcon.sprite = enemyIconSprite;
        }

        if (defenderRole == Role.Hero)
        {
            defenderTitleText.text = string.IsNullOrEmpty(defenderHeroClass) ? "Hero" : defenderHeroClass;
            if (defenderRoleIcon) defenderRoleIcon.sprite = GetHeroSprite(defenderHeroClass);
        }
        else
        {
            defenderTitleText.text = string.IsNullOrEmpty(specificEnemyName) ? "Enemy" : specificEnemyName;
            if (defenderRoleIcon) defenderRoleIcon.sprite = enemyIconSprite;
        }
    }

    private Sprite GetHeroSprite(string className)
    {
        if (string.IsNullOrEmpty(className)) return defaultHeroIconSprite;

        foreach (var entry in heroClassSprites)
        {
            if (entry.className.Equals(className, System.StringComparison.OrdinalIgnoreCase))
            {
                return entry.classSprite != null ? entry.classSprite : defaultHeroIconSprite;
            }
        }
        return defaultHeroIconSprite;
    }

    private IEnumerator PlayClashAnimation()
    {
        if (attackerSidePanel == null || defenderSidePanel == null) yield break;

        HorizontalLayoutGroup hLayout = attackerSidePanel.parent.GetComponent<HorizontalLayoutGroup>();
        VerticalLayoutGroup vLayout = attackerSidePanel.parent.GetComponent<VerticalLayoutGroup>();

        if (hLayout != null) hLayout.enabled = false;
        if (vLayout != null) vLayout.enabled = false;

        float halfTime = clashDuration * 0.5f;
        float elapsed = 0f;

        Vector2 originalAttackerPos = attackerSidePanel.anchoredPosition;
        Vector2 originalDefenderPos = defenderSidePanel.anchoredPosition;

        Vector2 attackerBumpPos = originalAttackerPos + new Vector2(clashDistance, 0f);
        Vector2 defenderBumpPos = originalDefenderPos + new Vector2(-clashDistance, 0f);

        // Phase 1: Slide inward toward center
        while (elapsed < halfTime)
        {
            float t = elapsed / halfTime;
            attackerSidePanel.anchoredPosition = Vector2.Lerp(originalAttackerPos, attackerBumpPos, t);
            defenderSidePanel.anchoredPosition = Vector2.Lerp(originalDefenderPos, defenderBumpPos, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Phase 2: Slide back out to original positions
        elapsed = 0f;
        while (elapsed < halfTime)
        {
            float t = elapsed / halfTime;
            attackerSidePanel.anchoredPosition = Vector2.Lerp(attackerBumpPos, originalAttackerPos, t);
            defenderSidePanel.anchoredPosition = Vector2.Lerp(defenderBumpPos, originalDefenderPos, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        attackerSidePanel.anchoredPosition = originalAttackerPos;
        defenderSidePanel.anchoredPosition = originalDefenderPos;

        if (hLayout != null) hLayout.enabled = true;
        if (vLayout != null) vLayout.enabled = true;
    }

    private IEnumerator PopUpResultText()
    {
        RectTransform resultRect = resultText.GetComponent<RectTransform>();
        float popDuration = 0.25f;
        float elapsed = 0f;

        resultRect.localScale = Vector3.zero;

        while (elapsed < popDuration)
        {
            float t = elapsed / popDuration;
            float scale = Mathf.Sin(t * Mathf.PI * 0.5f) * 1.25f;
            if (t > 0.6f) scale = Mathf.Lerp(1.25f, 1f, (t - 0.6f) / 0.4f);

            resultRect.localScale = new Vector3(scale, scale, 1f);
            elapsed += Time.deltaTime;
            yield return null;
        }

        resultRect.localScale = Vector3.one;
    }

    private IEnumerator ShrinkAndCleanLayouts()
    {
        float shrinkDuration = 0.4f;
        float elapsed = 0f;

        while (elapsed < shrinkDuration)
        {
            float progress = elapsed / shrinkDuration;
            float currentScale = Mathf.Lerp(1f, 0f, progress);

            if (attackerSidePanel) attackerSidePanel.localScale = new Vector3(currentScale, currentScale, 1f);
            if (defenderSidePanel) defenderSidePanel.localScale = new Vector3(currentScale, currentScale, 1f);
            if (resultText) resultText.GetComponent<RectTransform>().localScale = new Vector3(currentScale, currentScale, 1f);

            elapsed += Time.deltaTime;
            yield return null;
        }

        if (attackerSidePanel) attackerSidePanel.localScale = Vector3.zero;
        if (defenderSidePanel) defenderSidePanel.localScale = Vector3.zero;
        if (resultText) resultText.text = "";

        ClearContainer(attackerDiceContainer);
        ClearContainer(defenderDiceContainer);
    }

    private List<Image> CreateDiceUI(Transform container, int count)
    {
        List<Image> images = new List<Image>();
        for (int i = 0; i < count; i++)
        {
            GameObject newDie = Instantiate(diePrefab, container);
            Image img = newDie.transform.Find("Icon_Image") != null ?
                        newDie.transform.Find("Icon_Image").GetComponent<Image>() :
                        newDie.GetComponent<Image>();
            images.Add(img);
        }
        return images;
    }

    private void SetRandomSprites(List<Image> images)
    {
        foreach (Image img in images)
        {
            int rand = Random.Range(0, 3);
            if (rand == 0) img.sprite = skullSprite;
            else if (rand == 1) img.sprite = heroShieldSprite;
            else img.sprite = enemyShieldSprite;
        }
    }

    private int ResolveDiceGroup(List<Image> images, int diceCount, List<DiceFace> pool, Role? roleForDefense = null)
    {
        int score = 0;
        for (int i = 0; i < diceCount; i++)
        {
            DiceFace rolledFace = pool[Random.Range(0, pool.Count)];
            Image dieImage = images[i];

            // Reset color to fully opaque before applying evaluation tint
            dieImage.color = Color.white;

            switch (rolledFace)
            {
                case DiceFace.Skull:
                    dieImage.sprite = skullSprite;
                    if (roleForDefense == null)
                    {
                        score++; // Attacker scores on Skulls
                    }
                    else
                    {
                        dieImage.color = new Color(1f, 1f, 1f, 0.4f); // Dim Defender Skulls!
                    }
                    break;

                case DiceFace.HeroShield:
                    dieImage.sprite = heroShieldSprite;
                    if (roleForDefense == Role.Hero)
                    {
                        score++; // Hero defends on White Shields
                    }
                    else
                    {
                        dieImage.color = new Color(1f, 1f, 1f, 0.4f); // Dim unused shields
                    }
                    break;

                case DiceFace.EnemyShield:
                    dieImage.sprite = enemyShieldSprite;
                    if (roleForDefense == Role.Enemy)
                    {
                        score++; // Enemy defends on Black Shields
                    }
                    else
                    {
                        dieImage.color = new Color(1f, 1f, 1f, 0.4f); // Dim unused shields
                    }
                    break;
            }
        }
        return score;
    }

    private void ClearContainer(Transform container)
    {
        foreach (Transform child in container)
        {
            Destroy(child.gameObject);
        }
    }
}