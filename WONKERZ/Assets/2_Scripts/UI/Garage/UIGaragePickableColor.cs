using UnityEngine.UI;
using UnityEngine;
using System.Collections.Generic;
using Schnibble.UI;

public class UIGaragePickableColor : UIImageTab
{
    public List<COLORIZABLE_CAR_PARTS> parts_to_colorize;
    public Material material;

    void Start()
    {

    }

    override public void activate()
    {
        base.activate();
        
        foreach(COLORIZABLE_CAR_PARTS ccp in parts_to_colorize)
        {
            Access.PlayerCosmeticsManager().colorize( material , ccp);
        }
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
