using UnityEngine;
using Schnibble;

public class GameSettings : MonoBehaviour
{
    public bool   isLocal           = false;
    public bool   isOnline          = false;
    public string OnlinePlayerAlias = "Player";

    public Schnibble.UI.UITheme defaultUITheme = new Schnibble.UI.UITheme();

    public void init() {
        this.Log("init.");
        Schnibble.UI.UITheme.defaultUITheme = defaultUITheme;
    }
}
