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

}
