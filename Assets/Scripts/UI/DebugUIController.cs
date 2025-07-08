using UnityEngine;
using TMPro;

public class DebugUIController : MonoBehaviour
{
    public TMP_Text debugText;

    void Update()
    {
        if (debugText == null || GameState.Instance == null) return;

        debugText.text =
            $"Level: {GameState.Instance.CurrentLevel}\n" +
            $"Special Weapons: {GameState.Instance.AvailableSpecialWeapons}\n\n" +
            $"Highest Score: {GameState.Instance.HighScores[0]}";
    }
}
