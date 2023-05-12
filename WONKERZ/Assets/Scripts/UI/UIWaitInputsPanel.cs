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
