#if UNITY_EDITOR || DEVELOPMENT_BUILD
// FOR TESTING ONLY. Compiles only in Editor or Development builds.
// Put this in its own file: GameSpeedTester.cs

using UnityEngine;

public class GameSpeedTester : MonoBehaviour
{
    [Header("Game Speed")]
    [Tooltip("0.0 = paused, 1.0 = normal, >1.0 = faster")]
    [Range(0f, 3f)] public float gameSpeed = 1f;

    [Tooltip("Apply gameSpeed automatically on Start/Play.")]
    public bool applyOnStart = true;

    [Tooltip("Keep this object when loading new scenes.")]
    public bool persistBetweenScenes = true;

    [Tooltip("Reset time scale to 1x when this component is disabled or destroyed.")]
    public bool autoResetOnDisable = true;

    [Header("Runtime Controls")]
    [Tooltip("Enable dev hotkeys: '-' slower, '=' faster, '`' reset to 1x, '0' pause, '1' normal")]
    public bool hotkeysEnabled = true;

    [Tooltip("How much to change per hotkey press.")]
    public float step = 0.1f;

    [Tooltip("Show a tiny on-screen slider (toggle with F1) in dev builds.")]
    public bool showOverlayByDefault = false;

    [Tooltip("Pause AudioListener when gameSpeed == 0.")]
    public bool pauseAudioWhenPaused = true;

    private float _lastApplied = -1f;
    private bool _overlayVisible;

    private void Awake()
    {
        if (persistBetweenScenes) DontDestroyOnLoad(gameObject);
        _overlayVisible = showOverlayByDefault;
    }

    private void Start()
    {
        if (applyOnStart) ApplyGameSpeed();
    }

    private void Update()
    {
        if (hotkeysEnabled)
        {
            if (Input.GetKeyDown(KeyCode.Minus) || Input.GetKeyDown(KeyCode.Underscore))
                SetGameSpeed(gameSpeed - step);

            if (Input.GetKeyDown(KeyCode.Equals) || Input.GetKeyDown(KeyCode.Plus))
                SetGameSpeed(gameSpeed + step);

            if (Input.GetKeyDown(KeyCode.BackQuote)) // `
                SetGameSpeed(1f);

            if (Input.GetKeyDown(KeyCode.Alpha0))
                SetGameSpeed(0f);

            if (Input.GetKeyDown(KeyCode.Alpha1))
                SetGameSpeed(1f);

            if (Input.GetKeyDown(KeyCode.F1))
                _overlayVisible = !_overlayVisible;
        }

        // If tweaked in the Inspector at runtime, re-apply automatically
        if (Mathf.Abs(gameSpeed - _lastApplied) > 0.0001f)
            ApplyGameSpeed();
    }

    public void SetGameSpeed(float value)
    {
        gameSpeed = Mathf.Clamp(value, 0f, 3f);
        ApplyGameSpeed();
    }

    private void ApplyGameSpeed()
    {
        Time.timeScale = gameSpeed;
        // Keep physics step coherent with time scale (default fixedDeltaTime is 0.02)
        Time.fixedDeltaTime = 0.02f * Time.timeScale;

        if (pauseAudioWhenPaused)
            AudioListener.pause = (gameSpeed <= 0f);

        _lastApplied = gameSpeed;
        Debug.Log($"⏱️ Game speed: {gameSpeed:0.00}x (timeScale={Time.timeScale:0.00}, fixedDT={Time.fixedDeltaTime:0.000})");
    }

    private void OnDisable()
    {
        if (!autoResetOnDisable) return;
        ResetToNormal();
    }

    private void OnDestroy()
    {
        if (!autoResetOnDisable) return;
        ResetToNormal();
    }

    private void ResetToNormal()
    {
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;
        if (pauseAudioWhenPaused) AudioListener.pause = false;
        _lastApplied = 1f;
        Debug.Log("⏱️ Game speed reset to 1.00x");
    }

    // Minimal IMGUI overlay for quick tweaks in dev builds
    private void OnGUI()
    {
        if (!_overlayVisible) return;

        const int w = 260, h = 90;
        var rect = new Rect(10, 10, w, h);
        GUI.Box(rect, "Game Speed Tester");

        GUILayout.BeginArea(new Rect(20, 35, w - 20, h - 40));
        GUILayout.Label($"Speed: {gameSpeed:0.00}x");
        float newVal = GUILayout.HorizontalSlider(gameSpeed, 0f, 3f, GUILayout.Width(w - 40));
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Pause")) newVal = 0f;
        if (GUILayout.Button("1x")) newVal = 1f;
        if (GUILayout.Button("+")) newVal = Mathf.Min(3f, gameSpeed + step);
        GUILayout.EndHorizontal();
        GUILayout.EndArea();

        if (!Mathf.Approximately(newVal, gameSpeed))
            SetGameSpeed(newVal);
    }
}
#endif
