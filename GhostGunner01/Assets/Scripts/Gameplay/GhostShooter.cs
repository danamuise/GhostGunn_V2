using System.Collections.Generic;
using UnityEngine;
using System.Collections;
public class GhostShooter : MonoBehaviour
{
    public BulletPool bulletPool;
    public Transform firePoint;
    public LaserTrajectoryPreview trajectoryPreview;

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

        Vector2 direction = (worldPoint - firePoint.position).normalized;

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
            Debug.Log("Direction: " + direction);

            StartCoroutine(FireAllBullets(direction));
            DisableGun();
        }
    }

    void FireBullet(Vector2 direction)
    {
        Debug.Log($"🔫 FireBullet() called at {Time.time:F2}");

        GameObject bulletGO = bulletPool.GetBullet();

        if (bulletGO == null)
        {
            Debug.LogWarning("🚫 No available bullet to fire!");
            return;
        }

        Debug.Log($"✅ Firing bullet: {bulletGO.name}");

        bulletGO.transform.position = firePoint.position;
        bulletGO.SetActive(true);

        GhostBullet bullet = bulletGO.GetComponent<GhostBullet>();
        bullet.Fire(direction);
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

        Debug.Log($"🔎 AllBulletsAreInTank(): {inTank} in tank / {total} total");

        return inTank == total;
    }


    private IEnumerator FireAllBullets(Vector2 direction)
    {
        List<GameObject> bullets = bulletPool.GetAllBullets();

        foreach (GameObject bulletGO in bullets)
        {
            if (!bulletGO.activeInHierarchy) continue;

            GhostBullet bullet = bulletGO.GetComponent<GhostBullet>();
            if (bullet != null && bullet.IsInTank)
            {
                bulletGO.transform.position = firePoint.position;
                bulletGO.SetActive(true);
                bullet.Fire(direction);

                Debug.Log($"🚀 Fired bullet: {bulletGO.name} at {Time.time:F2}");

                yield return new WaitForSeconds(0.15f); // delay between shots
            }
        }
    }

}
