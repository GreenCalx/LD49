using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIGarageCategory : GarageUISelectable
{
    public GameObject panelToLoad;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public override void enter()
    {
        base.enter();
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
            uigp.open();
        }
    }

    public override void quit() 
    { 
        base.quit();

        if (panelToLoad==null)
        {
            Debug.LogWarning("No Panel to load attached to selected category.");
            return;
        }
        is_active = false; 

        UIGaragePanel uigp = panelToLoad.GetComponent<UIGaragePanel>();
        if (!!uigp)
        {
            uigp.close();
            uigp.gameObject.SetActive(false);
        }
    }
}
