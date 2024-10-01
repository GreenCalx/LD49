using System.Collections.Generic;
using UnityEngine;

using Schnibble.UI;

public class UIGaragePanel : UIPanelTabbed, IUIGarageElement
{
    public UIGarageInputHelper inputHelper;

    List<IUIGarageElement.UIGarageHelperValue> IUIGarageElement.getHelperInputs()
    {
        return getHelperInputs();
    }

    virtual protected List<IUIGarageElement.UIGarageHelperValue> getHelperInputs()
    {
        return new List<IUIGarageElement.UIGarageHelperValue>{
            new IUIGarageElement.UIGarageHelperValue(Constants.RES_ICON_A, "OK"),
            new IUIGarageElement.UIGarageHelperValue(Constants.RES_ICON_B, "CANCEL")
        };
    }

    public override void Activate()
    {
        base.Activate();

        inputHelper.refreshHelper(this);
    }

    public override void Deactivate()
    {
        base.Deactivate();
        if (parent!=null)
        inputHelper.refreshHelper((parent as UIGaragePanel));
    }

    public override void Select()
    {
        gameObject.SetActive(true);
        base.Select();
        animateIn();
    }

    public override void Deselect()
    {
        base.Deselect();
        animateOut();
        gameObject.SetActive(false);
    }

    public void animateIn()
    {
        var animators = new List<Animator>(GetComponentsInChildren<Animator>());
        if (animators != null)
        {
            foreach (Animator a in animators)
            {
                a.enabled = true;
                a.updateMode = AnimatorUpdateMode.UnscaledTime; // as we pause game by putting deltaTime to 0
                //a.SetTrigger("animatePanel");
                a.Play("Base Layer.GaragePanelIn", -1, 0);
            }
        }
    }

    public void animateOut()
    {
        var animators = new List<Animator>(GetComponentsInChildren<Animator>());
        if (animators != null)
        {
            foreach (Animator a in animators)
            {
                a.enabled = true;
                a.updateMode = AnimatorUpdateMode.UnscaledTime; // as we pause game by putting deltaTime to 0
                //a.SetTrigger("animatePanel");
                a.Play("Base Layer.GaragePanelOut", -1, 0);
            }
        }
    }

}
