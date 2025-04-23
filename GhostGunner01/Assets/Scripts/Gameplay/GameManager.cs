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

        // Step 3: If this is Move 10, trigger Game Over (after targets land visually)
        if (moveCount == 10)
        {
            Debug.Log("💀 Final move reached — targets are now in Area 10.");
            yield return new WaitForSeconds(0.6f); // Delay to allow targets to visually arrive
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

    public int GetMoveCount()
    {
        return moveCount;
    }

}
