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

    /*
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
        Vector2 randomized = AddSpreadToDirection(direction, 5f); // 2 degrees of spread
        Debug.Log($"🔄 Randomized dir: {randomized} from base: {direction}");

        bullet.Fire(randomized);

    }
    */ 
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

        foreach (GameObject bulletGO in bullets)
        {
            if (!bulletGO.activeInHierarchy) continue;

            GhostBullet bullet = bulletGO.GetComponent<GhostBullet>();
            if (bullet != null && bullet.IsInTank)
            {
                bulletGO.transform.position = firePoint.position;
                bulletGO.SetActive(true);
                Vector2 randomized = AddSpreadToDirection(direction, 2.4f);
                Debug.Log($"🚀 Spread direction for {bulletGO.name}: {randomized}");
                bullet.Fire(randomized);

                Debug.Log($"🚀 Fired bullet: {bulletGO.name} at {Time.time:F2}");

                yield return new WaitForSeconds(0.1f); // delay between shots
            }
        }
    }
    private Vector2 AddSpreadToDirection(Vector2 baseDirection, float maxAngleDegrees)
    {
        Debug.Log($"🚀 ADDING SPREAD TO DIRECTION ***************************************************");
        float angle = Random.Range(-maxAngleDegrees, maxAngleDegrees);
        float radians = angle * Mathf.Deg2Rad;

        // Rotate the direction vector by a small angle
        float cos = Mathf.Cos(radians);
        float sin = Mathf.Sin(radians);

        float newX = baseDirection.x * cos - baseDirection.y * sin;
        float newY = baseDirection.x * sin + baseDirection.y * cos;

        return new Vector2(newX, newY).normalized;
    }

}
