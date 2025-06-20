using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("Scene References")]
    public TargetManager targetManager;
    public GridTargetSpawner gridTargetSpawner;
    public TargetGridManager grid;
    public GhostShooter gun;
    public UIManager uiManager;

    [Header("NukePU Charging")]
    public GameObject nukeChargeBar;
    public float NukeFullCharge = 1000f;
    public SpriteRenderer nukeOutline;
    private float nukeChargeProgress = 0f;
    private bool nukeArmed = false; // ✅ One-time trigger flag

    private SpriteRenderer nukeIconSR;
    private Material nukeIconMaterial;
    private readonly Color dimColor = new Color32(164, 164, 164, 255);
    private readonly Color brightColor = new Color32(255, 255, 255, 255);

    private bool roundInProgress;
    private int moveCount = 0;
    private int totalScore = 0;

    private void Awake()
    {
        roundInProgress = false;
    }

    private void Start()
    {
        SFXManager.Instance.PlayMusic("mainBGmusic", 0.3f);
        grid.InitializeGrid();
        gun.EnableGun(true);
        uiManager?.InitializeUI();

        GameObject nukeIcon = GameObject.Find("NukeIcon");
        if (nukeIcon != null)
        {
            nukeIconSR = nukeIcon.GetComponent<SpriteRenderer>();
            if (nukeIconSR != null)
            {
                nukeIconMaterial = nukeIconSR.material;
                nukeIconMaterial.color = dimColor;
            }
            else
            {
                Debug.LogWarning("⚠️ NukeIcon found but has no SpriteRenderer.");
            }
        }
        else
        {
            Debug.LogWarning("⚠️ NukeIcon not found in scene!");
        }
    }

    private void Update()
    {
        if (nukeChargeProgress >= 1f && nukeIconMaterial != null)
        {
            float loopDuration = 2.0f;
            float timeInLoop = Time.time % loopDuration;
            Color newColor;

            if (timeInLoop < 0.5f)
            {
                float t = timeInLoop / 0.5f;
                newColor = Color.Lerp(dimColor, brightColor, t);
            }
            else if (timeInLoop < 1.5f)
            {
                newColor = brightColor;
            }
            else
            {
                float t = (timeInLoop - 1.5f) / 0.5f;
                newColor = Color.Lerp(brightColor, dimColor, t);
            }

            nukeIconMaterial.color = newColor;

            if (nukeOutline != null)
            {
                float blinkFrequency = 10f;
                bool shouldShow = Mathf.PingPong(Time.time * blinkFrequency, 1f) > 0.5f;
                nukeOutline.enabled = shouldShow;
            }
        }
    }

    public void AddScore(int amount)
    {
        Debug.Log($"➕ Adding {amount} points to score. New total: {totalScore + amount}");
        totalScore += amount;
        uiManager.UpdateScoreDisplay(totalScore);

        if (nukeIconSR != null && nukeIconSR.enabled)
        {
            nukeChargeProgress += amount / NukeFullCharge;
            nukeChargeProgress = Mathf.Clamp01(nukeChargeProgress);

            if (nukeChargeBar != null)
            {
                nukeChargeBar.transform.localScale = new Vector3(1f, nukeChargeProgress, 1f);
                Debug.Log($"🔋 NukeChargeBar Y Scale: {nukeChargeProgress:F3}");
            }

            if (!nukeArmed && nukeChargeProgress >= 1f)
            {
                nukeArmed = true; // ✅ Trigger once
                Debug.Log("💣 Nuke is fully charged!");

                GameObject nukeIcon = GameObject.Find("NukeIcon");
                if (nukeIcon != null)
                {
                    NukeTarget nukeTarget = nukeIcon.GetComponent<NukeTarget>();
                    if (nukeTarget != null)
                    {
                        nukeTarget.ArmNuke();
                        Debug.Log("NukeTarget armed and ready for collision.");
                    }
                }
            }
        }
    }

    public void OnShotComplete()
    {
        if (roundInProgress)
        {
            Debug.Log("⛔ OnShotComplete() skipped — round already in progress.");
            return;
        }

        Debug.Log($"🧪 OnShotComplete() triggered at {Time.time:F2}");
        roundInProgress = true;
        StartCoroutine(HandleTargetMovementAndRespawn());
    }

    private IEnumerator HandleTargetMovementAndRespawn()
    {
        moveCount++;
        Debug.Log($"<color=green>🌟 MOVE {moveCount} initiating</color>");

        float rowSpacing = grid.cellHeight;
        yield return StartCoroutine(targetManager.MoveTargetsDown(rowSpacing));

        gridTargetSpawner.AdvanceAllTargetsAndSpawnNew(moveCount);
        yield return new WaitForSeconds(0.6f);

        if (LeadArea() == 9)
        {
            Debug.Log("⚠️ Area 10 objects detected. Checking for targets only…");

            // Check if there are any actual targets in Area 10
            bool hasTargets = HasTargetsInRow(9);

            if (hasTargets)
            {
                Debug.Log("💀 Final move reached — targets in Area 10. Game Over triggered.");
                TriggerGameOver();
                yield break;
            }
            else
            {
                Debug.Log("✅ Only power-ups in Area 10 — destroying them and continuing game.");

                // Destroy power-ups in Area 10
                int cols = grid.GetColumnCount();
                for (int col = 0; col < cols; col++)
                {
                    GameObject obj = grid.GetTargetAt(col, 9);
                    if (obj != null && obj.CompareTag("PowerUp"))
                    {
                        Destroy(obj);
                        grid.MarkCellOccupied(col, 9, false);
                    }
                }
            }
        }

        gun.EnableGun(true);
        roundInProgress = false;
    }


    private void TriggerGameOver()
    {
        Debug.Log("💀 GAME OVER!");
        gun.DisableGun();

        ScoreKeeper.finalScore = totalScore;
        SceneManager.LoadScene("GameOverScene");

        targetManager.ClearAllTargets();
        FindObjectOfType<GridTargetSpawner>()?.ResetSpawnRowCounter();
    }

    private int LeadArea()
    {
        int leadRow = -1;
        int rows = grid.GetRowCount();
        int cols = grid.GetColumnCount();

        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < cols; col++)
            {
                if (grid.IsCellOccupied(col, row))
                {
                    leadRow = Mathf.Max(leadRow, row);
                }
            }
        }

        return leadRow;
    }

    public void ResetNukeChargeBar()
    {
        nukeChargeProgress = 0f;
        nukeArmed = false; // ✅ Reset flag
        if (nukeChargeBar != null)
            nukeChargeBar.transform.localScale = new Vector3(1f, 0f, 1f);

        if (nukeIconMaterial != null)
            nukeIconMaterial.color = dimColor;

        Debug.Log("🔄 Nuke charge bar reset.");
    }

    private bool HasTargetsInRow(int rowIndex)
    {
        int cols = grid.GetColumnCount();

        for (int col = 0; col < cols; col++)
        {
            GameObject obj = grid.GetTargetAt(col, rowIndex);
            if (obj != null && obj.CompareTag("Target"))
            {
                return true;
            }
        }
        return false;
    }

    public void ResetScore()
    {
        totalScore = 0;
    }

    public int GetScore()
    {
        return totalScore;
    }

    public int GetMoveCount()
    {
        return moveCount;
    }
}
