using UnityEngine;
using UnityEngine.UI;
using Fungus;

/// <summary>
/// SuspicionManager — attach this to a single empty GameObject in your scene.
/// Tracks all narrative variables, syncs them to Fungus, drives the suspicion UI,
/// and handles game-over + NPC unlock logic.
/// </summary>
public class SuspicionManager : MonoBehaviour
{
    // ─────────────────────────────────────────────
    //  SINGLETON
    // ─────────────────────────────────────────────
    public static SuspicionManager Instance { get; private set; }

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // ─────────────────────────────────────────────
    //  INSPECTOR FIELDS
    // ─────────────────────────────────────────────

    [Header("── Fungus ──────────────────────")]
    [Tooltip("Drag your Flowchart GameObject here")]
    public Flowchart flowchart;

    [Header("── Suspicion UI ─────────────────")]
    [Tooltip("Drag the Slider UI element here")]
    public Slider suspicionSlider;
    [Tooltip("Drag the Fill Image of the Slider here")]
    public Image sliderFill;

    [Header("── Suspicion Settings ──────────")]
    [Tooltip("Maximum suspicion before game over — set to 8")]
    public int maxSuspicion = 8;

    [Header("── Game Over ───────────────────")]
    [Tooltip("Drag your GameOverScreen GameObject here")]
    public GameOverScreen gameOverScreen;

    // ─────────────────────────────────────────────
    //  RUNTIME VARIABLES
    // ─────────────────────────────────────────────

    [Header("── Current Values (read-only) ──")]
    [SerializeField] private int suspicion = 0;
    [SerializeField] private int marthaTrust = 0;
    [SerializeField] private int thomasTrust = 0;
    [SerializeField] private int haleTrust = 0;

    [Header("── Progression Flags ───────────")]
    [SerializeField] private bool hasCompletedTutorial = false;
    [SerializeField] private bool hasTalkedToThomas = false;
    [SerializeField] private bool hasTalkedToHale = false;

    private bool gameOverTriggered = false;

    // ─────────────────────────────────────────────
    //  START
    // ─────────────────────────────────────────────

    void Start()
    {
        SyncAll();
    }

    // ─────────────────────────────────────────────
    //  CORE: MODIFY ANY VARIABLE
    // ─────────────────────────────────────────────

    private void Modify(string variable, int amount)
    {
        switch (variable)
        {
            case "Suspicion": suspicion = Mathf.Clamp(suspicion + amount, 0, maxSuspicion); break;
            case "MarthaTrust": marthaTrust = Mathf.Clamp(marthaTrust + amount, -10, 10); break;
            case "ThomasTrust": thomasTrust = Mathf.Clamp(thomasTrust + amount, -10, 10); break;
            case "HaleTrust": haleTrust = Mathf.Clamp(haleTrust + amount, -10, 10); break;
        }
        SyncAll();
        CheckGameOver();
    }

    // ─────────────────────────────────────────────
    //  GAME OVER
    // ─────────────────────────────────────────────

    private void CheckGameOver()
    {
        if (gameOverTriggered) return;

        if (suspicion >= maxSuspicion)
        {
            gameOverTriggered = true;
            Debug.Log("[SuspicionManager] Game over triggered. Final suspicion: " + suspicion);

            if (gameOverScreen != null)
                gameOverScreen.ShowBadEnding(suspicion);
        }
    }

    public bool IsGameOver() => suspicion >= maxSuspicion;

    // ─────────────────────────────────────────────
    //  GOOD ENDING
    // ─────────────────────────────────────────────

    public void TriggerGoodEnding()
    {
        Debug.Log("[SuspicionManager] Good ending triggered.");
        if (gameOverScreen != null)
            gameOverScreen.ShowGoodEnding(suspicion);
    }

    // ─────────────────────────────────────────────
    //  SYNC TO FUNGUS + UPDATE UI
    // ─────────────────────────────────────────────

    private void SyncAll()
    {
        if (flowchart != null)
        {
            flowchart.SetIntegerVariable("Suspicion", suspicion);
            flowchart.SetIntegerVariable("MarthaTrust", marthaTrust);
            flowchart.SetIntegerVariable("ThomasTrust", thomasTrust);
            flowchart.SetIntegerVariable("HaleTrust", haleTrust);

            flowchart.SetBooleanVariable("HasCompletedTutorial", hasCompletedTutorial);
            flowchart.SetBooleanVariable("HasTalkedToThomas", hasTalkedToThomas);
            flowchart.SetBooleanVariable("HasTalkedToHale", hasTalkedToHale);
        }

        UpdateSlider();
    }

    private void UpdateSlider()
    {
        if (suspicionSlider != null)
        {
            suspicionSlider.maxValue = maxSuspicion;
            suspicionSlider.value = suspicion;
        }

        if (sliderFill != null)
        {
            float t = (float)suspicion / maxSuspicion;
            sliderFill.color = Color.Lerp(Color.green, Color.red, t);
        }
    }

    // ─────────────────────────────────────────────
    //  PROGRESSION
    // ─────────────────────────────────────────────

    public void CompleteTutorial()
    {
        hasCompletedTutorial = true;
        SyncAll();
        Debug.Log("[SuspicionManager] Tutorial complete. Thomas and Hale now available.");
    }

    public bool CanTalkToNPC() => hasCompletedTutorial;

    public void SetTalkedToThomas()
    {
        hasTalkedToThomas = true;
        SyncAll();
        CheckAllNPCsTalkedTo();
    }

    public void SetTalkedToHale()
    {
        hasTalkedToHale = true;
        SyncAll();
        CheckAllNPCsTalkedTo();
    }

    private void CheckAllNPCsTalkedTo()
    {
        if (hasTalkedToThomas && hasTalkedToHale)
        {
            Debug.Log("[SuspicionManager] Both NPCs done. Firing Day3_BranchEvent.");
            if (flowchart != null)
                flowchart.ExecuteBlock("Day3_BranchEvent");
        }
    }

    // ─────────────────────────────────────────────
    //  DAY 1 — MARTHA
    //  Max from this scene: +4 (Help Openly)
    // ─────────────────────────────────────────────

    /// <summary> Option 1: "I cannot help you." </summary>
    public void Day1_Refuse()
    {
        Modify("Suspicion", -1);
        Modify("MarthaTrust", -2);
    }

    /// <summary> Option 2: "Return after sunset. Come alone." </summary>
    public void Day1_HelpQuietly()
    {
        Modify("Suspicion", +1);
        Modify("MarthaTrust", +2);
    }

    /// <summary> Option 3: "Bring him inside." </summary>
    public void Day1_HelpOpenly()
    {
        Modify("Suspicion", +4); // raised from +3
        Modify("MarthaTrust", +3);
    }

    // ─────────────────────────────────────────────
    //  DAY 2 — BAKER THOMAS
    //  Max from this scene: +3 (Tell Truth)
    // ─────────────────────────────────────────────

    /// <summary> Option 1: "It's fungus. Some can heal... some can ruin." </summary>
    public void Day2_TellTruth()
    {
        Modify("ThomasTrust", +3);
        Modify("Suspicion", +3); // raised from +2
    }

    /// <summary> Option 2: "Dry your flour. Keep your cellar aired." </summary>
    public void Day2_GiveAdvice()
    {
        Modify("ThomasTrust", +2);
        // No suspicion change
    }

    /// <summary> Option 3: "I know nothing of baking." </summary>
    public void Day2_Refuse()
    {
        Modify("ThomasTrust", -2);
        // No suspicion change
    }

    // ─────────────────────────────────────────────
    //  DAY 3 — REVEREND HALE
    //  Max from this scene: +4 (Challenge)
    // ─────────────────────────────────────────────

    /// <summary> Option 1: "I only help where I can." </summary>
    public void Day3_StayCalm()
    {
        Modify("HaleTrust", +2);
        // No suspicion change
    }

    /// <summary> Option 2: "I know nothing of herbs." </summary>
    public void Day3_Lie()
    {
        Modify("HaleTrust", -2);
        Modify("Suspicion", +3); // raised from +2
    }

    /// <summary> Option 3: "Would you rather the sick be left to die?" </summary>
    public void Day3_Challenge()
    {
        Modify("Suspicion", +4); // raised from +3
        Modify("HaleTrust", -3);
    }

    // ─────────────────────────────────────────────
    //  SEARCH BRANCH
    // ─────────────────────────────────────────────

    /// <summary> "They are medicine, nothing more." </summary>
    public void Search_Defend()
    {
        Modify("HaleTrust", +2);
        Modify("Suspicion", -1);
    }

    /// <summary> "You have no right to be here." </summary>
    public void Search_Challenge()
    {
        Modify("Suspicion", +4);
    }

    /// <summary> Stay silent </summary>
    public void Search_Silent()
    {
        Modify("Suspicion", +1);
    }

    /// <summary> "Hide everything quickly." </summary>
    public void Search_Hide()
    {
        Modify("Suspicion", -2);
        Modify("MarthaTrust", +1);
    }

    /// <summary> "Stand your ground." </summary>
    public void Search_StandGround()
    {
        Modify("Suspicion", +2);
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            Debug.Log("TEST — adding 4 suspicion manually");
            Modify("Suspicion", +4);
        }
    }
}
