using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIButton : UISelectableElement, IControllable {
    // TODO : make activating input generic

    void IControllable.ProcessInputs(InputManager.InputData Entry)
    {
        ProcessInputs(Entry);
    }

    virtual protected void ProcessInputs(InputManager.InputData Entry)
    {
        if (Entry.Inputs[Constants.INPUT_JUMP].IsDown)
            onActivate?.Invoke();
    }

     override public void select(){
         base.select();
         Utils.attachControllable(this);
    }

    override public void deselect(){
         base.deselect();
         Utils.detachControllable(this);
    }
}
