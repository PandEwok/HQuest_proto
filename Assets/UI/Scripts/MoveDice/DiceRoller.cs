using System;
using System.Collections;
using UnityEngine;
using TMPro;

public class DiceRoller : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject dicePanel;       // parent of everything below
    [SerializeField] private CanvasGroup panelGroup;     // on the same panel, used for the fade
    [SerializeField] private DiceView dicePrefab;
    [SerializeField] private Transform diceContainer;    // with a Horizontal Layout Group
    [SerializeField] private TMP_Text rollingText;
    [SerializeField] private TMP_Text totalText;

    [Header("Text")]
    [Tooltip("{0} is replaced by the name of whoever is rolling")]
    [SerializeField] private string rollingMessageFormat = "{0} is rolling the movement dice...";

    [Header("Animation")]
    [SerializeField] private float rollDuration = 1.2f;
    [SerializeField] private float staggerPerDie = 0.25f;
    [SerializeField] private float flickerInterval = 0.06f;
    [SerializeField] private float holdAfterRoll = 1f;   // how long the result stays visible
    [SerializeField] private float fadeDuration = 0.5f;

    public bool IsRolling { get; private set; }
    public int LastTotal { get; private set; }

    private void Awake()
    {
        dicePanel.SetActive(false);
    }

    /// onFinished is called as soon as the dice stop, before the fade-out.
    public void Roll(int diceCount, string rollerName, Action<int> onFinished = null)
    {
        if (IsRolling || diceCount <= 0) return;
        StartCoroutine(RollRoutine(diceCount, rollerName, onFinished));
    }

    private IEnumerator RollRoutine(int diceCount, string rollerName, Action<int> onFinished)
    {
        IsRolling = true;

        foreach (Transform child in diceContainer)
            Destroy(child.gameObject);

        rollingText.text = string.Format(rollingMessageFormat, rollerName);
        totalText.text = "";               // result stays hidden until the dice stop
        panelGroup.alpha = 1f;
        dicePanel.SetActive(true);

        int[] results = new int[diceCount];
        DiceView[] views = new DiceView[diceCount];
        float[] stopTimes = new float[diceCount];
        bool[] isSettled = new bool[diceCount];
        int total = 0;

        for (int i = 0; i < diceCount; i++)
        {
            results[i] = UnityEngine.Random.Range(1, 7);
            total += results[i];
            views[i] = Instantiate(dicePrefab, diceContainer);
            stopTimes[i] = rollDuration + i * staggerPerDie;
        }

        float elapsed = 0f;
        int settled = 0;
        while (settled < diceCount)
        {
            for (int i = 0; i < diceCount; i++)
            {
                if (isSettled[i]) continue;

                if (elapsed >= stopTimes[i])
                {
                    views[i].SetValue(results[i]);
                    isSettled[i] = true;
                    settled++;
                }
                else
                {
                    views[i].SetValue(UnityEngine.Random.Range(1, 7));
                }
            }
            yield return new WaitForSeconds(flickerInterval);
            elapsed += flickerInterval;
        }

        // Animation done: show the result and hand it over
        LastTotal = total;
        totalText.text = total.ToString();
        onFinished?.Invoke(total);

        // Let the player read it, then fade everything out
        yield return new WaitForSeconds(holdAfterRoll);

        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            panelGroup.alpha = 1f - Mathf.Clamp01(t / fadeDuration);
            yield return null;
        }

        dicePanel.SetActive(false);
        IsRolling = false;
    }
}