using UnityEngine;
using System.Collections.Generic;

public class TargetRegistry : MonoBehaviour
{
    public static TargetRegistry Instance;

    private Dictionary<int, TargetData> dataByID = new();
    private int nextID = 0;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public TargetData RegisterNewTarget(int health, GameObject prefab)
    {
        TargetData data = ScriptableObject.CreateInstance<TargetData>();
        data.targetID = nextID;
        //data.prefab = prefab;
        data.SetInitialHealth(health);

        dataByID[nextID] = data;
        nextID++;

        return data;
    }

    public TargetData GetDataByID(int id)
    {
        return dataByID.TryGetValue(id, out var data) ? data : null;
    }
}
