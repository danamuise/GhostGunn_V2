using UnityEngine;
using System.Collections.Generic;
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
        yield return StartCoroutine(targetManager.MoveTargetsDown());

        if (targetManager.CheckForGameOver())
        {
            TriggerGameOver();
        }
        else
        {
            targetManager.SpawnTargetsInArea1();
            gun.EnableGun(true); // or whatever your gun activation method is
        }
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
