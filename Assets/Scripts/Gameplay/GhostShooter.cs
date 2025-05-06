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
    private Coroutine recoilCoroutine;
    private bool canShoot = true;

    void Update()
    {
        if (!canShoot) return;

        if (!AllBulletsAreInTank())
        {
            trajectoryPreview?.ClearDots();
            return;
        }

#if UNITY_EDITOR
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

        // ✨ Clamp direction between 10 and 170 degrees
        Vector2 rawDirection = (worldPoint - firePoint.position).normalized;
        float angle = Mathf.Atan2(rawDirection.y, rawDirection.x) * Mathf.Rad2Deg;
        angle = Mathf.Clamp(angle, 10f, 170f);
        if (gunBase != null)
            gunBase.rotation = Quaternion.Euler(0f, 0f, angle-90f);

        float clampedRad = angle * Mathf.Deg2Rad;
        Vector2 direction = new Vector2(Mathf.Cos(clampedRad), Mathf.Sin(clampedRad));

        if (inputDown)
        {
            if (trajectoryPreview != null)
                trajectoryPreview.DrawLaserLine(firePoint.position, direction);
        }

        if (inputUp)
        {
            if (trajectoryPreview != null)
                trajectoryPreview.ClearDots();

            Debug.DrawLine(firePoint.position, worldPoint, Color.red, 2f);
            //Debug.Log("Direction: " + direction);

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

        SFXManager.Instance.Play("GhostSound", 0.5f, 0.9f, 1.1f);

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

                // 🔊 Play SFX per bullet
                SFXManager.Instance.Play("BulletShoot", 0.5f, 0.9f, 1.1f);

                // 🔫 Trigger gun barrel recoil
                TriggerRecoil();

                Debug.Log($"🚀 Fired bullet: {bulletGO.name} at {Time.time:F2}");

                yield return new WaitForSeconds(0.1f); // delay between shots
            }
        }
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
}
