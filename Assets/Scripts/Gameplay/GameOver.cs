﻿using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GameOver : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI scoreText;

    private void Start()
    {
        SFXManager.Instance.PlayMusic("GameOver", 0.3f);

        // sync to GameState
        //GameState.Instance.CurrentScore = ScoreKeeper.finalScore;
        //GameState.Instance.CheckAndUpdateHighScores();

        if (scoreText != null)
        {
            scoreText.text = ScoreKeeper.finalScore.ToString();
            Debug.Log($"🧾 GameOver UI Loaded — Final Score: {ScoreKeeper.finalScore}");
        }
        else
        {
            Debug.LogWarning("⚠️ GameOver.cs: scoreText reference not assigned.");
        }
    }

    public void OnPlayAgainPressed()
    {
        GameState.Instance.LevelNumber = 1;
        GameState.Instance.ContinueFromLastSave = false;
        SceneManager.LoadScene("MainGameScene"); // play again from the start
    }

    public void OnContinueClicked()
    {
        SceneManager.LoadScene("AdsScene"); // If continuing game after an ad
    }


    public void PlayButtonClickSound()
    {
        if (SFXManager.Instance != null)
        {
            SFXManager.Instance.Play("buttonClick0");
        }
    }
}
