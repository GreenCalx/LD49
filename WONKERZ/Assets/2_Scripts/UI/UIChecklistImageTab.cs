using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

using Schnibble.UI;

public class UIChecklistImageTab : UIImageTab
{
    [Header("Checklist Specifics")]
    public Color c_unlocked;
    public Color c_visible;
    public Color c_locked;
    public string locked_tooltip = "???";

    public BountyArray bountyArray { set; private get; }
    public TextMeshProUGUI tooltip_bountyDesc { set; private get; }
    public TextMeshProUGUI tooltip_bountyName { set; private get; }
    public TextMeshProUGUI tooltip_bountyReward { set; private get; }

    public Image img2;

    [HideInInspector]
    public int x, y;

    override public void select()
    {
        base.select();
        BountyArray.AbstractBounty bounty;
        if (bountyArray.getBountyAt(x, y, out bounty))
        {
            tooltip_bountyDesc.text     = bounty.hint;
            tooltip_bountyName.text     = bounty.name;
            if (bountyArray.getStatus(x, y)==BountyArray.EItemState.UNLOCKED)
                tooltip_bountyReward.text = bounty.GetRewardsAsText();
            else
                tooltip_bountyReward.text   = locked_tooltip;
        } else {
            // LOCKED
            tooltip_bountyName.text     = locked_tooltip;
            tooltip_bountyDesc.text     = locked_tooltip;
            tooltip_bountyReward.text   = locked_tooltip;
        }
        // Update tooltip_bountyDesc
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
