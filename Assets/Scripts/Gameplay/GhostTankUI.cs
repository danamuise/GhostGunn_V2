using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class GhostTankUI : MonoBehaviour
{
    public TextMeshProUGUI ghostsInTankText;
    public TextMeshProUGUI ghostsTotalText;
    [Header("Bar Sprites (Top to Bottom)")]
    public List<SpriteRenderer> barSprites; // 10 bars in order

    public void UpdateMeter(int ghostsInTank, int totalGhosts)
    {
        int barsToShow = 0;

        if (totalGhosts <= 1)
        {
            barsToShow = (ghostsInTank >= 1) ? 10 : 0;
        }
        else
        {
            float fillRatio = (float)ghostsInTank / totalGhosts;
            barsToShow = Mathf.CeilToInt(fillRatio * 10f);
        }

        for (int i = 0; i < barSprites.Count; i++)
        {
            barSprites[i].enabled = (i < barsToShow);
            Debug.Log($"Bar {i}: {(i < barsToShow ? "ON" : "OFF")}");
        }

        Debug.Log($"📊 UpdateMeter() — tanked: {ghostsInTank}, total: {totalGhosts}, showing bars: {barsToShow}");
    }


    public void SetBulletCounts(int tanked, int total)
        {
            if (ghostsInTankText != null)
                ghostsInTankText.text = tanked.ToString();

            if (ghostsTotalText != null)
                ghostsTotalText.text = total.ToString();

            Debug.Log($"🖥️ HUD updated: {tanked} in tank / {total} total");
        }
}
