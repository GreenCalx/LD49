using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIGarageSelector : MonoBehaviour
{
    public float selector_latch;
    protected List<GarageUISelectable> selectables;
    protected int i_selected;

    void Start()
    {
        initSelector();
    }

    protected void initSelector()
    {
        selectables = new List<GarageUISelectable>(GetComponentsInChildren<GarageUISelectable>());
        if (selectables.Count < 0)
            Debug.LogWarning("No GarageUISelectable found in UIGarage.");
        i_selected = 0;

        for (int i=0;i<selectables.Count;i++)
        { deselect(i);}
        select(i_selected);
    }

    protected void selectNext()
    {
        deselect(i_selected);

        i_selected++;
        if ( i_selected > selectables.Count - 1 )
        { i_selected = 0; }
        select(i_selected);
    }

    protected void selectPrevious()
    {
        deselect(i_selected);

        i_selected--;
        if ( i_selected < 0 )
        { i_selected = selectables.Count - 1; }
        select(i_selected);

    }

    protected GarageUISelectable getSelected()
    {
        return selectables[i_selected];
    }

    protected virtual void deselect(int index) { }
    protected virtual void select(int index) {}
}