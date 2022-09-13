using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIPanelTabbed : UIPanelControlable
{
    public List<UITab> tabs;
    protected int selected;
    // define the navigation mode
    public enum NavigationMode { Horizontal, Vertical };
    public NavigationMode navigationMode;

    override protected void ProcessInputs(InputManager.InputData Entry){
        base.ProcessInputs(Entry);
        if (elapsed_time > selector_latch)
        {
            float X = 0;
            if (navigationMode == NavigationMode.Horizontal)
            {
                X = Entry.Inputs[Constants.INPUT_TURN].AxisValue;
            }
            else if (navigationMode == NavigationMode.Vertical)
            {
                X = Entry.Inputs[Constants.INPUT_UIUPDOWN].AxisValue;
            }
            if (X < -0.2f)
            {
                SelectTab(PreviousTab());
                elapsed_time = 0f;
            }
            else if (X > 0.2f)
            {
                SelectTab(NextTab());
                elapsed_time = 0f;
            }

        }
        elapsed_time += Time.unscaledDeltaTime;
    }

    public override void activate()
    {
        base.activate();

        foreach(UITab t in tabs) {
            t.onDeselect?.Invoke();
        }

        tabs[0].onSelect?.Invoke();
    }

    public override void deactivate()
    {
        base.deactivate();

        foreach(UITab t in tabs) {
            t.onDeselect?.Invoke();
        }

        activator?.onDeactivate?.Invoke();
    }

    public override void select()
    {
        base.select();

        foreach(UITab t in tabs) {
            t.onDeselect?.Invoke();
        }
    }


    public int NextTab() { if (tabs.Count == 0) return 0; return (selected + 1) % tabs.Count; }
    public int PreviousTab() { if (tabs.Count == 0) return 0; return (selected - 1 + tabs.Count) % tabs.Count; }
    public int CurrentTab() { return selected; }

    public void SelectTab(int index)
    {
        if (tabs.Count == 0) return;

        GetTab(CurrentTab()).onDeselect?.Invoke();
        selected = index;
        GetTab(CurrentTab()).onSelect?.Invoke();
    }
    public UITab GetTab(int index) { if (tabs.Count == 0) return null; return tabs[selected]; }
}
