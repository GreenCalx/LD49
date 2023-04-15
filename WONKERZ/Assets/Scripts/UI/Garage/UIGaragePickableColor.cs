using UnityEngine.UI;
using UnityEngine;
using System.Collections.Generic;

public class UIGaragePickableColor : UIImageTab
{
    public List<COLORIZABLE_CAR_PARTS> parts_to_colorize;

    override public void activate()
    {
        base.activate();

        Color c = GetComponent<Image>().color;
        Color c2 = GetComponentInChildren<Image>().color;

        foreach(COLORIZABLE_CAR_PARTS ccp in parts_to_colorize)
        {
            PlayerColorManager.Instance.colorize(c2, ccp);
        }
        // PlayerColorManager.Instance.colorize(c2, COLORIZABLE_CAR_PARTS.MAIN);
        // PlayerColorManager.Instance.colorize(c2, COLORIZABLE_CAR_PARTS.DOORS);
        // PlayerColorManager.Instance.colorize(c2, COLORIZABLE_CAR_PARTS.HOOD);
    }
}
