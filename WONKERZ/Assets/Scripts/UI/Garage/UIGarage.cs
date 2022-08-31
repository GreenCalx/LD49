using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIGarageCancelablePanel : UITabbedPanel, IUIGarageElement {
    public bool isActivated = false;

    public UIGarageInputHelper inputHelper;

    List<IUIGarageElement.UIGarageHelperValue> IUIGarageElement.getHelperInputs() {
        return getHelperInputs();
    }

    virtual protected List<IUIGarageElement.UIGarageHelperValue> getHelperInputs() {
        return new List<IUIGarageElement.UIGarageHelperValue>{
            new IUIGarageElement.UIGarageHelperValue(Constants.RES_ICON_A, "OK"),
            new IUIGarageElement.UIGarageHelperValue(Constants.RES_ICON_B, "CANCEL")
        };
    }
    
    override protected void ProcessInputs(InputManager.InputData Entry)
    {
        base.ProcessInputs(Entry);

        if (Entry.Inputs["Cancel"].IsDown && isActivated)
            onDeactivate?.Invoke();
    }

    public override void activate()
    {
        base.activate();

        Utils.attachUniqueControllable(this);
        isActivated = true;
        elapsed_time = 0f;

        foreach(UITab t in Tabs) {
            t.onDeselect?.Invoke();
        }
        Tabs[0].onSelect?.Invoke();

        inputHelper.refreshHelper(this);
    }

    public override void deactivate()
    {
        base.deactivate();

        Utils.detachUniqueControllable();
        isActivated = false;

         activator?.onSelect?.Invoke();
    }
    public override void select()
    {
        base.select();
        gameObject.SetActive(true);
        animateIn();
        foreach(UITab t in Tabs) {
            t.onDeselect?.Invoke();
        }
    }

    public override void deselect()
    {
        base.deselect();
        animateOut();
        gameObject.SetActive(false);
    }
}

public class UIGarageCancelableTab : UITab {
    public bool isActivated = false;
    override protected void ProcessInputs(InputManager.InputData Entry)
    {
        base.ProcessInputs(Entry);

        if (Entry.Inputs["Cancel"].IsDown && isActivated)
            onDeactivate?.Invoke();
    }

    override public void select()
    {
        base.select();
        Utils.attachControllable(this);
        isActivated = false;
    }

    override public void deselect()
    {
        base.deselect();
        Utils.detachControllable(this);
        isActivated = false;
    }

    override public void activate()
    {
        base.activate();
        Utils.attachUniqueControllable(this);
        isActivated = true;
    }

    override public void deactivate()
    {
        base.deactivate();
        Utils.detachUniqueControllable();
        isActivated = false;
        activator?.onSelect?.Invoke();
    }

}


public class UIGarage : UIGarageCancelablePanel {
    private GarageEntry garageEntry;

    override public void deactivate() {
        base.deactivate();
        garageEntry.closeGarage();
    }

    public GarageEntry getGarageEntry()
    {
        return garageEntry;
    }

    public void setGarageEntry(GarageEntry iGE)
    {
        garageEntry = iGE;
    }
}
