using System.Collections;
using UnityEngine;
using TMPro;

public class ChallengeMode1 : MonoBehaviour
{
    [Header("Scene Objects")]
    [SerializeField] private GameObject ghost;
    [SerializeField] private GameObject wordBalloon;
    [SerializeField] private GameObject zombieGraphic0;
    [SerializeField] private GameObject zombieGraphic1;
    [SerializeField] private GameObject familesGraphic;
    [SerializeField] private GameObject desktop;
    [SerializeField] private GameObject stateAdvanceButton; // ✅ Renamed
    [SerializeField] private GameObject civilianPhotos;

    [Header("Text Objects")]
    [SerializeField] private TextMeshProUGUI CL1_textObject;
    [SerializeField] private TextMeshProUGUI CL2_textObject;
    [SerializeField] private TextMeshProUGUI CL3_textObject;
    [SerializeField] private TextMeshProUGUI CL4_textObject;

    [Header("Animation Settings")]
    [SerializeField] private float ghostStartY = -6.20f;
    [SerializeField] private float ghostTargetY = -4.72f;
    [SerializeField] private float ghostOvershoot = 0.2f;
    [SerializeField] private float ghostMoveDuration = 1.0f;
    [SerializeField] private float fadeInDuration = 0.25f;
    [SerializeField] private float fadeOutDuration = 0.25f;
    [SerializeField] private float blinkSpeed = 0.2f;

    private SpriteRenderer wordBalloonRenderer;
    private SpriteRenderer zombieGraphic0Renderer;
    private SpriteRenderer zombieGraphic1Renderer;
    private SpriteRenderer familesGraphicRenderer;
    private SpriteRenderer desktopRenderer;
    private SpriteRenderer stateAdvanceButtonRenderer;

    private Coroutine blinkCoroutine;
    private int currentState = 0;

    private void Awake()
    {
        // Get renderers once
        wordBalloonRenderer = wordBalloon.GetComponent<SpriteRenderer>();
        zombieGraphic0Renderer = zombieGraphic0.GetComponent<SpriteRenderer>();
        zombieGraphic1Renderer = zombieGraphic1.GetComponent<SpriteRenderer>();
        familesGraphicRenderer = familesGraphic.GetComponent<SpriteRenderer>();
        desktopRenderer = desktop.GetComponent<SpriteRenderer>();
        stateAdvanceButtonRenderer = stateAdvanceButton.GetComponent<SpriteRenderer>();

        // Ensure clean state
        wordBalloonRenderer.enabled = false;
        zombieGraphic0Renderer.enabled = false;
        zombieGraphic1Renderer.enabled = false;
        familesGraphicRenderer.enabled = false;
        desktopRenderer.enabled = false;
        stateAdvanceButtonRenderer.enabled = false;

        CL1_textObject.gameObject.SetActive(false);
        CL2_textObject.gameObject.SetActive(false);
        CL3_textObject.gameObject.SetActive(false);
        CL4_textObject.gameObject.SetActive(false);

        civilianPhotos.SetActive(false);

        // Place ghost at start position
        Vector3 pos = ghost.transform.position;
        pos.y = ghostStartY;
        ghost.transform.position = pos;
    }

    private void Start()
    {
        PlayChallengeMusic();
        Stage0sequence();
    }

    public void PlayChallengeMusic()
    {
        SFXManager.Instance.PlayMusic("challengeZone1", 0.5f);
        Debug.Log("🎵 Playing challengeZone1 music");
    }

    public void OnAdvanceButtonClicked()
    {
        SFXManager.Instance.Play("buttonBoing");
        Debug.Log("🔊 Played buttonBoing sound");

        // Stop blinking (it will restart in the next stage if needed)
        if (blinkCoroutine != null)
            StopCoroutine(blinkCoroutine);
        stateAdvanceButtonRenderer.enabled = true;

        currentState++;
        RunCurrentState();
    }

    private void RunCurrentState()
    {
        switch (currentState)
        {
            case 1:
                Stage1sequence();
                break;
            case 2:
                Stage2sequence();
                break;
            case 3:
                Stage3sequence();
                break;
            default:
                Debug.Log("🚦 State not implemented: " + currentState);
                break;
        }
    }

    // -------------------- STATES --------------------

    public void Stage0sequence()
    {
        Debug.Log("▶ Stage 0 sequence");
        StartCoroutine(Stage0sequenceCoroutine());
    }

    private IEnumerator Stage0sequenceCoroutine()
    {
        // Animate Ghost
        yield return StartCoroutine(MoveGhostWithOvershoot());

        // Word balloon & text
        wordBalloonRenderer.enabled = true;
        CL1_textObject.gameObject.SetActive(true);

        // Fade in zombieGraphic0
        zombieGraphic0Renderer.enabled = true;
        yield return StartCoroutine(FadeInSprite(zombieGraphic0Renderer));

        // Enable & blink advance button
        stateAdvanceButtonRenderer.enabled = true;
        blinkCoroutine = StartCoroutine(BlinkSprite(stateAdvanceButtonRenderer));
    }

    public void Stage1sequence()
    {
        Debug.Log("▶ Stage 1 sequence");
        StartCoroutine(Stage1sequenceCoroutine());
    }

    private IEnumerator Stage1sequenceCoroutine()
    {
        // Fade out zombieGraphic0
        yield return StartCoroutine(FadeOutToBlack(zombieGraphic0Renderer));

        // Fade in zombieGraphic1
        zombieGraphic1Renderer.enabled = true;
        yield return StartCoroutine(FadeInSprite(zombieGraphic1Renderer));

        // Switch text
        CL1_textObject.gameObject.SetActive(false);
        CL2_textObject.gameObject.SetActive(true);

        // Blink advance button again
        blinkCoroutine = StartCoroutine(BlinkSprite(stateAdvanceButtonRenderer));
    }

    public void Stage2sequence()
    {
        Debug.Log("▶ Stage 2 sequence");
        StartCoroutine(Stage2sequenceCoroutine());
    }

    private IEnumerator Stage2sequenceCoroutine()
    {
        // Fade out zombieGraphic1
        yield return StartCoroutine(FadeOutToBlack(zombieGraphic1Renderer));

        // Fade in familesGraphic
        familesGraphicRenderer.enabled = true;
        yield return StartCoroutine(FadeInSprite(familesGraphicRenderer));

        // Switch text
        CL2_textObject.gameObject.SetActive(false);
        CL3_textObject.gameObject.SetActive(true);

        // Blink advance button again
        blinkCoroutine = StartCoroutine(BlinkSprite(stateAdvanceButtonRenderer));
    }

    public void Stage3sequence()
    {
        Debug.Log("▶ Stage 3 sequence");
        StartCoroutine(Stage3sequenceCoroutine());
    }

    private IEnumerator Stage3sequenceCoroutine()
    {
        // Fade out familesGraphic
        yield return StartCoroutine(FadeOutToBlack(familesGraphicRenderer));

        // Fade in desktop
        desktopRenderer.enabled = true;
        yield return StartCoroutine(FadeInSprite(desktopRenderer));

        // Switch text
        CL3_textObject.gameObject.SetActive(false);
        CL4_textObject.gameObject.SetActive(true);

        // Enable civilianPhotos
        civilianPhotos.SetActive(true);

        // Blink advance button again
        blinkCoroutine = StartCoroutine(BlinkSprite(stateAdvanceButtonRenderer));
    }

    // -------------------- UTILS --------------------

    private IEnumerator MoveGhostWithOvershoot()
    {
        Vector3 startPos = ghost.transform.position;
        Vector3 overshootPos = new Vector3(startPos.x, ghostTargetY + ghostOvershoot, startPos.z);
        Vector3 targetPos = new Vector3(startPos.x, ghostTargetY, startPos.z);

        float t = 0f;
        while (t < ghostMoveDuration)
        {
            t += Time.deltaTime;
            float normalized = t / ghostMoveDuration;
            ghost.transform.position = Vector3.Lerp(startPos, overshootPos, EaseOutCubic(normalized));
            yield return null;
        }

        ghost.transform.position = overshootPos;

        t = 0f;
        float settleDuration = 0.2f;
        while (t < settleDuration)
        {
            t += Time.deltaTime;
            float normalized = t / settleDuration;
            ghost.transform.position = Vector3.Lerp(overshootPos, targetPos, EaseOutCubic(normalized));
            yield return null;
        }

        ghost.transform.position = targetPos;
    }

    private float EaseOutCubic(float x)
    {
        return 1 - Mathf.Pow(1 - x, 3);
    }

    private IEnumerator FadeInSprite(SpriteRenderer renderer)
    {
        Color c = renderer.color;
        c.a = 0;
        renderer.color = c;

        float t = 0f;
        while (t < fadeInDuration)
        {
            t += Time.deltaTime;
            c.a = Mathf.Lerp(0, 1, t / fadeInDuration);
            renderer.color = c;
            yield return null;
        }

        c.a = 1;
        renderer.color = c;
    }

    private IEnumerator FadeOutToBlack(SpriteRenderer renderer)
    {
        Color c = renderer.color;
        Color startColor = c;
        Color endColor = Color.black;
        endColor.a = 1;

        float t = 0f;
        while (t < fadeOutDuration)
        {
            t += Time.deltaTime;
            renderer.color = Color.Lerp(startColor, endColor, t / fadeOutDuration);
            yield return null;
        }

        renderer.color = endColor;
        renderer.enabled = false;
    }

    private IEnumerator BlinkSprite(SpriteRenderer renderer)
    {
        while (true)
        {
            renderer.enabled = !renderer.enabled;
            yield return new WaitForSeconds(blinkSpeed);
        }
    }
}
