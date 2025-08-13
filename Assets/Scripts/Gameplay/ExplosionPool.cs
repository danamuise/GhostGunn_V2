using System.Collections.Generic;
using UnityEngine;

public class ExplosionPool : MonoBehaviour
{
    public GameObject explosionPrefab;
    public int poolSize = 20;

    private Queue<GameObject> explosionPool = new Queue<GameObject>();

    private void Start()
    {
        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = Instantiate(explosionPrefab);
            obj.SetActive(false);
            explosionPool.Enqueue(obj);
        }
    }

    public GameObject GetExplosion()
    {
        if (explosionPool.Count > 0)
        {
            GameObject obj = explosionPool.Dequeue();
            obj.SetActive(true);
            return obj;
        }
        else
        {
            // Optionally expand pool if needed
            GameObject obj = Instantiate(explosionPrefab);
            return obj;
        }
    }

    public void ReturnExplosion(GameObject obj)
    {
        obj.SetActive(false);
        explosionPool.Enqueue(obj);
    }
}
