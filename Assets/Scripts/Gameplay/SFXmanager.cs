using UnityEngine;
using System.Collections.Generic;

public class SFXManager : MonoBehaviour
{
    public static SFXManager Instance;
    public static bool isClickingButton = false;
    [Header("Sound UI References")]
    public GameObject soundUI;
    public GameObject moveUIoutButton;
    public GameObject moveUIinButton;

    [Header("SFX & Music Buttons (not used in this snippet, safe to remove if unneeded)")]
    public GameObject soundOffButton;
    public GameObject soundOnButton;
    public GameObject musicOnButton;
    public GameObject musicOffButton;
    private bool isSFXOn = true; // Track sound effect state

    [System.Serializable]
    public class NamedAudioClip
    {
        public string name;
        public AudioClip clip;
    }

    [Header("SFX Library")]
    public List<NamedAudioClip> soundLibrary = new List<NamedAudioClip>();

    [Header("Music Library")]
    public List<NamedAudioClip> musicLibrary = new List<NamedAudioClip>();

    private Dictionary<string, AudioSource> sfxSources = new Dictionary<string, AudioSource>();
    private Dictionary<string, AudioClip> musicClips = new Dictionary<string, AudioClip>();

    private AudioSource musicSource;
    private string lastRandomSFXName = null;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        BuildSFXSources();
        BuildMusicLibrary();

        GameObject musicGO = new GameObject("MusicSource");
        musicGO.transform.SetParent(transform);
        musicSource = musicGO.AddComponent<AudioSource>();
        musicSource.loop = true;
        musicSource.playOnAwake = false;
        musicSource.spatialBlend = 0f;

        // Load saved toggle preferences
        isSFXOn = PlayerPrefs.GetInt("SFX_ENABLED", 1) == 1;
        isMusicOn = PlayerPrefs.GetInt("MUSIC_ENABLED", 1) == 1;

        soundOnButton.SetActive(isSFXOn);
        soundOffButton.SetActive(!isSFXOn);
        musicOnButton.SetActive(isMusicOn);
        musicOffButton.SetActive(!isMusicOn);

        if (!isMusicOn)
        {
            musicSource.Stop();
        }

        moveUIoutButton.SetActive(true);
        moveUIinButton.SetActive(false);
    }


    private void BuildSFXSources()
    {
        foreach (var entry in soundLibrary)
        {
            if (entry.clip == null || string.IsNullOrEmpty(entry.name)) continue;

            if (!sfxSources.ContainsKey(entry.name))
            {
                GameObject child = new GameObject($"SFX_{entry.name}");
                child.transform.SetParent(transform);

                AudioSource source = child.AddComponent<AudioSource>();
                source.clip = entry.clip;
                source.playOnAwake = false;
                source.spatialBlend = 0f;

                sfxSources.Add(entry.name, source);
                Debug.Log($"SFXSource created for: {entry.name}");
            }
        }
    }


    private void BuildMusicLibrary()
    {
        foreach (var entry in musicLibrary)
        {
            if (entry.clip == null || string.IsNullOrEmpty(entry.name)) continue;

            if (!musicClips.ContainsKey(entry.name))
            {
                musicClips.Add(entry.name, entry.clip);
            }
        }
    }

    // 🔊 Play SFX with optional pitch variation

    public void Play(string soundName, float volume = 1f, float pitchMin = 0.95f, float pitchMax = 1.05f)
    {
        if (!isSFXOn) return; // 🔇 Respect SFX toggle

        if (!sfxSources.TryGetValue(soundName, out AudioSource source))
        {
            Debug.LogWarning($"🔇 SFXManager: No AudioSource found for '{soundName}'");
            return;
        }

        source.pitch = Random.Range(pitchMin, pitchMax);
        source.volume = volume;
        source.PlayOneShot(source.clip);

        Debug.Log($"🎧 Playing SFX: {soundName}");
    }



    public void PlayRandom(string[] soundNames, float volume = 1f, float pitchMin = 0.95f, float pitchMax = 1.05f)
    {
        if (!isSFXOn || soundNames == null || soundNames.Length == 0) return;

        string chosen;
        int attempts = 0;

        do
        {
            chosen = soundNames[Random.Range(0, soundNames.Length)];
            attempts++;
        } while (chosen == lastRandomSFXName && attempts < 5);

        lastRandomSFXName = chosen;
        Play(chosen, volume, pitchMin, pitchMax);
    }


    // 🎵 Play music by name
    public void PlayMusic(string musicName, float volume = 1f)
    {
        if (!musicClips.TryGetValue(musicName, out AudioClip clip))
        {
            Debug.LogWarning($"🎵 SFXManager: No music clip found for '{musicName}'");
            return;
        }

        if (musicSource.clip == clip && musicSource.isPlaying)
            return; // Already playing

        musicSource.clip = clip;
        musicSource.volume = volume;
        musicSource.Play();
    }

    public void StopMusic()
    {
        musicSource.Stop();
    }

    public void FadeOutMusic(float duration = 1f)
    {
        StartCoroutine(FadeMusicOut(duration));
    }

    private System.Collections.IEnumerator FadeMusicOut(float duration)
    {
        float startVol = musicSource.volume;
        float time = 0f;

        while (time < duration)
        {
            musicSource.volume = Mathf.Lerp(startVol, 0f, time / duration);
            time += Time.deltaTime;
            yield return null;
        }

        musicSource.Stop();
        musicSource.volume = startVol; // Reset volume
    }

    // --- UI MOVE BUTTONS ---

    public void MoveUIOut()
    {
        Debug.Log("MoveUIOut() called!");
        StartCoroutine(MoveSoundUI(2.606f, 1.69f));
        moveUIoutButton.SetActive(false);
        moveUIinButton.SetActive(true);

        // Schedule auto-close after 10 seconds
        Invoke(nameof(MoveUIIn), 10f);
    }

    public void MoveUIIn()
    {
        Debug.Log("MoveUIIn() called!");
        StartCoroutine(MoveSoundUI(1.69f, 2.606f));
        moveUIinButton.SetActive(false);
        moveUIoutButton.SetActive(true);

        // Cancel the auto-close in case it was scheduled
        CancelInvoke(nameof(MoveUIIn));
    }


    private System.Collections.IEnumerator MoveSoundUI(float startX, float endX, float duration = 0.3f)
    {
        Vector3 startPos = new Vector3(startX, soundUI.transform.position.y, soundUI.transform.position.z);
        Vector3 endPos = new Vector3(endX, soundUI.transform.position.y, soundUI.transform.position.z);

        float time = 0f;
        while (time < 1f)
        {
            time += Time.deltaTime / duration;
            float t = Mathf.Pow(time, 2); // ease-in
            soundUI.transform.position = Vector3.Lerp(startPos, endPos, t);
            yield return null;
        }
        soundUI.transform.position = endPos;
    }

    private bool isMusicOn = true; // Track music state ourselves

    public void ToggleMusic()
    {
        isMusicOn = !isMusicOn;
        PlayerPrefs.SetInt("MUSIC_ENABLED", isMusicOn ? 1 : 0);
        PlayerPrefs.Save();

        if (isMusicOn)
        {
            if (musicSource.clip != null)
            {
                musicSource.Play();
                Debug.Log("🎵 Music ON");
            }
            else
            {
                Debug.LogWarning("🎵 No music clip assigned to resume playback!");
            }
        }
        else
        {
            musicSource.Stop();
            Debug.Log("🔇 Music OFF");
        }

        musicOnButton.SetActive(isMusicOn);
        musicOffButton.SetActive(!isMusicOn);
        MoveUIIn();
    }

    public void ToggleSFX()
    {
        if (isSFXOn)
        {
            Debug.Log("🔇 Sound OFF");
            soundOnButton.SetActive(false);
            soundOffButton.SetActive(true);
            isSFXOn = false;
            PlayerPrefs.SetInt("SFX_ENABLED", 0);
        }
        else
        {
            Debug.Log("🔊 Sound ON");
            soundOnButton.SetActive(true);
            soundOffButton.SetActive(false);
            isSFXOn = true;
            PlayerPrefs.SetInt("SFX_ENABLED", 1);
        }

        PlayerPrefs.Save();
        MoveUIIn();
    }

    /// <summary>
    /// TESTING ONLY - DELETE AFTER USE
    /// </summary>
    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 worldPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(worldPoint, Vector2.zero);

            if (hit.collider != null)
            {
                Debug.Log($"🖱 Click hit: {hit.collider.gameObject.name}");
            }
            else
            {
                Debug.Log("🖱 Click hit: nothing");
            }
        }
    }


}
