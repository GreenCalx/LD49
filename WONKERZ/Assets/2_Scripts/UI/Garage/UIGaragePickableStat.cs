using UnityEngine;
using UnityEngine.UI;
using Schnibble.UI;

public class UIGaragePickableStat : UITextTab
{
    public string XLabel;
    public string YLabel;

    override public void Select()
    {
        base.Select();
        // update background if it exists
        Image bg = GetComponentInChildren<Image>();
        if (!!bg)
        bg.color = new Color(bg.color.r, bg.color.g, bg.color.b, 0.8f);

    }

    override public void Deselect()
    {
        base.Deselect();
        // update background if it exists
        Image bg = GetComponentInChildren<Image>();
        if (!!bg)
        bg.color = new Color(bg.color.r, bg.color.g, bg.color.b, 0.3f);
    }
}
