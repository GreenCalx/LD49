using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIGarageCosmeticsPanel : UIGarageCancelablePanel
{
    public void activated(){
        Utils.attachUniqueControllable(this);
        isActivated = true;
    }

    public void deactivated(){
        Utils.detachUniqueControllable();
        isActivated = false;

        activator.onSelect?.Invoke();
    }

    public void open()
    {   
        elapsed_time = 0f;

        gameObject.SetActive(true);
        animateIn();
        foreach(UITab t in Tabs) {
            t.onDeselect?.Invoke();
        }
        Tabs[0].onSelect?.Invoke();
    }

    public void close()
    {
       animateOut();
       gameObject.SetActive(false);
    }

}
