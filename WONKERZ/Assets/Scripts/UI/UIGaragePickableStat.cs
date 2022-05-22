using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIGaragePickableStat : GarageUISelectable
{
    public string XLabel;
    public string YLabel;
    public UIGarageCurve.CAR_PARAM car_param;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void enter() { findParent(); is_active = true; }
    public override void quit() { is_active = false; }
}
