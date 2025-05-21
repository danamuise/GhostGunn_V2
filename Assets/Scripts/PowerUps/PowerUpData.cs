using UnityEngine;

[CreateAssetMenu(fileName = "PowerUpData", menuName = "GhostGunn/PowerUp")]
public class PowerUpData : ScriptableObject
{
    [Header("Core Settings")]
    public string powerUpName;
    public GameObject powerUpPrefab;
    public int priority = 1; // Lower = higher priority
    public int cooldown = 1;

    [Header("Pickup Effects")]
    public GameObject pickupVFX;           // Optional visual FX prefab to spawn on pickup
    public string pickupSFX = "PUCollect"; // Sound effect name registered in SFXManager

    [Header("Probability")]
    [Range(0f, 1f)]
    public float probability = 1.0f; // Higher = more frequent appearance

    public int timesUsed = 0;
    public int lastUsedMove = -1000; // Used for cooldown check

    public void ResetRuntimeState()
    {
        lastUsedMove = -1000;
        timesUsed = 0;
    }

    // Determines whether this power-up can be used on the current move
    public bool IsAvailable(int currentMove)
    {
        if (timesUsed >= 20) return false;

        int movesSinceLastUse = currentMove - lastUsedMove;
        if (movesSinceLastUse < cooldown) return false;

        if (Random.value > probability) return false;

        return true;
    }
}
