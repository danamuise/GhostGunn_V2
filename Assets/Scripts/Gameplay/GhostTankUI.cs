using UnityEngine;
using TMPro;

public class GhostTankUI : MonoBehaviour
{
    public TextMeshProUGUI ghostsInTankText;
    public TextMeshProUGUI ghostsTotalText;

    public void SetBulletCounts(int tanked, int total)
    {
        if (ghostsInTankText != null)
            ghostsInTankText.text = tanked.ToString();

        if (ghostsTotalText != null)
            ghostsTotalText.text = total.ToString();

        Debug.Log($"🖥️ HUD updated: {tanked} in tank / {total} total");
    }
}
