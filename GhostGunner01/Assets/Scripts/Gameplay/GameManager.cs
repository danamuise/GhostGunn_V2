using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public TargetManager targetManager;
    public GhostShooter gun;
    public GameObject gameOverPopup;

    public void OnShotComplete()
    {
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
