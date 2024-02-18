using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using Schnibble.UI;
using TMPro;


public class UIGaragePickableSkin : UIImageTab
{
    [Header("Self Refs")]
    public TextMeshProUGUI self_skinIdLbl;

    [Header("Skin")]
    public string skinName;
    private int skinID;
    public COLORIZABLE_CAR_PARTS carPart;

    public void setSkinID(int iVal)
    {
        skinID = iVal;
        self_skinIdLbl.text = iVal.ToString();
    }

    override public void activate()
    {
        base.activate();

        Access.PlayerCosmeticsManager().customize(skinID, carPart);
        
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
