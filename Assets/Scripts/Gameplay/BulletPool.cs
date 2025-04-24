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
                    return bullet;
                }
            }
        }

        Debug.LogWarning("No bullets available in the tank!");
        return null;
    }

    public List<GameObject> GetAllBullets()
    {
        return pool;
    }

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

    public void EnableNextBullet()
    {
        if (activeBulletCount >= poolSize)
        {
            Debug.Log("🔫 All bullets are already active.");
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

                activeBulletCount++;
                Debug.Log($"🔫 Enabled bullet #{activeBulletCount}");
                break;
            }
        }
    }

    public int GetActiveBulletCount()
    {
        return activeBulletCount;
    }

    public int GetTotalBulletCount()
    {
        return poolSize;
    }
}
