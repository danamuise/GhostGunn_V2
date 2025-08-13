//FOR TESTING ONLY. Do not include in project

using UnityEngine;
using UnityEngine.SceneManagement;

public class DevStartLevel2 : MonoBehaviour
{
    [Header("Mock Game State")]
    public int mockScore = 2000;
    public int mockHealth = 3;
    public int mockBulletCount = 6;

    [Header("Startup Settings")]
    public bool autoStart = true;

    private void Start()
    {
        if (autoStart)
        {
            StartLevel2WithMockData();
        }
    }

    public void StartLevel2WithMockData()
    {
        GameState.Instance.CurrentScore = mockScore;
        GameState.Instance.SavedTargetHealth = mockHealth;
        GameState.Instance.SavedBulletCount = mockBulletCount;
        GameState.Instance.LevelNumber = 2;
        GameState.Instance.ContinueFromLastSave = true;

        Debug.Log($"🚀 Starting Level 2 with mock data — Score: {mockScore}, Health: {mockHealth}, Bullets: {mockBulletCount}");
        SceneManager.LoadScene("MainGameScene");
    }
}
