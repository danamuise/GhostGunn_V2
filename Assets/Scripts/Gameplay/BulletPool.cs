using UnityEngine;
using System.Collections.Generic;

public class BulletPool : MonoBehaviour
{
    [Header("Bullet Pool Settings")]
    public GameObject bulletPrefab;
    public int poolSize = 10;
    public int startingBullets = 1;

    [Header("Tank Settings")]
    public float tankVerticalOffset = 0f;

    [Header("Hierarchy")]
    public Transform bulletParent;
    public GhostTankUI ghostTankUI; // ✅ HUD reference

    private List<GameObject> pool = new List<GameObject>();
    private int activeBulletCount = 0;

    void Awake()
    {
        for (int i = 0; i < poolSize; i++)
        {
            GameObject bullet = Instantiate(bulletPrefab, Vector3.zero, Quaternion.identity, bulletParent);
            GhostBullet ghost = bullet.GetComponent<GhostBullet>();

            bullet.SetActive(true);

            if (i < startingBullets)
            {
                ghost.EnterTank();
                Vector3 pos = bullet.transform.position;
                bullet.transform.position = new Vector3(pos.x, pos.y + tankVerticalOffset, pos.z);
                activeBulletCount++;
            }
            else
            {
                bullet.SetActive(false);
            }

            pool.Add(bullet);
        }

        // ✅ Initial HUD update after all bullets are initialized
        if (ghostTankUI != null)
        {
            int tanked = GetTankedBulletCount();
            int enabled = GetEnabledBulletCount();
            ghostTankUI.SetBulletCounts(tanked, enabled);
            Debug.Log($"📟 Initial HUD update: {tanked} in tank / {enabled} total");
        }
    }

    public GameObject GetBullet()
    {
        foreach (GameObject bullet in pool)
        {
            if (bullet.activeInHierarchy)
            {
                GhostBullet ghost = bullet.GetComponent<GhostBullet>();
                if (ghost != null && ghost.IsInTank)
                {
                    Debug.Log($"🎯 GetBullet() returning: {bullet.name}");
                    Debug.Log($"👻 Ghosts in tank (ready to fire): {GetTankedBulletCount()}");
                    return bullet;
                }
            }
        }

        Debug.LogWarning("⚠️ No bullets available in the tank!");
        return null;
    }

    public void AddBullet()
    {
        Debug.Log("➕ AddBullet() called");

        int tanked = GetTankedBulletCount();
        int max = poolSize;

        if (tanked >= max)
        {
            Debug.Log("🔫 Bullet tank already full — cannot add more bullets.");
            return;
        }

        for (int i = 0; i < pool.Count; i++)
        {
            GameObject bullet = pool[i];
            if (!bullet.activeInHierarchy)
            {
                bullet.SetActive(true);
                GhostBullet ghost = bullet.GetComponent<GhostBullet>();
                if (ghost != null)
                {
                    ghost.EnterTank();
                    Vector3 pos = bullet.transform.position;
                    bullet.transform.position = new Vector3(pos.x, pos.y + tankVerticalOffset, pos.z);
                }

                Debug.Log($"✨ Bullet added: {bullet.name}");
                Debug.Log($"👻 Ghosts in tank after add: {GetTankedBulletCount()}");

                if (ghostTankUI != null)
                {
                    ghostTankUI.SetBulletCounts(GetTankedBulletCount(), GetEnabledBulletCount());
                }

                return;
            }
        }

        Debug.LogWarning("🔫 AddBullet() called but no inactive bullets were found.");
    }

    public int GetTankedBulletCount()
    {
        int count = 0;
        foreach (GameObject bulletGO in pool)
        {
            if (bulletGO.activeInHierarchy)
            {
                GhostBullet bullet = bulletGO.GetComponent<GhostBullet>();
                if (bullet != null && bullet.IsInTank)
                    count++;
            }
        }
        return count;
    }

    public int GetEnabledBulletCount()
    {
        int count = 0;
        foreach (GameObject bullet in pool)
        {
            if (bullet.activeInHierarchy)
                count++;
        }
        return count;
    }

    public List<GameObject> GetAllBullets() => pool;

    public int GetTotalBulletCount() => poolSize;

    public bool AllBulletsReturned()
    {
        foreach (GameObject bulletGO in pool)
        {
            if (bulletGO.activeInHierarchy)
            {
                GhostBullet bullet = bulletGO.GetComponent<GhostBullet>();
                if (bullet != null && !bullet.IsInTank)
                    return false;
            }
        }
        return true;
    }

    public GameObject GetNextAvailableBullet()
    {
        foreach (GameObject bullet in pool)
        {
            if (!bullet.activeInHierarchy)
            {
                return bullet;
            }
        }

        Debug.LogWarning("⚠️ BulletPool: No available bullets left.");
        return null;
    }

    public int GetActiveBulletCount() => activeBulletCount;

    public void SetTankedBulletCount(int count)
    {
        Debug.Log($"🛠️ Restoring {count} bullets to tank from saved state…");

        int currentTanked = GetTankedBulletCount();

        // 1. Activate bullets until we reach the target count
        foreach (GameObject bulletGO in pool)
        {
            if (GetTankedBulletCount() >= count)
                break;

            GhostBullet bullet = bulletGO.GetComponent<GhostBullet>();

            if (!bulletGO.activeInHierarchy)
            {
                bulletGO.SetActive(true);
            }

            if (bullet != null && !bullet.IsInTank)
            {
                bullet.EnterTank();
                Vector3 pos = bulletGO.transform.position;
                bulletGO.transform.position = new Vector3(pos.x, pos.y + tankVerticalOffset, pos.z);
            }
        }

        // 2. Deactivate excess tanked bullets
        foreach (GameObject bulletGO in pool)
        {
            if (GetTankedBulletCount() <= count)
                break;

            GhostBullet bullet = bulletGO.GetComponent<GhostBullet>();
            if (bullet != null && bullet.IsInTank)
            {
                bulletGO.SetActive(false);
            }
        }

        if (ghostTankUI != null)
        {
            ghostTankUI.SetBulletCounts(GetTankedBulletCount(), GetEnabledBulletCount());
            Debug.Log($"📟 Bullet count after restore: {GetTankedBulletCount()} in tank / {GetEnabledBulletCount()} total");
        }
    }
}
