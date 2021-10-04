using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UICheckpoint : MonoBehaviour
{
    public TMPro.TextMeshProUGUI cp_text    ;
    public TMPro.TextMeshProUGUI cp_name_textval;
    public TMPro.TextMeshProUGUI cp_time_textval;
    public float PANEL_DURATION = 3f;
    public float CP_INFO_DURATION = 1.5f;

    private bool is_enabled = false;
    private float display_start_time;


    void Start()
    {
        disable();
    }

    public void disable()
    {
        cp_text.gameObject.active = false;
        cp_name_textval.gameObject.active = false;
        cp_time_textval.gameObject.active = false;
        is_enabled = false;
    }

    public void enable()
    {
        cp_text.gameObject.active = true;
        cp_name_textval.gameObject.active = true;
        cp_time_textval.gameObject.active = true;
        is_enabled = true;
    }

    public void disable_cpinfo()
    {
        cp_text.gameObject.active = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (is_enabled)
        {
            if ((Time.time - display_start_time) >= CP_INFO_DURATION )
                disable_cpinfo();
            if ((Time.time - display_start_time) >= PANEL_DURATION )
                disable();
        }
    }

    public void displayCP(GameObject iGO)
    {
        CheckPoint cp = iGO.GetComponent<CheckPoint>();
        if (!!cp)
        {
            int racetime_val_min = (int)(cp.cpm.finishLine.racetime / 60);
            int racetime_val_sec = (int)(cp.cpm.finishLine.racetime % 60);

            cp_name_textval.SetText( cp.name );
            cp_time_textval.SetText( racetime_val_min.ToString() + " m " + racetime_val_sec.ToString() + " s" );
            enable();
            display_start_time = Time.time;
        }
    }
}
