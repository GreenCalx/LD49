using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIChecklistImageTab : UIImageTab
{
    [Header("Checklist Specifics")]
    public Color c_unlocked;
    public Color c_visible;
    public Color c_locked;
    public string locked_tooltip = "???";

    public BountyArray bountyArray { set; private get; }
    public TextMeshProUGUI tooltip { set; private get; }

    public Image img2;

    [HideInInspector]
    public int x, y;

    override public void select()
    {
        base.select();
        BountyArray.AbstractBounty bounty;
        if (bountyArray.getBountyAt(x, y, out bounty))
        {
            tooltip.text = bounty.hint;
        } else {
            // LOCKED
            tooltip.text = locked_tooltip;
        }
        // Update Tooltip
    }
    
    override public void deselect()
    {
        base.deselect();
    }

    override public void activate()
    {
        base.activate();
    }

    override public void deactivate()
    {
        base.deactivate();
    }

    public void updateColor()
    {
        BountyArray.EItemState status = bountyArray.getStatus(x, y);
        switch (status)
        {
            case BountyArray.EItemState.UNLOCKED:
                img2.color = c_unlocked;
                break;
            case BountyArray.EItemState.VISIBLE:
                img2.color = c_visible;
                break;
            case BountyArray.EItemState.LOCKED:
                img2.color = c_locked;
                break;
            default:
                img2.color = c_locked;
                break;
        }
    }
}
