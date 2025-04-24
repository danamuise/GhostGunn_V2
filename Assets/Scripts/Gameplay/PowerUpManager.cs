using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUpManager : MonoBehaviour
{
    [Header("Scene References")]
    public Transform powerUpParent; // Assign the 'PowerUps' GO in scene
    public List<PowerUpData> powerUpList; // Assign in Inspector

    private int currentMove = 0;
    void Start()
    {
        // 🔧 TEMP: Simulate power-ups having been used recently
        foreach (var pu in powerUpList)
        {
            pu.ResetRuntimeState();
            if (pu.powerUpName == "AddBulletPU")
            {
                pu.lastUsedMove = currentMove - 1; // Simulate usage last move
                //pu.timesUsed = 1;
            }
            else if (pu.powerUpName == "ProximityBombPU")
            {
                pu.lastUsedMove = currentMove - 3; // Used 3 moves ago
               // pu.timesUsed = 1;
            }

        }
    }
    public void OnNewMove(int moveNumber)
    {
        currentMove = moveNumber;
        Debug.Log($"<color=green>🔁 PowerUpManager | Move number: {currentMove}</color>");

        DebugCheckAvailablePowerUps(currentMove); // 🔍 Hook the debug check here
    }


    public int GetCurrentMove()
    {
        return currentMove;
    }

    public void DebugCheckAvailablePowerUps(int move)
    {
        foreach (var pu in powerUpList)
        {
            bool isValid = pu.IsAvailable(move);
            Debug.Log($"<color={(isValid ? "green" : "red")}>" +
                $"[Check] {pu.powerUpName} | Move: {move} | " +
                $"Used: {pu.timesUsed} | LastUsed: {pu.lastUsedMove} | " +
                $"Cooldown: {pu.cooldown} | Prob Roll: {(isValid ? "✅ PASS" : "❌ FAIL")}</color>");
        }
    }
    public void TrySpawnSelectedPowerUp(TargetManager targetManager)
    {
       /*
        PowerUpData selectedPU = GetSelectedPowerUp();

        if (selectedPU == null)
        {
            Debug.Log("❌ No power-up selected to spawn.");
            return;
        }

        Vector2? spawnPos = targetManager.GetAvailablePowerUpPosition(0); // Row 0 = Area 1

        if (spawnPos.HasValue)
        {
            // Instantiate off-screen and animate it down
            GameObject puInstance = Instantiate(selectedPU.powerUpPrefab, spawnPos.Value, Quaternion.identity, powerUpParent);
            AnimatePowerUpToPosition(puInstance.transform, spawnPos.Value); // 👈 animate it down

            selectedPU.lastUsedMove = currentMove;
            selectedPU.timesUsed++;

            Debug.Log($"<color=lime>✅ Spawned and animating {selectedPU.powerUpName} at {spawnPos.Value}</color>");
        }
        else
        {
            Debug.Log($"<color=orange>🟠 No space to spawn {selectedPU.powerUpName} this move.</color>");
        }
       */
    }


    private PowerUpData GetSelectedPowerUp()
    {
        List<PowerUpData> availablePUs = new List<PowerUpData>();

        foreach (var pu in powerUpList)
        {
            bool available = pu.IsAvailable(currentMove);
            //Debug.Log($"🔍 {pu.powerUpName} | Available: {available} | Move: {currentMove} | " +
             //         $"LastUsed: {pu.lastUsedMove} | Cooldown: {pu.cooldown} | " +
              //        $"TimesUsed: {pu.timesUsed} | Probability: {pu.probability}");

            if (available)
                availablePUs.Add(pu);
        }

        if (availablePUs.Count == 0)
        {
            //Debug.Log($"<color=cyan>✨ availablePUs.Count: {availablePUs.Count}</color>");
            return null;
        }

       // Debug.Log($"✅ {availablePUs.Count} power-up(s) available. Proceeding to sort by priority...");

        // Sort by priority ascending (lower number = higher priority)
        availablePUs.Sort((a, b) =>
        {
            //Debug.Log($"🔍 Comparing priority: {a.powerUpName} ({a.priority}) vs {b.powerUpName} ({b.priority})");
            return a.priority.CompareTo(b.priority);
        });

        PowerUpData selected = availablePUs[0];
        //Debug.Log($"<color=yellow>✨ PU selected: {selected.powerUpName} with priority {selected.priority}</color>");
        return selected;
    }

    public void MovePowerUpsDown(float distance, float duration = 0.35f, float ease = 2.5f)
    {
        foreach (Transform pu in powerUpParent)
        {
            if (pu == null) continue;

            Vector3 start = pu.position;
            Vector3 end = new Vector3(start.x, start.y - distance, start.z);

            StartCoroutine(AnimatePU(pu, start, end, duration, ease));
        }
    }

    public void AnimatePowerUpToPosition(Transform powerUp, Vector2 finalPos, float duration = 0.35f, float ease = 2.5f)
    {
        //Debug.LogError($"❌ finalPos passed to PositionX: "+ finalPos.x + ", PositionY: "+ finalPos.y );
        if (float.IsNaN(finalPos.x) || float.IsNaN(finalPos.y))
        {
            Debug.LogError($"❌ Invalid finalPos passed to AnimatePowerUpToPosition: {finalPos}");
            return;
        }

        Vector2 startPos = new Vector2(finalPos.x, finalPos.y + 1.0f); // spawn above target row
        powerUp.position = startPos;

        StartCoroutine(AnimatePU(powerUp, startPos, finalPos, duration, ease));
    }

    private IEnumerator AnimatePU(Transform pu, Vector2 start, Vector2 end, float duration, float ease)
    {
        if (float.IsNaN(start.x) || float.IsNaN(start.y) || float.IsNaN(end.x) || float.IsNaN(end.y))
        {
            Debug.LogError($"❌ AnimatePU received NaN input: start={start}, end={end}");
            yield break;
        }

        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            t = Mathf.Clamp01(t); // 👈 fixes easing NaN

            float eased = 1f - Mathf.Pow(1f - t, ease);

            Vector2 interpolated = Vector2.Lerp(start, end, eased);

            if (float.IsNaN(interpolated.x) || float.IsNaN(interpolated.y))
            {
                Debug.LogError($"❌ AnimatePU failed: interpolated position is NaN — start={start}, end={end}, eased={eased}");
                yield break;
            }

            pu.position = interpolated;
            yield return null;
        }

        pu.position = end; // Final snap to ensure clean finish
    }


}
