using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

/// <summary>
/// GameOverScreen — handles both the bad ending (game over) and good ending (survived).
/// Attach to your EndCanvas GameObject.
/// Call ShowBadEnding(suspicion) for game over.
/// Call ShowGoodEnding(suspicion) for the survived ending.
/// </summary>
public class GameOverScreen : MonoBehaviour
{
    // ─────────────────────────────────────────────
    //  INSPECTOR FIELDS
    // ─────────────────────────────────────────────

    [Header("── UI Elements ─────────────────────")]
    [Tooltip("The CanvasGroup on your Panel — controls fade")]
    public CanvasGroup screenPanel;

    [Tooltip("The main title text")]
    public TMP_Text titleText;

    [Tooltip("Flavour text shown beneath the title")]
    public TMP_Text flavourText;

    [Tooltip("Smaller closing line shown last")]
    public TMP_Text closingText;

    [Tooltip("Restart button — shown on bad ending only")]
    public Button restartButton;

    [Tooltip("Quit button")]
    public Button quitButton;

    [Header("── Timing ───────────────────────────")]
    [Tooltip("Delay before screen starts fading in")]
    public float fadeDelay = 1.5f;

    [Tooltip("How long the fade in takes")]
    public float fadeDuration = 3.0f;

    [Tooltip("Delay between title and flavour text appearing")]
    public float textStaggerDelay = 1.5f;

    [Header("── Scene ────────────────────────────")]
    [Tooltip("Name of your game scene for restarting")]
    public string gameSceneName = "Game";

    // ─────────────────────────────────────────────
    //  BAD ENDING TEXT
    // ─────────────────────────────────────────────

    private string[] badEndingTitles = {
        "The village has spoken.",
        "They came for you at dawn.",
        "There is nowhere left to hide."
    };

    private string[] lowSuspicionFlavour = {
        "One whisper was all it took.",
        "The village has a long memory.",
        "You did not see it coming."
    };

    private string[] highSuspicionFlavour = {
        "You pushed too hard.\nThe Reverend had been watching.",
        "Pride was your undoing.",
        "The herbs were still hanging\nwhen they arrived."
    };

    private string badEndingClosing =
        "Eleanor's story ends here.";

    // ─────────────────────────────────────────────
    //  GOOD ENDING TEXT
    // ─────────────────────────────────────────────

    private string goodEndingTitle =
        "She survived another day.";

    private string[] goodEndingFlavour = {
        "The village watches.\nBut it does not yet act.",
        "The herbs remain hidden.\nFor now, that is enough.",
        "Martha's silence was a kindness.\nEleanor would not forget it."
    };

    private string goodEndingClosing =
        "But for how long?";

    // ─────────────────────────────────────────────
    //  START — screen starts fully hidden
    // ─────────────────────────────────────────────

    void Start()
    {
        HideAll();

        if (restartButton != null)
            restartButton.onClick.AddListener(Restart);

        if (quitButton != null)
            quitButton.onClick.AddListener(Quit);
    }

    private void HideAll()
    {
        if (screenPanel != null)
        {
            screenPanel.alpha = 0f;
            screenPanel.interactable = false;
            screenPanel.blocksRaycasts = false;
        }

        if (titleText != null) titleText.alpha = 0f;
        if (flavourText != null) flavourText.alpha = 0f;
        if (closingText != null) closingText.alpha = 0f;

        if (restartButton != null)
            restartButton.gameObject.SetActive(false);
    }

    // ─────────────────────────────────────────────
    //  PUBLIC — call these from SuspicionManager
    // ─────────────────────────────────────────────

    /// <summary>
    /// Call this when suspicion hits max — bad ending.
    /// </summary>
    public void ShowBadEnding(int finalSuspicion)
    {
        // Pick title
        string title = badEndingTitles[Random.Range(0, badEndingTitles.Length)];

        // Pick flavour based on suspicion level
        string[] pool = finalSuspicion >= 8 ? highSuspicionFlavour : lowSuspicionFlavour;
        string flavour = pool[Random.Range(0, pool.Length)];

        if (restartButton != null)
            restartButton.gameObject.SetActive(true);

        StartCoroutine(PlayEnding(title, flavour, badEndingClosing));
    }

    /// <summary>
    /// Shorthand for bad ending with no suspicion value.
    /// </summary>
    public void Show(int finalSuspicion) => ShowBadEnding(finalSuspicion);
    public void Show() => ShowBadEnding(10);

    /// <summary>
    /// Call this from Fungus Search_Survived block — good ending.
    /// </summary>
    public void ShowGoodEnding(int finalSuspicion)
    {
        string flavour = goodEndingFlavour[Random.Range(0, goodEndingFlavour.Length)];

        if (restartButton != null)
            restartButton.gameObject.SetActive(false);

        StartCoroutine(PlayEnding(goodEndingTitle, flavour, goodEndingClosing));
    }

    // ─────────────────────────────────────────────
    //  ENDING SEQUENCE COROUTINE
    // ─────────────────────────────────────────────

    private IEnumerator PlayEnding(string title, string flavour, string closing)
    {
        // Set text content
        if (titleText != null) titleText.text = title;
        if (flavourText != null) flavourText.text = flavour;
        if (closingText != null) closingText.text = closing;

        // Wait before starting
        yield return new WaitForSeconds(fadeDelay);

        // Fade the background panel to black
        yield return StartCoroutine(FadePanel(fadeDuration));

        // Enable interaction
        if (screenPanel != null)
            screenPanel.interactable = true;

        // Stagger the text in one by one
        yield return StartCoroutine(FadeText(titleText, 1.5f));
        yield return new WaitForSeconds(textStaggerDelay);

        yield return StartCoroutine(FadeText(flavourText, 1.5f));
        yield return new WaitForSeconds(textStaggerDelay);

        yield return StartCoroutine(FadeText(closingText, 2.0f));

        // Show restart button if it's a bad ending
        if (restartButton != null && restartButton.gameObject.activeSelf)
        {
            yield return new WaitForSeconds(1.0f);
            restartButton.GetComponent<CanvasGroup>().alpha = 1f;
        }
    }

    // ─────────────────────────────────────────────
    //  FADE HELPERS
    // ─────────────────────────────────────────────

    private IEnumerator FadePanel(float duration)
    {
        if (screenPanel == null) yield break;

        screenPanel.blocksRaycasts = true;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            screenPanel.alpha = Mathf.Clamp01(elapsed / duration);
            yield return null;
        }

        screenPanel.alpha = 1f;
    }

    private IEnumerator FadeText(TMP_Text text, float duration)
    {
        if (text == null) yield break;

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            text.alpha = Mathf.Clamp01(elapsed / duration);
            yield return null;
        }

        text.alpha = 1f;
    }

    // ─────────────────────────────────────────────
    //  BUTTONS
    // ─────────────────────────────────────────────

    public void Restart()
    {
        SceneManager.LoadScene(gameSceneName);
    }

    public void Quit()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}