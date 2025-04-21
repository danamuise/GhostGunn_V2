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
            if (pu.powerUpName == "AddBulletPU")
            {
                pu.lastUsedMove = currentMove - 1; // Simulate usage last move
                pu.timesUsed = 1;
            }
            else if (pu.powerUpName == "ProximityBombPU")
            {
                pu.lastUsedMove = currentMove - 3; // Used 3 moves ago
                pu.timesUsed = 1;
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
        PowerUpData selectedPU = GetSelectedPowerUp();

        if (selectedPU == null)
        {
            Debug.Log("❌ No power-up selected to spawn.");
            return;
        }

        Vector2? spawnPos = targetManager.GetAvailablePowerUpPosition(0); // Row 0 = Area 1

        if (spawnPos.HasValue)
        {
            Instantiate(selectedPU.powerUpPrefab, spawnPos.Value, Quaternion.identity, powerUpParent);
            selectedPU.lastUsedMove = currentMove;
            selectedPU.timesUsed++;
            //Debug.Log($"<color=lime>✅ Spawned {selectedPU.powerUpName} at {spawnPos.Value}</color>");
        }
        else
        {
            //Debug.Log($"<color=orange>🟠 No space to spawn {selectedPU.powerUpName} this move.</color>");
        }
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



    public void MovePowerUpsDown(float distance)
    {
        foreach (Transform pu in powerUpParent)
        {
            if (pu == null) continue;
            Vector3 pos = pu.position;
            pu.position = new Vector3(pos.x, pos.y - distance, pos.z);
        }
    }

}
