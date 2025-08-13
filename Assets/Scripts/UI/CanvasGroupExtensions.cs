using UnityEngine;

public static class CanvasGroupExtensions
{
    public static void SetInteractable(this CanvasGroup cg, bool state)
    {
        if (cg != null)
        {
            cg.interactable = state;
            cg.blocksRaycasts = state;
            cg.alpha = state ? 1f : 0.5f; // optional: dim when disabled
        }
    }
}