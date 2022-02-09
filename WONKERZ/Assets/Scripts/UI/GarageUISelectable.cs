using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GarageUISelectable : MonoBehaviour
{
    protected UIGarage parent;
    protected bool is_active;

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

    public virtual void enter() { findParent(); is_active = true; }
    public virtual void quit() { is_active = false; }
}
