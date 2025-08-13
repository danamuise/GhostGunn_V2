using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class AdsSceneLoader : MonoBehaviour
{
    private void Start()
    {
        StartCoroutine(WaitAndLoadMainGame());
    }

    private IEnumerator WaitAndLoadMainGame()
    {
        yield return new WaitForSeconds(3f);

        GameState.Instance.ContinueFromLastSave = true;

        Debug.Log("🎬 Ad finished. Setting ContinueFromLastSave = true and loading MainGameScene…");
        SceneManager.LoadScene("MainGameScene");
    }
}
