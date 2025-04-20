using UnityEngine;

[CreateAssetMenu(fileName = "PowerUpData", menuName = "GhostGunn/PowerUp")]
public class PowerUpData : ScriptableObject
{
    [Header("Core Settings")]
    public string powerUpName;
    public GameObject powerUpPrefab;
    public int priority; // if multiple power ups are available, highest priority will be used.
    public int cooldown = 1;

    [Header("Probability")]
    [Range(0f, 1f)]
    public float probability = 1.0f; // Higher = more frequent appearance

    [HideInInspector] public int timesUsed = 0;
    public int lastUsedMove = -1000; // Used for cooldown check

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

