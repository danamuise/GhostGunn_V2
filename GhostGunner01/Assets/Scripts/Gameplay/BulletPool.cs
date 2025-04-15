using UnityEngine;
using System.Collections.Generic;

public class BulletPool : MonoBehaviour
{
    [Header("Bullet Pool Settings")]
    public GameObject bulletPrefab;
    public int poolSize = 10;
    public int startingBullets = 1; // Number of bullets to show at game start

    [Header("Tank Settings")]
    public float tankVerticalOffset = 0f; // Vertical offset for floating bullets in tank

    private List<GameObject> pool = new List<GameObject>();

    void Awake()
    {
        for (int i = 0; i < poolSize; i++)
        {
            GameObject bullet = Instantiate(bulletPrefab, transform);
            GhostBullet ghost = bullet.GetComponent<GhostBullet>();

            bullet.SetActive(true);

            if (i < startingBullets)
            {
                ghost.EnterTank();

                // Apply vertical offset after entering tank
                Vector3 pos = bullet.transform.position;
                bullet.transform.position = new Vector3(pos.x, pos.y + tankVerticalOffset, pos.z);
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
} 
