using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GarageUICarStats : GarageUISelectable
{
    private List<UIGaragePickableStat> stats;
    public float selector_latch;
    private float elapsed_time;
    private int i_stat;

    private Color enabled_stat;
    private Color disabled_stat;
    private UIGarageCurve curve;
    
    // Start is called before the first frame update
    void Start()
    {
        findParent();
        enabled_stat    = parent.enabled_category;
        disabled_stat   = parent.disabled_category;
    }

    // Update is called once per frame
    void Update()
    {
        if (!is_active)
            return;
        
        if ( elapsed_time > selector_latch )
        {
            float Y = Input.GetAxis("Vertical");
            if ( Y > 0.2f )
            {
                deselect(i_stat);

                i_stat++;
                if ( i_stat > stats.Count - 1 )
                { i_stat = 0; }
                select(i_stat);
                elapsed_time = 0f;
            }
            else if ( Y < -0.2f )
            {
                deselect(i_stat);

                i_stat--;
                if ( i_stat < 0 )
                { i_stat = stats.Count - 1; }
                select(i_stat);
                elapsed_time = 0f;
            }
        }
        elapsed_time += Time.unscaledDeltaTime;


        if (Input.GetButtonDown("Submit"))
            pick();
        else if (Input.GetButtonDown("Cancel"))
        { parent.quitSubMenu(); return;}
    }

    private void deselect(int index)
    {
        GameObject target = stats[index].gameObject;

        // update text label
        TextMeshProUGUI target_txt = target.GetComponent<TextMeshProUGUI>();
        target_txt.color = disabled_stat;
    }
    private void select(int index)
    {
        UIGaragePickableStat curr_stat = stats[index];
        GameObject target = stats[index].gameObject;

        // update text label
        TextMeshProUGUI target_txt = target.GetComponent<TextMeshProUGUI>();
        target_txt.color = enabled_stat;

        // update X/Y Labels of graph
        curve.setLabels( curr_stat.XLabel, curr_stat.YLabel);

        // update displayed curve
        curve.changeCurve( curr_stat.car_param );
    }

    public void pick()
    {
        // control slider
        GameObject target = stats[i_stat].gameObject;

    }

    public override void enter()
    {
        base.enter();
        curve = parent.GetComponentInChildren<UIGarageCurve>();
        stats = new List<UIGaragePickableStat>(GetComponentsInChildren<UIGaragePickableStat>());
        if (stats.Count == 0)
        { base.quit(); return;}
        i_stat = 0;
        elapsed_time = 0f;
        select(i_stat);
    }

    public override void quit()
    {
        base.quit();
        deselect(i_stat);
    }
}
