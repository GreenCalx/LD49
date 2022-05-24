using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIGarageCarStatsPanel : UIGaragePanel, IControllable
{
    public GameObject UIGarageCurvePicker_Ref;

    private GameObject UIGarageCurvePicker_Inst;
    private int i_stat;

    private Color enabled_stat;
    private Color disabled_stat;
    private Color selected_stat;
    private UIGarageCurve curve;

    // Start is called before the first frame update
    void Start()
    {
        init();
        initSelector();
     


        Utils.attachControllable<UIGarageCarStatsPanel>(this);
    }

    private void init()
    {
        if (parentUI==null)
        {
            parentUI = GameObject.Find(Constants.GO_UIGARAGE).GetComponent<UIGarage>();
            if (parentUI==null)
            {
                Debug.LogError("ParentUI is null in UIGarageCarStatsPanel!");
            }
        }
        curve = displayPanel.GetComponentInChildren<UIGarageCurve>();
        elapsed_time = 0f;
        
        enabled_stat  = parentUI.enabled_category;
        disabled_stat = parentUI.disabled_category;
        selected_stat = parentUI.entered_category;
    }

    void OnDestroy() {
        Utils.detachControllable<UIGarageCarStatsPanel>(this);
    }

    void IControllable.ProcessInputs(InputManager.InputData Entry) {
        if (elapsed_time > selector_latch)
        {
            float Y = Entry.Inputs[Constants.INPUT_UIUPDOWN].AxisValue;
            if (Y < -0.2f)
            {
                selectPrevious();
                elapsed_time = 0f;
            }
            else if ( Y > 0.2f )
            {
                selectNext();
                elapsed_time = 0f;
            }
        }
        elapsed_time += Time.unscaledDeltaTime;


        if (Entry.Inputs["Jump"].IsDown)
            pick();
        if (Entry.Inputs[Constants.INPUT_CANCEL].IsDown)
            close(true);
    }

    // Update is called once per frame
    void Update()
    {
    }

    protected override void deselect(int index)
    {
        GameObject target = selectables[index].gameObject;

        // update text label
        TextMeshProUGUI target_txt = target.GetComponent<TextMeshProUGUI>();
        target_txt.color = disabled_stat;

        // update background if it exists
        Image bg = target.GetComponentInChildren<Image>();
        if (!!bg)
            bg.color = new Color( bg.color.r, bg.color.g, bg.color.b, 0.3f);
    }
    protected override void select(int index)
    {
        GameObject target = selectables[index].gameObject;
        UIGaragePickableStat curr_stat = target.GetComponent<UIGaragePickableStat>();

        // update text label
        TextMeshProUGUI target_txt = target.GetComponent<TextMeshProUGUI>();
        target_txt.color = enabled_stat;

        // update background if it exists
        Image bg = target.GetComponentInChildren<Image>();
        if (!!bg)
            bg.color = new Color( bg.color.r, bg.color.g, bg.color.b, 0.8f);

        // update X/Y Labels of graph
        curve.setLabels(curr_stat.XLabel, curr_stat.YLabel);

        // update displayed curve
        curve.changeCurve(curr_stat.car_param);

        // Set Slider to the right keyframe/curve
        GameObject player = parentUI.getGarageEntry().playerRef;
        CarController cc = player.GetComponent<CarController>();
        KeyValuePair<AnimationCurve, int> kvp = cc.getCurveKVP(curr_stat.car_param);
        setCurveSlider(curr_stat.car_param, kvp.Value);
    }

    public override void handGivenBack()
    {
        base.handGivenBack();
    }

    public void pick()
    {
        i_stat = i_selected;
        GameObject target = selectables[i_stat].gameObject;

        // update text label
        TextMeshProUGUI target_txt = target.GetComponent<TextMeshProUGUI>();
        target_txt.color = selected_stat;

        // write bnew curve in car controller
        GameObject player = parentUI.getGarageEntry().playerRef;
        CarController cc = player.GetComponent<CarController>();

        UICurveSelector uics = UIGarageCurvePicker_Inst.GetComponent<UICurveSelector>();
        uics.enter(this);
        Utils.GetInputManager().SetUnique(uics as IControllable);

        cc.setCurve(curve.getSelectedCurve(), curve.selected_parm);
        UIGarageCurvePicker_Inst.SetActive(true);
    }

    public void setCurveSlider(UIGarageCurve.CAR_PARAM parm, int keyFrameID)
    {
        // instantiate slider
        if (UIGarageCurvePicker_Inst == null)
            UIGarageCurvePicker_Inst = Instantiate(UIGarageCurvePicker_Ref, parentUI.gameObject.transform);

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

        // make the cursor invisible while curve not select
        UIGarageCurvePicker_Inst.SetActive(false);

    }

    public void notifySliderMove(int iMovableKey)
    {
        UICurveSelector uics = UIGarageCurvePicker_Inst.GetComponent<UICurveSelector>();

        // you need to take into account the initial position you fool!!!!!
        float new_time = Mathf.Clamp(uics.transform.position.x - uics.XLeftBound, 0, uics.XRightBound - uics.XLeftBound) / curve.lineRenderer.unitWidth;
        //float new_time = uics.transform.position.x / curve.lineRenderer.unitWidth;
        curve.moveCurveKey(uics.movable_key, new_time);
    }

    public override void open(bool iAnimate)
    {   
        init();
        base.open(iAnimate);

        Utils.GetInputManager().SetUnique(this as IControllable);

        // Read curves from CarController
        GameObject player = parentUI.getGarageEntry().playerRef;
        CarController cc = player.GetComponent<CarController>();
        curve.setTorqueCurve(cc.TORQUE);
        curve.setWeightCurve(cc.WEIGHT);

        selectables = new List<UIGarageSelectable>(GetComponentsInChildren<UIGarageSelectable>());
        if (selectables.Count == 0)
        { close(true); return; }
        i_stat = 0;
        elapsed_time = 0f;
    }

    public override void close(bool iAnimate)
    {
        base.close(iAnimate);
        Utils.GetInputManager().UnsetUnique(this as IControllable);
        deselect(i_stat);
        if (!!UIGarageCurvePicker_Inst)
            Destroy(UIGarageCurvePicker_Inst);
        parentUI.handGivenBack();
    }
}
