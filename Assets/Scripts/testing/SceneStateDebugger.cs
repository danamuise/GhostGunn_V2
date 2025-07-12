using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneStateDebugger : MonoBehaviour
{
    private static SceneStateDebugger instance;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        if (Input.anyKeyDown) Debug.Log("🔑 Some key was pressed");

        if (Input.GetKeyDown(KeyCode.S))
        {
            Debug.Log(" S key pressed");
            GameState.Instance.SaveState();
        }

        if (Input.GetKeyDown(KeyCode.L))
        {
            Debug.Log(" L key pressed");
            GameState.Instance.LoadState();
        }
    }
}
