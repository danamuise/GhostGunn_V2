using UnityEngine;

public class PowerUpManager : MonoBehaviour
{
    [Header("Power-Up Prefabs")]
    public GameObject[] powerUpPrefabs;  // 0: AddBulletPU, others later

    [Header("Scene References")]
    public Transform powerUpParent;      // Assign the 'PowerUps' GO in scene

    //[Header("Test Spawn Settings")]
    ///public Vector2 testSpawnPosition = new Vector2(0f, 3f);  // Area 1 test position
    //public int testPrefabIndex = 0;  // 0 = AddBulletPU

    void Start()
    {
        // TEMP: Manual test spawn of AddBulletPU (optional for dev-only verification)
        //TrySpawnPowerUp(testPrefabIndex, testSpawnPosition);
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
