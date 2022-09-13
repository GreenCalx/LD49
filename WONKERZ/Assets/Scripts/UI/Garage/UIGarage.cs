using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIGarage : UIGaragePanel {
    private GarageEntry garageEntry;

    override public void deactivate() {
        base.deactivate();
        garageEntry.closeGarage();
    }

    public GarageEntry getGarageEntry()
    {
        return garageEntry;
    }

    public void setGarageEntry(GarageEntry iGE)
    {
        garageEntry = iGE;
    }
}
