using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIGaragePickableTest : UIGarageSelectable
{
    private UIGarageTestManager uigtm;
    // Start is called before the first frame update
    void Start()
    {
        uigtm = Utils.getTestManager();
    }

    // Update is called once per frame
    void Update()
    {

    }
    public override void enter(UIGarageSelector uigs) 
    { 
        base.enter(uigs);

        if(null==uigtm)
        {
            Debug.LogError("Missing UIGarage Test Manager.");
            return;
        }
        uigtm.launchTest();
    }
    public override void quit() 
    { 
        if(null==uigtm)
        {
            Debug.LogError("Missing UIGarage Test Manager.");
        } else {
            uigtm.quitTest();
        }
        
        base.quit(); 
    }


}
