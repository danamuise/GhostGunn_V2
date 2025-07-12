using UnityEngine;

public class ClickableSpriteButton : MonoBehaviour
{
    public enum ButtonAction
    {
        MoveUIOut,
        MoveUIIn,
        ToggleMusic,
        ToggleSFX
    }

    public ButtonAction action;

    private void OnMouseDown()
    {
        switch (action)
        {
            case ButtonAction.MoveUIOut:
                SFXManager.Instance.MoveUIOut();
                break;
            case ButtonAction.MoveUIIn:
                SFXManager.Instance.MoveUIIn();
                break;
            case ButtonAction.ToggleMusic:
                Debug.Log("Toggle Music");
                SFXManager.Instance.ToggleMusic();
                break;
            case ButtonAction.ToggleSFX:
                Debug.Log("Toggle SFX");
                SFXManager.Instance.ToggleSFX(); // 👈 New case
                break;
        }
    }

}
