using UnityEngine;

public class GameSettings : MonoBehaviour
{
    public bool isLocal = false;
    public bool IsOnline = false;
    public string OnlinePlayerAlias = "Player";
    // HACK: to remove by doing something more generic
    public UIOnline.States goToState = UIOnline.States.MainMenu;

    public Schnibble.UI.UITheme defaultUITheme = new Schnibble.UI.UITheme();

    void Awake() {
        Schnibble.UI.UITheme.defaultUITheme = defaultUITheme;
    }
}
