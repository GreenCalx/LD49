using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIGarage : MonoBehaviour
{
    private GarageEntry garageEntry;
    private List<GarageUISelectable> selectables;
    private int i_selected;
    private float elapsed_time;
    private bool submenu_is_active;

    /// PUB PARAMS
    // > COLORS
    public Color enabled_category;
    public Color disabled_category;
    public Color entered_category;
    // > MENU PARMS
    public float selector_latch;

    // Start is called before the first frame update
    void Start()
    {
        selectables = new List<GarageUISelectable>(GetComponentsInChildren<GarageUISelectable>());
        if (selectables.Count < 0)
            Debug.LogWarning("No GarageUISelectable found in UIGarage.");
        i_selected = 0;
        elapsed_time = 0f;
        submenu_is_active = false;
        select(i_selected);

        tryReadCurvesFromPlayer();
    }

    // Update is called once per frame
    void Update()
    {
        if (submenu_is_active)
            return;

        if ( elapsed_time > selector_latch )
        {
            float Y = Input.GetAxis("Vertical");
            if ( Y < -0.2f )
            {
                deselect(i_selected);

                i_selected++;
                if ( i_selected > selectables.Count - 1 )
                { i_selected = 0; }
                select(i_selected);
                elapsed_time = 0f;
            }
            else if ( Y > 0.2f )
            {
                deselect(i_selected);

                i_selected--;
                if ( i_selected < 0 )
                { i_selected = selectables.Count - 1; }
                select(i_selected);
                elapsed_time = 0f;
            }
            if (Input.GetButtonDown("Submit"))
                enterSubMenu();
            else if (Input.GetButtonDown("Cancel"))
                quit();
        }
        elapsed_time += Time.unscaledDeltaTime;
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

    private void deselect(int index)
    {
        GameObject target = selectables[i_selected].gameObject;
        TextMeshProUGUI tmp = target.GetComponent<TextMeshProUGUI>();
        if (tmp != null)
        {
            tmp.color = disabled_category;
        }
    }
    private void select(int index)
    {
        GameObject target = selectables[i_selected].gameObject;
        TextMeshProUGUI tmp = target.GetComponent<TextMeshProUGUI>();
        if (tmp != null)
        {
            tmp.color = enabled_category;
        }
    }

    private void enterSubMenu()
    {
        submenu_is_active = true;
        selectables[i_selected].enter();

        // indicate we are navigating in this submenu now
        GameObject target = selectables[i_selected].gameObject;
        TextMeshProUGUI tmp = target.GetComponent<TextMeshProUGUI>();
        if (tmp != null)
        {
            tmp.color = entered_category;
        }
    }

    public void quitSubMenu()
    {
        selectables[i_selected].quit();
        submenu_is_active = false;
        elapsed_time = 0f;

        // turn color back to std selected
        select(i_selected);
    }

    public void setGarageEntry(GarageEntry iGE)
    {
        garageEntry = iGE;
    }

    public void quit()
    {
        if (!!garageEntry)
            garageEntry.closeGarage();
        else
            Destroy(this.gameObject);
    }
}
