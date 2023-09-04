using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIBountyMatrix : UIPanelTabbed
{
    private BountyArray bountyMatrix;
    public TextMeshProUGUI tooltip; 
    // Start is called before the first frame update
    void Start()
    {
        bountyMatrix = Access.BountyArray();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    override public void activate()
    {
        bountyMatrix = Access.BountyArray();
        bountyMatrix.initUI(this, tooltip);
        base.activate();
    }

    override public void deactivate()
    {
        
        bountyMatrix.hide(this);
        transform.parent.gameObject.SetActive(false);
        base.deactivate();
    }
}
