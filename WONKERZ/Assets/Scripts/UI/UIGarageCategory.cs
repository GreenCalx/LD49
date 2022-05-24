using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIGarageCategory : UIGarageSelectable
{
    public GameObject panelToLoad;
    private string openedPanelName;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public override void enter(UIGarageSelector uigs)
    {
        base.enter(uigs);
        
        if (panelToLoad==null)
        {
            Debug.LogWarning("No Panel to load attached to selected category.");
            return;
        }
        is_active = true;

        UIGaragePanel uigp = panelToLoad.GetComponent<UIGaragePanel>();
        if (!!uigp)
        {
            uigp.gameObject.SetActive(true);
            bool animate = openedPanelName!=uigp.name; // new panel
            uigp.open(animate);
            openedPanelName = uigp.name;
        }
    }

    public override void quit() 
    { 
        if (panelToLoad==null)
        {
            Debug.LogWarning("No Panel to load attached to selected category.");
            return;
        }
        is_active = false; 

        UIGaragePanel uigp = panelToLoad.GetComponent<UIGaragePanel>();
        if (!!uigp)
        {
            openedPanelName = "";
            uigp.close(true);
            uigp.gameObject.SetActive(false);
        }
        
        base.quit();
    }
}
