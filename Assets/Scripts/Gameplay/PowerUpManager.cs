using System.Collections.Generic;
using UnityEngine;

public class PowerUpManager : MonoBehaviour
{
    [Header("References")]
    public TargetGridManager gridTargetManager;  // Assign in Inspector
    public GameObject addBulletPrefab;           // Assign AddBulletPU prefab in Inspector
    public Transform powerUpParent;              // Assign PowerUps container in Inspector

    [Header("Shared VFX / SFX")]
    public GameObject PUPickUpVFX;               // VFX prefab (auto-destroys in 2 sec)
    public void TrySpawnAddBulletPU(int move)
    {
        BulletPool bulletPool = FindObjectOfType<BulletPool>();

        if (bulletPool == null)
        {
            Debug.LogWarning("[PowerUpManager] Missing BulletPool reference.");
            return;
        }

        // ✅ Enforce even-numbered moves here
        if (move % 2 != 0)
        {
            Debug.Log($"[PowerUpManager] Skipping spawn — Move {move} is odd.");
            return;
        }

        if (bulletPool.GetEnabledBulletCount() >= bulletPool.GetTotalBulletCount())
        {
            Debug.Log("[PowerUpManager] Bullet tank full — skipping AddBulletPU spawn.");
            return;
        }

        List<int> availableCols = gridTargetManager.GetAvailableColumnsInRow(0);
        if (availableCols == null || availableCols.Count == 0)
        {
            Debug.Log("[PowerUpManager] No available columns for PowerUp spawn.");
            return;
        }

        int chosenCol = availableCols[Random.Range(0, availableCols.Count)];
        Vector2 spawnPos = gridTargetManager.GetWorldPosition(chosenCol, 0);

        Debug.Log($"🧲 Spawning AddBulletPU — Move {move}, Column {chosenCol + 1}");

        GameObject newPU = Instantiate(addBulletPrefab, spawnPos, Quaternion.identity, powerUpParent);

        var mover = newPU.GetComponent<PowerUpMover>();
        if (mover != null)
        {
            mover.AnimateToPosition(spawnPos, 0.5f, fromEndzone: true);
        }

        gridTargetManager.MarkCellOccupied(chosenCol, 0, true);
    }

    public void PlayPickupEffects(Vector2 position)
    {
        // VFX block
        if (PUPickUpVFX != null)
        {
            position.y += 0.9f;

            GameObject vfx = Instantiate(PUPickUpVFX, position, Quaternion.identity);
            vfx.transform.localScale = Vector3.one * 0.6f; // 💡 Adjust the scale here
            Destroy(vfx, 2f); // ⏱️ Auto-destroy after 2 seconds
        }
            SFXManager.Instance.Play("PUCollect", 0.5f, 0.9f, 1.1f);
    }

}
