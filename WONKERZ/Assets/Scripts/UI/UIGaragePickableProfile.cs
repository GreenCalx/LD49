using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIGaragePickableProfile : UIGarageSelectable
{
    public string profile_name;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public override void enter(UIGarageSelector uigs) { base.enter(uigs); }
    public override void quit() { base.quit(); }
}
