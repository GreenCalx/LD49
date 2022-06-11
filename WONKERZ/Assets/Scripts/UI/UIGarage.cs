using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIGarage : UIGarageSelector, IControllable
{
    private GarageEntry garageEntry;
    private float elapsed_time;
    private int i_entered_category;

    /// PUB PARAMS
    // > COLORS
    public Color enabled_category;
    public Color disabled_category;
    public Color entered_category;
    // > MENU PARMS

    // Start is called before the first frame update
    void Start()
    {
        elapsed_time = 0f;
        i_entered_category = -1;

        initSelector();

        tryReadCurvesFromPlayer();

        Utils.attachControllable<UIGarage>(this);
    }

    void OnDestroy() {
        Utils.detachControllable<UIGarage>(this);
    }

    void IControllable.ProcessInputs(InputManager.InputData Entry) {
        if ( elapsed_time > selector_latch )
        {
            float X = Entry.Inputs[Constants.INPUT_TURN].AxisValue;
            if ( X < -0.2f )
            {
                selectPrevious();
                elapsed_time = 0f;
            }
            else if ( X > 0.2f )
            {
                selectNext();
                elapsed_time = 0f;
            }
            if (Entry.Inputs[Constants.INPUT_JUMP].IsDown)
                enterSubMenu();
            else if(Entry.Inputs[Constants.INPUT_CANCEL].IsDown)
                quitGarage();
            else if( Entry.Inputs[Constants.INPUT_CANCEL].IsDown && (i_entered_category>=0) )
                quitSubMenu();
        }
        elapsed_time += Time.unscaledDeltaTime;
    }

    // Update is called once per frame
    void Update()
    {
    }

    private void tryReadCurvesFromPlayer()
    {
        if (garageEntry.playerRef!=null)
        {
            CarController cc = garageEntry.playerRef.GetComponent<CarController>();
            // set animation curves from player...
            // torque_curve = cc.torque_curve;
        }
    }

    public void tryWriteCurvesInPlayer()
    {
        if (garageEntry.playerRef!=null)
        {
            CarController cc = garageEntry.playerRef.GetComponent<CarController>();
            // set player curve from given curve ?
            // cc.torque_curve = torque_curve;
        } 
    }

    protected override void deselect(int index)
    {
        GameObject target = selectables[index].gameObject;
        Image tmp = target.GetComponent<Image>();
        if (tmp != null)
        {
            tmp.color = disabled_category;
        }
    }
    protected override void select(int index)
    {
        GameObject target = selectables[index].gameObject;
        Image tmp = target.GetComponent<Image>();
        if (tmp != null)
        {
            tmp.color = enabled_category;
        }
    }

    public override void handGivenBack()
    {
        base.handGivenBack();
    }

    private void enterSubMenu()
    {
        if (i_entered_category >= 0)
            quitSubMenu();

        i_entered_category = i_selected;

        selectables[i_entered_category].enter(this);

        // indicate we are navigating in this submenu now
        GameObject target = getSelected().gameObject;
        Image tmp = target.GetComponent<Image>();
        if (tmp != null)
        {
            tmp.color = entered_category;
        }
    }

    public void quitSubMenu()
    {
        if (i_entered_category < 0)
            return;

        selectables[i_entered_category].quit();
        
        i_entered_category = -1;
    }

    public void setGarageEntry(GarageEntry iGE)
    {
        garageEntry = iGE;
    }

    public GarageEntry getGarageEntry()
    {
        return garageEntry;
    }

    public CarController getPlayerCC()
    {
        return garageEntry.playerCC;
    }

    public void quitGarage()
    {
        if (!!garageEntry)
            garageEntry.closeGarage();
        else
            Destroy(this.gameObject);
    }
}
