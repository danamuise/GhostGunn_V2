using UnityEngine;
using System.Collections.Generic;
using System.Collections;
public class GameManager : MonoBehaviour
{
    public TargetManager targetManager;
    public GhostShooter gun;

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
        gun.DisableGun(); // You’ll create this next
                                   // Optional: Show UI, restart level, etc.
    }

}
