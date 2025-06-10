using UnityEngine;

public class ClickableSpriteButton : MonoBehaviour
{
    public enum ButtonAction
    {
        MoveUIOut,
        MoveUIIn,
        ToggleMusic
    }

    public ButtonAction action;

    private void OnMouseDown()
    {
        if (action == ButtonAction.MoveUIOut)
        {
            SFXManager.Instance.MoveUIOut();
        }
        else if (action == ButtonAction.MoveUIIn)
        {
            SFXManager.Instance.MoveUIIn();
        }
        else if (action == ButtonAction.ToggleMusic)
        {
            Debug.Log("Toggle Music");
            SFXManager.Instance.ToggleMusic();
        }
    }
}
