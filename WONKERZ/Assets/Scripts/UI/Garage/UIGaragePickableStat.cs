using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIGaragePickableStat : UITextTab
{
    public string XLabel;
    public string YLabel;

    override public void select()
    {
        base.select();
        // update background if it exists
        Image bg = GetComponentInChildren<Image>();
        if (!!bg)
            bg.color = new Color( bg.color.r, bg.color.g, bg.color.b, 0.8f);

    }

    override public void deselect()
    {
        base.deselect();
        // update background if it exists
        Image bg = GetComponentInChildren<Image>();
        if (!!bg)
            bg.color = new Color( bg.color.r, bg.color.g, bg.color.b, 0.3f);
    }
}
