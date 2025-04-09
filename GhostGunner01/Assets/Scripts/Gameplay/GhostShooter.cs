using UnityEngine;
using System.Collections.Generic;

public class GhostShooter : MonoBehaviour
{
    [Header("Bullet Settings")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float bulletSpeed = 10f;
    public BulletPool bulletPool;

    [Header("Trajectory Dot Settings")]
    public GameObject dotPrefab;
    public int dotCount = 20;
    public float dotSpacing = 0.1f;
    public float minDotScale = 0.5f;
    public float maxDotScale = 1f;
    public float minAlpha = 0.1f;
    public Gradient dotColorGradient;

    [Header("Trajectory Collision")]
    public LayerMask wallMask;

    private List<GameObject> dots = new List<GameObject>();
    private List<SpriteRenderer> renderers = new List<SpriteRenderer>();

    private Vector2 aimTarget;
    private bool isAiming = false;
    private bool bulletReady = true;

    private BouncingBullet currentBullet; // 👻 The one true ghost

    void Start()
    {
        // Setup dot preview objects
        for (int i = 0; i < dotCount; i++)
        {
            GameObject dot = Instantiate(dotPrefab, transform);
            dot.SetActive(false);
            dots.Add(dot);
            renderers.Add(dot.GetComponent<SpriteRenderer>());
        }

        // Spawn ghost bullet at fire point and keep it idle
        GameObject bullet = bulletPool.GetBullet();
        currentBullet = bullet.GetComponent<BouncingBullet>();
        currentBullet.SetShooter(this);
        currentBullet.PrepareAtHome(firePoint.position); // idle ghost at start
    }

    void Update()
    {
#if UNITY_EDITOR
        HandleMouseInput();
#else
        HandleTouchInput();
#endif
    }

    void HandleMouseInput()
    {
        if (Input.GetMouseButtonDown(0) && bulletReady)
            isAiming = true;

        if (Input.GetMouseButton(0) && isAiming)
        {
            aimTarget = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 velocity = (aimTarget - (Vector2)firePoint.position).normalized * bulletSpeed;
            ShowTrajectory(firePoint.position, velocity);
        }

        if (Input.GetMouseButtonUp(0) && isAiming)
        {
            FireBullet();
            HideTrajectory();
            isAiming = false;
        }
    }

    void HandleTouchInput()
    {
        if (Input.touchCount > 0)
        {
            Touch t = Input.GetTouch(0);

            if (t.phase == TouchPhase.Began && bulletReady)
                isAiming = true;

            else if ((t.phase == TouchPhase.Moved || t.phase == TouchPhase.Stationary) && isAiming)
            {
                aimTarget = Camera.main.ScreenToWorldPoint(t.position);
                Vector2 velocity = (aimTarget - (Vector2)firePoint.position).normalized * bulletSpeed;
                ShowTrajectory(firePoint.position, velocity);
            }

            else if (t.phase == TouchPhase.Ended && isAiming)
            {
                FireBullet();
                HideTrajectory();
                isAiming = false;
            }
        }
    }

    void FireBullet()
    {
        if (!bulletReady || currentBullet == null) return;

        bulletReady = false;

        Vector2 direction = (aimTarget - (Vector2)firePoint.position).normalized;
        currentBullet.Fire(firePoint.position, direction);
    }

    public void NotifyBulletReady()
    {
        bulletReady = true;
    }

    void ShowTrajectory(Vector2 startPos, Vector2 initialVelocity)
    {
        Vector2 currentPos = startPos;
        Vector2 currentDir = initialVelocity.normalized;
        float speed = initialVelocity.magnitude;
        int dotIndex = 0;
        int bounceLimit = 5;

        for (int bounce = 0; bounce < bounceLimit && dotIndex < dotCount; bounce++)
        {
            RaycastHit2D hit = Physics2D.Raycast(currentPos, currentDir, Mathf.Infinity, wallMask);
            float distanceToDraw = hit.collider != null ? hit.distance : speed * dotSpacing * (dotCount - dotIndex);
            int dotsThisSegment = Mathf.Min(dotCount - dotIndex, Mathf.FloorToInt(distanceToDraw / (speed * dotSpacing)));

            for (int i = 0; i < dotsThisSegment; i++)
            {
                float dist = i * speed * dotSpacing;
                Vector2 pos = currentPos + currentDir * dist;

                GameObject dot = dots[dotIndex];
                float waveOffset = Mathf.Sin(Time.time * 4f + dotIndex * 0.3f) * 0.05f;
                dot.transform.position = pos + Vector2.up * waveOffset;
                dot.SetActive(true);

                float t = (float)dotIndex / dotCount;
                Color c = dotColorGradient.Evaluate(t);
                c.a = Mathf.Max(c.a * Mathf.Lerp(1f, minAlpha, t), minAlpha);
                renderers[dotIndex].color = c;

                float scale = Mathf.Lerp(maxDotScale, minDotScale, t);
                dot.transform.localScale = Vector3.one * scale;

                dotIndex++;
            }

            if (hit.collider == null) break;

            currentPos = hit.point + hit.normal * 0.01f;
            currentDir = Vector2.Reflect(currentDir, hit.normal);
        }

        for (int i = dotIndex; i < dots.Count; i++)
        {
            dots[i].SetActive(false);
        }
    }

    void HideTrajectory()
    {
        foreach (var dot in dots)
            dot.SetActive(false);
    }
}
