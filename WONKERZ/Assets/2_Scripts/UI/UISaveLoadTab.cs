using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UISaveLoadTab : UITextTab
{
    public override void activate()
    {
        base.activate();
    }

    public void save()
    {
        Access.CollectiblesManager().saveJars();
    }

    public void load()
    {
        Access.CollectiblesManager().loadJars();
    }
}
