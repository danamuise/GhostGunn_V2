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

    private bool roundInProgress;
    private int moveCount = 1;

    private void Awake()
    {
        roundInProgress = false;
    }

    void Start()
    {
        grid.InitializeGrid();
        gun.EnableGun(true);
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
        Debug.Log($"<color=green>🌟 MOVE {moveCount} initiating</color>");

        // Step 1: Animate all targets down visually (position only)
        float rowSpacing = grid.cellHeight;
        yield return StartCoroutine(targetManager.MoveTargetsDown(rowSpacing));

        // Step 2: Shift grid state + spawn new targets into Area 1
        gridTargetSpawner.AdvanceAllTargetsAndSpawnNew(moveCount);

        // Step 3: Check to see if there are targets in Area 10 (after landing visually)
        yield return new WaitForSeconds(0.6f);

        if (LeadArea() == 9)  // Assuming Area 10 = row index 9
        {
            Debug.Log("💀 Final move reached — targets are now in Area 10.");
            TriggerGameOver();
            yield break;
        }

        // Step 4: Ready for next round
        gun.EnableGun(true);
        roundInProgress = false;
        moveCount++;
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

        return leadRow; // Returns the highest row index that has something in it
    }


    public int GetMoveCount()
    {
        return moveCount;
    }

}
