using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GhostShooter : MonoBehaviour
{
    [Header("Bullet Settings")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float bulletSpeed = 10f;
    public BulletPool bulletPool;

    [Header("Ghost Tank")]
    public Transform ghostTank;
    public int ghostCount = 10;
    public float ghostSpacingTime = 0.1f;
    public Vector2 firePointOffset = Vector2.zero;

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

    [Header("Managers")]
    public TargetManager targetManager;

    private List<BouncingBullet> tankGhosts = new List<BouncingBullet>();
    private List<BouncingBullet> activeGhosts = new List<BouncingBullet>();
    private List<GameObject> dots = new List<GameObject>();
    private List<SpriteRenderer> renderers = new List<SpriteRenderer>();

    private Vector2 aimTarget;
    private bool isAiming = false;
    private bool isShotInProgress = false;
    private bool canShoot = true;

    void Start()
    {
        for (int i = 0; i < dotCount; i++)
        {
            GameObject dot = Instantiate(dotPrefab, transform);
            dot.SetActive(false);
            dots.Add(dot);
            renderers.Add(dot.GetComponent<SpriteRenderer>());
        }

        for (int i = 0; i < ghostCount; i++)
        {
            GameObject ghost = bulletPool.GetBullet();
            BouncingBullet script = ghost.GetComponent<BouncingBullet>();
            script.SetShooter(this);
            script.SetGhostTank(ghostTank);

            Vector2 offset = new Vector2(Random.Range(-0.6f, 0.6f), Random.Range(-0.2f, 0.3f));
            ghost.transform.position = ghostTank.position + (Vector3)offset;
            script.PrepareAtHome(ghost.transform.position);

            tankGhosts.Add(script);
        }

        targetManager.SpawnTargetsInArea1();
    }

    void Update()
    {
        if (!canShoot || isShotInProgress) return;

#if UNITY_EDITOR
        HandleMouseInput();
#else
        HandleTouchInput();
#endif
    }

    void HandleMouseInput()
    {
        if (Input.GetMouseButtonDown(0)) isAiming = true;

        if (Input.GetMouseButton(0) && isAiming)
        {
            aimTarget = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 firePos = (Vector2)firePoint.position + firePointOffset;
            Vector2 velocity = (aimTarget - firePos).normalized * bulletSpeed;
            ShowTrajectory(firePos, velocity);
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
        if (Input.touchCount == 0) return;

        Touch t = Input.GetTouch(0);
        if (t.phase == TouchPhase.Began) isAiming = true;

        else if ((t.phase == TouchPhase.Moved || t.phase == TouchPhase.Stationary) && isAiming)
        {
            aimTarget = Camera.main.ScreenToWorldPoint(t.position);
            Vector2 firePos = (Vector2)firePoint.position + firePointOffset;
            Vector2 velocity = (aimTarget - firePos).normalized * bulletSpeed;
            ShowTrajectory(firePos, velocity);
        }

        else if (t.phase == TouchPhase.Ended && isAiming)
        {
            FireBullet();
            HideTrajectory();
            isAiming = false;
        }
    }

    void FireBullet()
    {
        if (isShotInProgress || tankGhosts.Count == 0) return;

        isShotInProgress = true;
        StartCoroutine(FireGhostsFromTank());
    }

    IEnumerator FireGhostsFromTank()
    {
        Vector2 dir = (aimTarget - ((Vector2)firePoint.position + firePointOffset)).normalized;
        activeGhosts.Clear();

        while (tankGhosts.Count > 0)
        {
            BouncingBullet ghost = tankGhosts[0];
            tankGhosts.RemoveAt(0);

            Vector2 firePos = (Vector2)firePoint.position + firePointOffset;
            ghost.Fire(firePos, dir);

            activeGhosts.Add(ghost);
            yield return new WaitForSeconds(ghostSpacingTime);
        }
    }

    public void NotifyBulletReady(BouncingBullet ghost)
    {
        activeGhosts.Remove(ghost);
        tankGhosts.Add(ghost);

        Vector2 offset = new Vector2(Random.Range(-0.6f, 0.6f), Random.Range(-0.2f, 0.3f));
        ghost.transform.position = ghostTank.position + (Vector3)offset;

        if (activeGhosts.Count == 0)
        {
            isShotInProgress = false;
            StartCoroutine(MoveTargetsAndRespawn());
        }
    }

    IEnumerator MoveTargetsAndRespawn()
    {
        if (targetManager == null) yield break;

        yield return StartCoroutine(targetManager.MoveTargetsDown());

        if (targetManager.CheckForGameOver())
        {
            Debug.Log("Game Over!");
            DisableGun();
            yield break;
        }

        targetManager.SpawnTargetsInArea1();
    }

    public void EnableGun() => canShoot = true;
    public void DisableGun() => canShoot = false;

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
            dots[i].SetActive(false);
    }

    void HideTrajectory()
    {
        foreach (var dot in dots)
            dot.SetActive(false);
    }
}
