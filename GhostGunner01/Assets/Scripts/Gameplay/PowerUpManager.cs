using UnityEngine;

public class PowerUpManager : MonoBehaviour
{
    [Header("Power-Up Prefabs")]
    public GameObject[] powerUpPrefabs;  // 0: AddBulletPU, others later

    [Header("Scene References")]
    public Transform powerUpParent;      // Assign the 'PowerUps' GO in scene


    void Start()
    {

    }


    // Manual spawn method (used for test or future logic)
    public void TrySpawnPowerUp(int prefabIndex, Vector2 spawnPosition)
    {
        if (powerUpPrefabs == null || prefabIndex < 0 || prefabIndex >= powerUpPrefabs.Length)
        {
            Debug.LogWarning($"⚠️ Invalid prefab index {prefabIndex} — cannot spawn PowerUp.");
            return;
        }

        if (powerUpParent == null)
        {
            Debug.LogWarning("⚠️ PowerUpParent not assigned in PowerUpManager.");
            return;
        }

        GameObject prefab = powerUpPrefabs[prefabIndex];
        GameObject instance = Instantiate(prefab, spawnPosition, Quaternion.identity, powerUpParent);
        Debug.Log($"✅ Spawned PowerUp: {prefab.name} at {spawnPosition} under {powerUpParent.name}");
    }
}
