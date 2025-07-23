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
    private bool nukeArmed = false;

    [Header("Sound Control Panel")]
    public GameObject soundUI;
    public GameObject moveUIoutButton;
    public GameObject moveUIinButton;
    public GameObject soundOffButton;
    public GameObject soundOnButton;
    public GameObject musicOnButton;
    public GameObject musicOffButton;
    private bool isSFXOn = true;
    private bool isMusicOn = true;

    private SpriteRenderer nukeIconSW;
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
        if (grid == null) grid = FindObjectOfType<TargetGridManager>();
        if (targetManager == null) targetManager = FindObjectOfType<TargetManager>();
        if (gridTargetSpawner == null) gridTargetSpawner = FindObjectOfType<GridTargetSpawner>();
        if (gun == null) gun = FindObjectOfType<GhostShooter>();
        if (uiManager == null) uiManager = FindObjectOfType<UIManager>();

        Debug.Log($"[Init] Rebinding scene refs: Grid={grid}, TargetMgr={targetManager}, Gun={gun}, UIManager={uiManager}");

        if (GameState.Instance.ContinueFromLastSave)
        {
            Debug.Log("🔁 Continuing from saved state…");
            Debug.Log($"Health base: {GameState.Instance.SavedTargetHealth}");
            Debug.Log($"Bullets: {GameState.Instance.SavedBulletCount}");

            totalScore = GameState.Instance.CurrentScore;
            uiManager.UpdateScoreDisplay(totalScore);
        }
        else
        {
            Debug.Log("🆕 Starting fresh — new level, no saved data.");
        }

        SFXManager.Instance.PlayMusic("mainBGmusic", 0.3f);
        grid.InitializeGrid();
        gun.EnableGun(true);
        uiManager?.InitializeUI();

        GameObject nukeIcon = GameObject.Find("NukeIcon");
        if (nukeIcon != null)
        {
            nukeIconSW = nukeIcon.GetComponent<SpriteRenderer>();
            if (nukeIconSW != null)
            {
                nukeIconMaterial = nukeIconSW.material;
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

        if (GameState.Instance.ContinueFromLastSave)
        {
            StartCoroutine(ResetContinueFlag());
        }

        // Load saved toggle preferences
        isSFXOn = PlayerPrefs.GetInt("SFX_ENABLED", 1) == 1;
        isMusicOn = PlayerPrefs.GetInt("MUSIC_ENABLED", 1) == 1;

        soundOnButton?.SetActive(isSFXOn);
        soundOffButton?.SetActive(!isSFXOn);
        musicOnButton?.SetActive(isMusicOn);
        musicOffButton?.SetActive(!isMusicOn);

        moveUIoutButton?.SetActive(true);
        moveUIinButton?.SetActive(false);

        if (!isMusicOn)
        {
            SFXManager.Instance.StopMusic();
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

        if (nukeIconSW != null && nukeIconSW.enabled)
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
                nukeArmed = true;
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
        nukeArmed = false;
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
                return true;
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

    private IEnumerator ResetContinueFlag()
    {
        yield return new WaitForSeconds(1.5f); // ⏳ Wait for everything to spawn
        Debug.Log("🧹 Resetting ContinueFromLastSave = false");
        //GameState.Instance.ContinueFromLastSave = false;
    }

    public void MoveUIOut()
    {
        Debug.Log("MoveUIOut() called!");
        StartCoroutine(MoveSoundUI(2.606f, 1.69f));
        moveUIoutButton.SetActive(false);
        moveUIinButton.SetActive(true);
        Invoke(nameof(MoveUIIn), 10f); // Auto-close
    }

    public void MoveUIIn()
    {
        Debug.Log("MoveUIIn() called!");
        StartCoroutine(MoveSoundUI(1.69f, 2.606f));
        moveUIinButton.SetActive(false);
        moveUIoutButton.SetActive(true);
        CancelInvoke(nameof(MoveUIIn));
    }

    private IEnumerator MoveSoundUI(float startX, float endX, float duration = 0.3f)
    {
        Vector3 startPos = new Vector3(startX, soundUI.transform.position.y, soundUI.transform.position.z);
        Vector3 endPos = new Vector3(endX, soundUI.transform.position.y, soundUI.transform.position.z);

        float time = 0f;
        while (time < 1f)
        {
            time += Time.deltaTime / duration;
            float t = Mathf.Pow(time, 2); // ease-in
            soundUI.transform.position = Vector3.Lerp(startPos, endPos, t);
            yield return null;
        }
        soundUI.transform.position = endPos;
    }

    public void ToggleMusic()
    {
        isMusicOn = !isMusicOn;
        PlayerPrefs.SetInt("MUSIC_ENABLED", isMusicOn ? 1 : 0);
        PlayerPrefs.Save();

        if (isMusicOn)
        {
            SFXManager.Instance.PlayMusic("mainBGmusic", 0.3f);
            Debug.Log("🎵 Music ON");
        }
        else
        {
            SFXManager.Instance.StopMusic();
            Debug.Log("🔇 Music OFF");
        }

        musicOnButton.SetActive(isMusicOn);
        musicOffButton.SetActive(!isMusicOn);
        MoveUIIn();
    }

    public void ToggleSFX()
    {
        isSFXOn = !isSFXOn;
        PlayerPrefs.SetInt("SFX_ENABLED", isSFXOn ? 1 : 0);
        PlayerPrefs.Save();

        soundOnButton.SetActive(isSFXOn);
        soundOffButton.SetActive(!isSFXOn);

        Debug.Log(isSFXOn ? "🔊 SFX ON" : "🔇 SFX OFF");
        MoveUIIn();
    }

}
