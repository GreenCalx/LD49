using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIGaragePickableColor : UIImageTab
{
    override public void activate(){
        base.activate();
        
        PlayerColorManager.Instance.colorize(GetComponent<Image>().color);
    }
}
