using Fungus;
using UnityEngine;
using UnityEngine.UI;

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
    //  INSPECTOR FIELDS — drag objects in here
    // ─────────────────────────────────────────────

    [Header("── Fungus ──────────────────────")]
    [Tooltip("Drag your Flowchart GameObject here")]
    public Flowchart flowchart;

    [Header("── Suspicion UI ─────────────────")]
    [Tooltip("Drag the Slider UI element here")]
    public Slider suspicionSlider;
    [Tooltip("Drag the Fill Image of the Slider here (for colour change)")]
    public Image sliderFill;

    [Header("── Suspicion Settings ──────────")]
    [Tooltip("Maximum suspicion before game over")]
    public int maxSuspicion = 10;

    [Header("── Game Over ───────────────────")]
    [Tooltip("Drag your GameOverScreen GameObject here")]
    public GameOverScreen gameOverScreen;
    // ─────────────────────────────────────────────
    //  RUNTIME VARIABLES (shown in Inspector)
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

    /// <summary>
    /// Internal method — modifies a named variable by amount and syncs everything.
    /// </summary>
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
    //  SYNC TO FUNGUS + UPDATE UI
    // ─────────────────────────────────────────────

    private void SyncAll()
    {
        if (flowchart != null)
        {
            // Numeric variables
            flowchart.SetIntegerVariable("Suspicion", suspicion);
            flowchart.SetIntegerVariable("MarthaTrust", marthaTrust);
            flowchart.SetIntegerVariable("ThomasTrust", thomasTrust);
            flowchart.SetIntegerVariable("HaleTrust", haleTrust);

            // Progression flags
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
    //  GAME OVER CHECK
    //  Call this from Fungus "CheckSuspicion" block
    // ─────────────────────────────────────────────

    /// <summary>
    /// Returns true if suspicion has hit max.
    /// Fungus CheckSuspicion block should call ExecuteBlock("GameOver") if this is true.
    /// </summary>
    private void CheckGameOver()
    {
        if (gameOverTriggered) return;

        if (suspicion >= maxSuspicion)
        {
            gameOverTriggered = true;
            if (gameOverScreen != null)
                gameOverScreen.Show(suspicion);
        }
    }
    
    

    public bool IsGameOver()
    {
        return suspicion >= maxSuspicion;
    }

    // ─────────────────────────────────────────────
    //  PROGRESSION
    // ─────────────────────────────────────────────

    /// <summary>
    /// Call at the END of Martha's dialogue block.
    /// Unlocks Thomas and Hale for free-roam interaction.
    /// </summary>
    public void CompleteTutorial()
    {
        hasCompletedTutorial = true;
        SyncAll();
        Debug.Log("[SuspicionManager] Tutorial complete. Thomas and Hale are now available.");
    }

    /// <summary>
    /// Returns true if the player has finished Martha's tutorial scene.
    /// Gate Thomas and Hale interactions behind this.
    /// </summary>
    public bool CanTalkToNPC()
    {
        return hasCompletedTutorial;
    }

    /// <summary>
    /// Call at the END of Thomas's dialogue block.
    /// </summary>
    public void SetTalkedToThomas()
    {
        hasTalkedToThomas = true;
        SyncAll();
        CheckAllNPCsTalkedTo();
    }

    /// <summary>
    /// Call at the END of Hale's dialogue block.
    /// </summary>
    public void SetTalkedToHale()
    {
        hasTalkedToHale = true;
        SyncAll();
        CheckAllNPCsTalkedTo();
    }

    /// <summary>
    /// Automatically fires Day3_BranchEvent once both Thomas and Hale have been spoken to.
    /// Called internally after each NPC conversation ends.
    /// </summary>
    private bool gameOverTriggered = false;
    private void CheckAllNPCsTalkedTo()
    {
        if (hasTalkedToThomas && hasTalkedToHale)
        {
            Debug.Log("[SuspicionManager] Both NPCs talked to. Firing Day3_BranchEvent.");
            if (flowchart != null)
                flowchart.ExecuteBlock("Day3_BranchEvent");
        }
    }

    // ─────────────────────────────────────────────
    //  DAY 1 — MARTHA (tutorial)
    //  Wire these to Fungus Menu options via Call Method
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
        Modify("Suspicion", +3);
        Modify("MarthaTrust", +3);
    }

    // ─────────────────────────────────────────────
    //  DAY 2 — BAKER THOMAS (free-roam)
    //  Wire these to Fungus Menu options via Call Method
    // ─────────────────────────────────────────────

    /// <summary> Option 1: "It's fungus. Some can heal... some can ruin." </summary>
    public void Day2_TellTruth()
    {
        Modify("ThomasTrust", +3);
        Modify("Suspicion", +2);
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
    //  DAY 3 — REVEREND HALE (free-roam)
    //  Wire these to Fungus Menu options via Call Method
    // ─────────────────────────────────────────────

    /// <summary> Option 1: "I only help where I can." </summary>
    public void Day3_StayCalm()
    {
        Modify("HaleTrust", +2);
    }

    /// <summary> Option 2: "I know nothing of herbs." </summary>
    public void Day3_Lie()
    {
        Modify("HaleTrust", -2);
        Modify("Suspicion", +2);
    }

    /// <summary> Option 3: "Would you rather the sick be left to die?" </summary>
    public void Day3_Challenge()
    {
        Modify("Suspicion", +3);
        Modify("HaleTrust", -3);
    }
}