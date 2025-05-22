using UnityEngine;
using System.Collections.Generic;

public class ProximityBombPU : MonoBehaviour
{
    public float checkRadius = 0.1f;
    public LayerMask targetLayer;
    public PowerUpData powerUpData;

    private TargetGridManager gridManager;

    private void Awake()
    {
        gridManager = FindObjectOfType<TargetGridManager>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Bullet"))
        {
            Activate();
        }
    }

    public void Activate()
    {
        if (gridManager == null)
        {
            Debug.LogWarning("ProximityBombPU: No TargetGridManager found.");
            return;
        }

        // Get grid coordinates of the power-up
        if (!gridManager.GetGridCoordinates(transform.position, out int col, out int row))
        {
            Debug.LogWarning("ProximityBombPU: Failed to get grid coordinates.");
            return;
        }

        List<(int c, int r)> affectedCells = new();

        // Add horizontal range (left/right 2)
        for (int dx = -2; dx <= 2; dx++)
        {
            if (dx != 0) affectedCells.Add((col + dx, row));
        }

        // Add vertical range (up/down 2)
        for (int dy = -2; dy <= 2; dy++)
        {
            if (dy != 0) affectedCells.Add((col, row + dy));
        }

        foreach (var (c, r) in affectedCells)
        {
            if (gridManager.IsCellInBounds(c, r))
            {
                GameObject target = gridManager.GetTargetAt(c, r);
                if (target != null && target.CompareTag("Target"))
                {
                    TargetBehavior tb = target.GetComponentInParent<TargetBehavior>();
                    if (tb != null)
                    {
                        int hp = tb.GetCurrentHealth();
                        tb.TakeDamage(hp);
                        Debug.Log($"💣 ProximityBomb destroyed target at col={c} row={r}");
                    }
                }
            }
        }

        // Trigger camera shake
        CameraShaker.Instance?.Shake(0.2f, 0.15f);

        // Trigger pickup VFX/SFX
        PowerUpManager manager = FindObjectOfType<PowerUpManager>();
        if (manager != null && powerUpData != null)
        {
            manager.PlayPickupEffects(transform.position, powerUpData);
        }

        Destroy(gameObject);
    }

}
