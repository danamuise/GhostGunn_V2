using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour
{
    [Header("Scene References")]
    public TargetManager targetManager;
    public GridTargetSpawner gridTargetSpawner;
    public TargetGridManager grid;
    public GhostShooter gun;
    public GameObject gameOverPopup;
    public UIManager uiManager;

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
        moveCount++; // ✅ Advance first — guarantees correct moveCount for this round

        Debug.Log($"<color=green>🌟 MOVE {moveCount} initiating</color>");

        float rowSpacing = grid.cellHeight;
        yield return StartCoroutine(targetManager.MoveTargetsDown(rowSpacing));

        gridTargetSpawner.AdvanceAllTargetsAndSpawnNew(moveCount); // Targets and PowerUps use correct move #

        yield return new WaitForSeconds(0.6f);

        if (LeadArea() == 9)
        {
            Debug.Log("💀 Final move reached — targets are now in Area 10.");
            TriggerGameOver();
            yield break;
        }

        gun.EnableGun(true);
        roundInProgress = false;
    }


    private void TriggerGameOver()
    {
        Debug.Log("💀 GAME OVER!");
        gun.DisableGun();

        if (gameOverPopup != null)
        {
            Debug.Log("✅ Showing Game Over Popup");
            gameOverPopup.SetActive(true);
        }

        uiManager?.ShowFinalScore(totalScore);
        targetManager.ClearAllTargets();
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

    public int GetMoveCount()
    {
        return moveCount;
    }

    public void AddScore(int amount)
    {
        Debug.Log($"➕ Adding {amount} points to score. New total: {totalScore + amount}");
        totalScore += amount;
        uiManager.UpdateScoreDisplay(totalScore);
    }

    public void ResetScore()
    {
        totalScore = 0;
    }
}
