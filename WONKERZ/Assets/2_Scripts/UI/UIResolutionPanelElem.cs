using UnityEngine;
using Schnibble.UI;

public class UIResolutionPanelElem : UITextTab
{
    public void ApplyResolution()
    {
        var res = Screen.resolutions[index];
        Screen.SetResolution(res.width, res.height, Screen.fullScreen);
    }
}
