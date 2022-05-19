using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GarageUICarStats : GarageUISelectable, IControllable
{
    public float selector_latch;
    public GameObject UIGarageCurvePicker_Ref;

    private GameObject UIGarageCurvePicker_Inst;
    private List<UIGaragePickableStat> stats;
    private float elapsed_time;
    private int i_stat;

    private Color enabled_stat;
    private Color disabled_stat;
    private UIGarageCurve curve;

    // Start is called before the first frame update
    void Start()
    {
        findParent();
        enabled_stat = parent.enabled_category;
        disabled_stat = parent.disabled_category;

        Utils.attachControllable<GarageUICarStats>(this);
    }

    void OnDestroy() {
        Utils.detachControllable<GarageUICarStats>(this);
    }

    void IControllable.ProcessInputs(InputManager.InputData Entry) {
        if (elapsed_time > selector_latch)
        {
            float Y = Entry.Inputs["Accelerator"].AxisValue;
            if (Y > 0.2f)
            {
                deselect(i_stat);

                i_stat++;
                if (i_stat > stats.Count - 1)
                { i_stat = 0; }
                select(i_stat);
                elapsed_time = 0f;
            }
            else if (Y < -0.2f)
            {
                deselect(i_stat);

                i_stat--;
                if (i_stat < 0)
                { i_stat = stats.Count - 1; }
                select(i_stat);
                elapsed_time = 0f;
            }
        }
        elapsed_time += Time.unscaledDeltaTime;


        if (Entry.Inputs["Jump"].IsDown)
            pick();
        else if (Entry.Inputs["Cancel"].IsDown)
        {
            parent.quitSubMenu(); return;
        }
    }

    // Update is called once per frame
    void Update()
    {
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
        curve.setLabels(curr_stat.XLabel, curr_stat.YLabel);

        // update displayed curve
        curve.changeCurve(curr_stat.car_param);

        // Set Slider to the right keyframe/curve
        GameObject player = parent.getGarageEntry().playerRef;
        CarController cc = player.GetComponent<CarController>();
        KeyValuePair<AnimationCurve, int> kvp = cc.getCurveKVP(curr_stat.car_param);
        setCurveSlider(curr_stat.car_param, kvp.Value);


    }

    public void pick()
    {
        GameObject target = stats[i_stat].gameObject;

        // write bnew curve in car controller
        GameObject player = parent.getGarageEntry().playerRef;
        CarController cc = player.GetComponent<CarController>();

        UICurveSelector uics = UIGarageCurvePicker_Inst.GetComponent<UICurveSelector>();

        GameObject.Find(Constants.GO_MANAGERS).GetComponent<InputManager>().SetUnique(uics as IControllable);

        cc.setCurve(curve.getSelectedCurve(), curve.selected_parm);
    }

    public void setCurveSlider(UIGarageCurve.CAR_PARAM parm, int keyFrameID)
    {
        // instantiate slider
        if (UIGarageCurvePicker_Inst == null)
            UIGarageCurvePicker_Inst = Instantiate(UIGarageCurvePicker_Ref, this.transform);

        // set slider X/Y position
        // > Get Grid
        UIGridRenderer uigr = curve.lineRenderer.gridRenderer;
        RectTransform rectTransform = uigr.GetComponent<RectTransform>();

        // > Y to grid bottom line / X to 0
        UIGarageCurvePicker_Inst.transform.position = uigr.transform.position;

        // Set X bounds
        UICurveSelector uics = UIGarageCurvePicker_Inst.GetComponent<UICurveSelector>();
        uics.XLeftBound = uigr.transform.position.x;
        uics.XRightBound = uics.XLeftBound + uigr.getWidth();
        uics.movable_key = keyFrameID;
        uics.observer = this;

        // > X to current CarController Value
        float x_offset = curve.getSelectedCurve().keys[keyFrameID].time;
        x_offset *= curve.lineRenderer.unitWidth;
        uics.transform.position += new Vector3(x_offset, 0, 0);

        // Compute XBound+/XBound-
        curve.refreshMovableKeyBounds(keyFrameID);
        uics.XKeyLeftBound     = uics.XLeftBound  + (curve.minbound_nomerge * curve.lineRenderer.unitWidth);
        uics.XKeyRightBound    = uics.XRightBound - (curve.maxbound_nomerge * curve.lineRenderer.unitWidth);

        // deactivate this UI while controlling the slider

    }

    public void notifySliderMove(int iMovableKey)
    {
        UICurveSelector uics = UIGarageCurvePicker_Inst.GetComponent<UICurveSelector>();

        // you need to take into account the initial position you fool!!!!!
        float new_time = Mathf.Clamp(uics.transform.position.x - uics.XLeftBound, 0, uics.XRightBound - uics.XLeftBound) / curve.lineRenderer.unitWidth;
        //float new_time = uics.transform.position.x / curve.lineRenderer.unitWidth;
        curve.moveCurveKey(uics.movable_key, new_time);
    }

    public override void enter()
    {
        base.enter();

        GameObject.Find(Constants.GO_MANAGERS).GetComponent<InputManager>().SetUnique(this as IControllable);
        curve = parent.GetComponentInChildren<UIGarageCurve>();

        // Read curves from CarController
        GameObject player = parent.getGarageEntry().playerRef;
        CarController cc = player.GetComponent<CarController>();
        curve.setTorqueCurve(cc.TORQUE);
        curve.setWeightCurve(cc.WEIGHT);

        stats = new List<UIGaragePickableStat>(GetComponentsInChildren<UIGaragePickableStat>());
        if (stats.Count == 0)
        { base.quit(); return; }
        i_stat = 0;
        elapsed_time = 0f;
        select(i_stat);
    }

    public override void quit()
    {
        base.quit();
        GameObject.Find(Constants.GO_MANAGERS).GetComponent<InputManager>().UnsetUnique(this as IControllable);
        deselect(i_stat);
        Destroy(UIGarageCurvePicker_Inst);
    }
}
