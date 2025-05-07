using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GameOver : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI scoreText;

    private void Start()
    {
        if (scoreText != null)
        {
            scoreText.text = "SCORE: " + ScoreKeeper.finalScore.ToString();
            Debug.Log($"🧾 GameOver UI Loaded — Final Score: {ScoreKeeper.finalScore}");
        }
        else
        {
            Debug.LogWarning("⚠️ GameOver.cs: scoreText reference not assigned.");
        }
    }

    public void OnPlayAgainPressed()
    {
        SceneManager.LoadScene("MainGameScene"); // Use the exact name of your gameplay scene
    }
}
