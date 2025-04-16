using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public TargetManager targetManager;
    public GhostShooter gun;
    public GameObject gameOverPopup;
    private bool roundInProgress;

    private void Awake()
    {
        roundInProgress = false;
    }
    void Start()
    {
        Debug.Log("🧠 GameManager.Start() running");
        // First spawn at game start
        targetManager.SpawnTargetsInArea(0);
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
        if (targetManager.WillMoveIntoGameOverZone())
        {
            TriggerGameOver();
            yield break;
        }

        // Begin moving targets down
        Coroutine moveRoutine = StartCoroutine(targetManager.MoveTargetsDown());

        // Short delay to stagger the animation slightly, tweak as needed
        yield return new WaitForSeconds(0.05f);

        // Spawn new targets into Area 1 while others are still moving
        targetManager.SpawnTargetsInArea(0);

        // Wait for movement to finish
        yield return moveRoutine;

        // Re-enable the gun
        gun.EnableGun(true);

        roundInProgress = false;
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
