using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using Schnibble.UI;

public class UIGaragePickableSkin : UIImageTab
{
    [Header("Skin")]
    public string skinName;
    public List<COLORIZABLE_CAR_PARTS> carParts;

    override public void activate()
    {
        base.activate();

        Access.PlayerCosmeticsManager().customize(skinName, carParts);
        
    }

    override public void select()
    {
        base.select();
        transform.localScale = new Vector3(1.2f, 1.2f, 1.2f );
    }

    override public void deselect()
    {
        base.deselect();
        transform.localScale = new Vector3(1f, 1f, 1f );
    }
}
