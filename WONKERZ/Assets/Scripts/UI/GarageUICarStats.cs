using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GarageUICarStats : GarageUISelectable
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!is_active)
            return;
        
        if (Input.GetButtonDown("Cancel"))
            { parent.quitSubMenu(); return;}
    }

    public override void enter()
    {
        base.enter();
    }

    public override void quit()
    {
        base.quit();
    }
}
