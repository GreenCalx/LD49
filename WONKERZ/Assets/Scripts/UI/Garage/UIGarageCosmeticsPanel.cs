using UnityEngine;
using System.Collections.Generic;
using Schnibble;

/**
* MAIN, DOORS, HOOD, BUMPS, WHEEL
*/

public class UIGarageCosmeticsPanel : UIGaragePanel
{
    override protected List<IUIGarageElement.UIGarageHelperValue> getHelperInputs()
    {
        return new List<IUIGarageElement.UIGarageHelperValue>{
            new IUIGarageElement.UIGarageHelperValue(Constants.RES_ICON_A, "OK"),
            new IUIGarageElement.UIGarageHelperValue(Constants.RES_ICON_B, "CANCEL")
        };
    }

    public void selectCategory()
    {
        // Part to modify
    }


    public void refreshCarPartsCosmetics(COLORIZABLE_CAR_PARTS iPart)
    {
        // init from table
    }


}
