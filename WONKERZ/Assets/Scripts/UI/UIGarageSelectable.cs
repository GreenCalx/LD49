using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIGarageSelectable : MonoBehaviour
{
    protected UIGarage parent;
    protected bool is_active;
    protected UIGarageSelector originalCaller;

    public void setOriginalCaller( UIGarageSelector uigs)
    {
        originalCaller = uigs;
    }

    // Start is called before the first frame update
    void Start()
    {
        is_active = false;
    }

    protected void findParent()
    {
        if (parent == null)
            parent = GetComponentInParent<UIGarage>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public virtual void enter(UIGarageSelector uigs) 
    { 
        findParent(); 
        is_active = true;
        setOriginalCaller(uigs); 
    }
    public virtual void quit() 
    { 
        is_active = false; 
        originalCaller.handGivenBack(); 
        setOriginalCaller(null); 
    }
}
