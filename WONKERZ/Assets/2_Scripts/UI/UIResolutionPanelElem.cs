using UnityEngine;
using Schnibble.UI;

public class UIResolutionPanelElem : UITextTab
{
    public int idx;
    public void ApplyResolution()
    {
        var res = Screen.resolutions[idx];
        Screen.SetResolution(res.width, res.height, Screen.fullScreen);
    }
}
