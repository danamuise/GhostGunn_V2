using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("GamePlay Score UI")]
    public TextMeshProUGUI scoreFieldUI;

    [Header("Final Score UI")]
    public TextMeshProUGUI finalScoreUI;

    public void UpdateScoreDisplay(int score)
    {
        if (scoreFieldUI != null)
        {
            scoreFieldUI.text = $"Score: {score}";
        }
    }

    public void ShowFinalScore(int finalScore)
    {
        if (finalScoreUI != null)
        {
            finalScoreUI.text = $"Final Score: {finalScore}";
        }
    }

    public void InitializeUI()
    {
        UpdateScoreDisplay(0);
    }
}
