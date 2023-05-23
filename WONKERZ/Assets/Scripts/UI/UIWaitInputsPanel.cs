using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIWaitInputsPanel : UIPanelControlable
{
    private bool wait = false;
    void OnGUI(){
        if (wait) {
            var e = Event.current;
            if (e.isKey && e.type == EventType.KeyDown) {
                if (e.keyCode != KeyCode.None)
                {
                    wait = false;
                    (Parent as UIBindingElement).SetBinding(e.keyCode);
                }
            }
        }
    }

    // IMPORTANT toffa: needs to be done in update to be sure that we dont send
    // inputs back next frame.
    void Update(){
        if (!wait)
                    onDeactivate?.Invoke();
    }

    public void WaitForInput(){
        wait = true;
    }

    public void SetParent(UIElement e){
        Parent = e;
    }
}
