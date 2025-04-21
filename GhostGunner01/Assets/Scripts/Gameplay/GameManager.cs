using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public TargetManager targetManager;
    public GhostShooter gun;
    public GameObject gameOverPopup;
    public PowerUpManager powerUpManager;
    private bool roundInProgress;
    private int moveCount = 1;

    private void Awake()
    {
        roundInProgress = false;
    }

    void Start()
    {
        targetManager.InitializeGrid();
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

        if (targetManager.WillMoveIntoGameOverZone())
        {
            TriggerGameOver();
            yield break;
        }

        // Start moving targets down
        Coroutine moveRoutine = StartCoroutine(targetManager.MoveTargetsDown());

        // Move power-ups down by one row
        powerUpManager.MovePowerUpsDown(targetManager.GetRowSpacing());

        yield return new WaitForSeconds(0.05f); // Slight buffer

        // Spawn new targets into Area 1
        targetManager.SpawnTargetsInArea(0, moveCount);

        yield return new WaitForSeconds(0.25f); // Let targets animate into place

        // Try to spawn a power-up
        powerUpManager.TrySpawnSelectedPowerUp(targetManager);

        // Wait for all target movement to finish
        yield return moveRoutine;

        gun.EnableGun(true);
        roundInProgress = false;

        powerUpManager.OnNewMove(moveCount);
        moveCount++;
    }




    private void TriggerGameOver()
    {
        Debug.Log("GAME OVER!");
        gun.DisableGun();

        if (gameOverPopup != null)
        {
            Debug.Log("✅ Showing Game Over Popup");
            gameOverPopup.SetActive(true);
        }

        targetManager.ClearAllTargets();
    }

}
