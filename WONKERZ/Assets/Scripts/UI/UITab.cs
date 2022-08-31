using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UITab : UISelectableElement, IControllable
{
    public UISelectableElement toActivate;

    void IControllable.ProcessInputs(InputManager.InputData Entry)
    {
        ProcessInputs(Entry);
    }

    virtual protected void ProcessInputs(InputManager.InputData Entry)
    {
        if (Entry.Inputs[Constants.INPUT_JUMP].IsDown)
            onActivate?.Invoke();
    }

     virtual public void select(){
         base.select();

         toActivate?.onSelect?.Invoke();

         Utils.attachControllable(this);
    }

    virtual public void deselect(){
         base.deselect();

         toActivate?.onDeselect?.Invoke();
         Utils.detachControllable(this);
    }

    virtual public void activate(){
         base.activate();

         toActivate?.onActivate?.Invoke();
    }

    virtual public void deactivate(){
        
    }
}
