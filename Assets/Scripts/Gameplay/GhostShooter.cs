using System.Collections.Generic;
using UnityEngine;
using System.Collections;
public class GhostShooter : MonoBehaviour
{
    public BulletPool bulletPool;
    public Transform firePoint;
    public LaserTrajectoryPreview trajectoryPreview;
    public Transform gunBase;
    public Transform gunBarrel;
    public GhostTankUI ghostTankUI;
    private Coroutine recoilCoroutine;
    private bool canShoot = true;
    private bool justLoaded = true;

    void OnEnable()
    {
        StartCoroutine(ResetJustLoaded());
    }
    void Update()
    {
        // 🛑 Prevent input if clicking on UI
        if (UnityEngine.EventSystems.EventSystem.current != null &&
            UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
            return;

#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBGL
        Vector3 inputPos = Input.mousePosition;
        bool inputDown = Input.GetMouseButton(0);
        bool inputUp = Input.GetMouseButtonUp(0);
#else
    if (Input.touchCount == 0) return;
    Touch touch = Input.GetTouch(0);
    Vector3 inputPos = touch.position;
    bool inputDown = touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary;
    bool inputUp = touch.phase == TouchPhase.Ended;
#endif

        Vector3 worldPoint = Camera.main.ScreenToWorldPoint(inputPos);
        worldPoint.z = 0f;

        // 🔁 Always aim the gun
        Vector2 rawDirection = (worldPoint - firePoint.position).normalized;
        float angle = Mathf.Atan2(rawDirection.y, rawDirection.x) * Mathf.Rad2Deg;
        angle = Mathf.Clamp(angle, 10f, 170f);

        if (gunBase != null)
            gunBase.rotation = Quaternion.Euler(0f, 0f, angle - 90f);

        // 🔒 Only allow trajectory and firing if we’re allowed to shoot AND all bullets are in tank
        if (!canShoot || !AllBulletsAreInTank())
        {
            trajectoryPreview?.ClearDots();
            return;
        }

        float clampedRad = angle * Mathf.Deg2Rad;
        Vector2 direction = new Vector2(Mathf.Cos(clampedRad), Mathf.Sin(clampedRad));

        if (inputDown)
        {
            trajectoryPreview?.DrawLaserLine(firePoint.position, direction);
        }

        if (inputUp)
        {
            trajectoryPreview?.ClearDots();
            StartCoroutine(FireAllBullets(direction));
            DisableGun();
        }
    }


    public void EnableGun(bool enable)
    {
        canShoot = enable;
    }

    public void DisableGun()
    {
        canShoot = false;
    }

    private bool AllBulletsAreInTank()
    {
        int inTank = 0;
        int total = 0;

        foreach (GameObject bulletGO in bulletPool.GetAllBullets())
        {
            if (!bulletGO.activeInHierarchy) continue;

            total++;
            GhostBullet bullet = bulletGO.GetComponent<GhostBullet>();
            if (bullet != null && bullet.IsInTank)
                inTank++;
        }

        //Debug.Log($"🔎 AllBulletsAreInTank(): {inTank} in tank / {total} total");

        return inTank == total;
    }

    private IEnumerator FireAllBullets(Vector2 direction)
    {
        List<GameObject> bullets = bulletPool.GetAllBullets();

        int totalInTank = bulletPool.GetTankedBulletCount();
        Debug.Log($"🔫 Starting FireAllBullets() — {totalInTank} ghosts in tank");

        // Optional initial HUD refresh
        ghostTankUI?.SetBulletCounts(totalInTank, bulletPool.GetEnabledBulletCount());

        SFXManager.Instance.Play("GhostSound", 0.5f, 0.9f, 1.1f);

        int fired = 0;
        foreach (GameObject bulletGO in bullets)
        {
            if (!bulletGO.activeInHierarchy) continue;

            GhostBullet bullet = bulletGO.GetComponent<GhostBullet>();
            if (bullet != null && bullet.IsInTank)
            {
                bulletGO.transform.position = firePoint.position;
                bulletGO.SetActive(true);
                Vector2 randomized = AddSpreadToDirection(direction, 2.4f);
                bullet.Fire(randomized);

                SFXManager.Instance.Play("BulletShoot", 0.5f, 0.9f, 1.1f);
                TriggerRecoil();

                Debug.Log($"🚀 Fired bullet: {bulletGO.name} | time: {Time.time:F2}");

                fired++;

                // ✅ Delayed HUD update for smoother number change
                StartCoroutine(DelayedHUDUpdate(0.15f));

                yield return new WaitForSeconds(0.1f); // original firing delay remains
            }
        }

        // Final HUD sync to ensure it's accurate
        int remaining = bulletPool.GetTankedBulletCount();
        ghostTankUI?.SetBulletCounts(remaining, bulletPool.GetEnabledBulletCount());

        Debug.Log($"✅ FireAllBullets() complete — Fired: {fired}, Remaining in tank: {remaining}");
    }


    private Vector2 AddSpreadToDirection(Vector2 baseDirection, float maxAngleDegrees)
    {
        //Debug.Log($"🚀 ADDING SPREAD TO DIRECTION);
        float angle = Random.Range(-maxAngleDegrees, maxAngleDegrees);
        float radians = angle * Mathf.Deg2Rad;

        // Rotate the direction vector by a small angle
        float cos = Mathf.Cos(radians);
        float sin = Mathf.Sin(radians);

        float newX = baseDirection.x * cos - baseDirection.y * sin;
        float newY = baseDirection.x * sin + baseDirection.y * cos;

        return new Vector2(newX, newY).normalized;
    }

    private void TriggerRecoil()
    {
        if (recoilCoroutine != null)
            StopCoroutine(recoilCoroutine);

        recoilCoroutine = StartCoroutine(PlayRecoil());
    }

    private IEnumerator PlayRecoil()
    {
        // Instantly recoil
        Vector3 startPos = gunBarrel.localPosition;
        gunBarrel.localPosition = new Vector3(startPos.x, 0.33f, startPos.z);

        float returnDuration = 0.06f; // fast return
        float elapsed = 0f;

        while (elapsed < returnDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / returnDuration;
            float y = Mathf.Lerp(0.33f, 0.49f, t);
            gunBarrel.localPosition = new Vector3(startPos.x, y, startPos.z);
            yield return null;
        }

        gunBarrel.localPosition = new Vector3(startPos.x, 0.49f, startPos.z);
    }

    private IEnumerator ResetJustLoaded()
    {
        yield return null; // Wait one frame
        justLoaded = false;
    }

    private IEnumerator DelayedHUDUpdate(float delay)
    {
        yield return new WaitForSeconds(delay);

        int tankedNow = bulletPool.GetTankedBulletCount();
        int total = bulletPool.GetEnabledBulletCount();
        ghostTankUI?.SetBulletCounts(tankedNow, total);

        Debug.Log($"⏱️ HUD updated with delay: {tankedNow} in tank / {total} total");
    }


}
