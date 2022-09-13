using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UITab : UIButton
{
    public bool isActivated = false;
    public UISelectableElement toActivate;

     override public void select(){
         base.select();

         // NOTE : not sure this is good
         toActivate?.onSelect?.Invoke();
    }

    override public void deselect(){
         base.deselect();

         // NOTE : not sure this is good
         toActivate?.onDeselect?.Invoke();
    }

    override public void activate(){
         base.activate();

         toActivate?.onActivate?.Invoke();
    }
}
