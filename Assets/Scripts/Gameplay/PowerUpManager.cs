using System.Collections.Generic;
using UnityEngine;

public class PowerUpManager : MonoBehaviour
{
    [Header("References")]
    public TargetGridManager gridTargetManager;      // Assign in Inspector
    public Transform powerUpParent;                  // Container for all power-ups

    [Header("Power-Ups")]
    public List<PowerUpData> powerUps;               // Each PowerUpData defines prefab, VFX, SFX, etc.

    [Header("VFX / SFX Settings")]
    public Vector2 vfxOffset = new Vector2(0f, 0.9f);

    private int totalTargetSpawnCycles = 0;

    public void TrySpawnPowerUp(int move)
    {
        if (powerUps == null || powerUps.Count == 0)
        {
            Debug.LogWarning("[PowerUpManager] No power-ups assigned.");
            return;
        }

        BulletPool bulletPool = FindObjectOfType<BulletPool>();
        if (bulletPool == null) return;

        List<int> availableCols = gridTargetManager.GetAvailableColumnsInRow(0);
        if (availableCols == null || availableCols.Count == 0) return;

        PowerUpData selectedPU = null;

        // AddBulletPU logic
        if (bulletPool.GetEnabledBulletCount() < bulletPool.GetTotalBulletCount())
        {
            if (move % 2 == 0 && powerUps.Count > 0)
            {
                selectedPU = powerUps[0];
            }
        }
        else
        {
            // ProximityBombPU logic (every 3rd spawn cycle)
            totalTargetSpawnCycles++;
            if (totalTargetSpawnCycles % 4 == 0 && powerUps.Count > 1)
            {
                selectedPU = powerUps[1];
            }
        }

        if (selectedPU == null || selectedPU.powerUpPrefab == null) return;

        int chosenCol = availableCols[Random.Range(0, availableCols.Count)];
        Vector2 spawnPos = gridTargetManager.GetWorldPosition(chosenCol, 0);

        GameObject newPU = Instantiate(selectedPU.powerUpPrefab, spawnPos, Quaternion.identity, powerUpParent);

        PowerUpMover mover = newPU.GetComponent<PowerUpMover>();
        if (mover != null)
        {
            mover.AnimateToPosition(spawnPos, 0.5f, fromEndzone: true);
        }

        gridTargetManager.MarkCellOccupied(chosenCol, 0, true);
        Debug.Log($"🧲 Spawned: {selectedPU.powerUpName} — Move {move}, Column {chosenCol + 1}");
    }

    public void PlayPickupEffects(Vector2 position, PowerUpData powerUpData)
    {
        if (powerUpData == null) return;

        // Spawn VFX
        if (powerUpData.pickupVFX != null)
        {
            Vector2 vfxPos = position + vfxOffset;
            Vector3 worldPos = new Vector3(position.x, position.y, 0f);
            GameObject vfx = Instantiate(powerUpData.pickupVFX, worldPos, Quaternion.identity);

            vfx.transform.localScale = Vector3.one * 0.6f;
            Destroy(vfx, 2f);
        }

        // Play SFX
        if (!string.IsNullOrEmpty(powerUpData.pickupSFX))
        {
            SFXManager.Instance.Play(powerUpData.pickupSFX, 0.5f, 0.9f, 1.1f);
        }
    }

    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    void DrawDebugMarker(Vector3 pos, Color color)
    {
#if UNITY_EDITOR
        GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        marker.transform.position = pos;
        marker.transform.localScale = Vector3.one * 0.2f;
        marker.GetComponent<Renderer>().material.color = color;
        marker.name = "💥 VFX Debug Marker";
        Destroy(marker, 2f); // Auto-destroy after 2 seconds
#endif
    }

}
