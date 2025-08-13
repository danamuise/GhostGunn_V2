using UnityEngine;

[CreateAssetMenu(menuName = "GhostGunn/RuntimeTargetData", fileName = "RuntimeTargetData")]
public class TargetData : ScriptableObject
{
    [Header("Target Properties")]
    public int targetID;
    public int initialHealth;

    public void SetInitialHealth(int value)
    {
        initialHealth = value;
    }

    public void SetInitialID(int id)
    {
        targetID = id;
    }
    public int GetInitialHealth()
    {
        return initialHealth;
    }
    public int GetID()
    {
        return targetID;
    }
}
