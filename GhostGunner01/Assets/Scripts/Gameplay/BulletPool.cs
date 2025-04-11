using UnityEngine;
using System.Collections.Generic;

public class BulletPool : MonoBehaviour
{
    [Header("Bullet Pool Settings")]
    public GameObject bulletPrefab;
    public int poolSize = 10;
    public int maxPoolSize = 20;

    private Queue<GameObject> pool = new Queue<GameObject>();

    void Awake()
    {
        for (int i = 0; i < poolSize; i++)
        {
            GameObject bullet = Instantiate(bulletPrefab, transform);
            bullet.SetActive(false);
            pool.Enqueue(bullet);
        }
    }

    public GameObject GetBullet()
    {
        foreach (GameObject bullet in pool)
        {
            if (!bullet.activeInHierarchy)
            {
                ResetBullet(bullet);
                return bullet;
            }
        }

        if (pool.Count >= maxPoolSize)
        {
            Debug.LogWarning("Bullet pool limit reached. Reusing oldest bullet.");
            GameObject recycled = pool.Dequeue();
            ResetBullet(recycled);
            pool.Enqueue(recycled);
            return recycled;
        }

        GameObject newBullet = Instantiate(bulletPrefab, transform);
        newBullet.SetActive(false);
        ResetBullet(newBullet);
        pool.Enqueue(newBullet);
        return newBullet;
    }

    private void ResetBullet(GameObject bullet)
    {
        bullet.transform.SetParent(transform);
        bullet.transform.localPosition = Vector3.zero;
        bullet.transform.rotation = Quaternion.identity;
        bullet.SetActive(false);
    }
}
