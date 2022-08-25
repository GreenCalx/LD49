using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIGarageCosmeticsPanel : UIGaragePanel, IControllable, IUIGarageElement
{
    private int i_cosmetics;

    private Color enabled_cosmetic;
    private Color disabled_cosmetic;
    private Color selected_cosmetic;

    // Start is called before the first frame update
    void Start()
    {
        init();
        initSelector();
     
        Utils.attachControllable<UIGarageCosmeticsPanel>(this);
    }

    private void init()
    {
        if (rootUI==null)
        {
            rootUI = Access.UIGarage();
            if (rootUI==null)
            {
                Debug.LogError("rootUI is null in UIGarageCosmeticsPanel!");
            }
        }
        elapsed_time = 0f;
        
        enabled_cosmetic  = rootUI.enabled_category;
        disabled_cosmetic = rootUI.disabled_category;
        selected_cosmetic = rootUI.entered_category;
    }

    void OnDestroy() {
        Utils.detachControllable<UIGarageCosmeticsPanel>(this);
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


        if (Entry.Inputs[Constants.INPUT_JUMP].IsDown)
            pick();
        if (Entry.Inputs[Constants.INPUT_CANCEL].IsDown)
            close(true);
    }

    Dictionary<string,string> IUIGarageElement.getHelperInputs()
    {
        Dictionary<string,string> retval = new Dictionary<string, string>();

        retval.Add(Constants.RES_ICON_A, "EDIT");
        retval.Add(Constants.RES_ICON_B, "BACK");

        return retval;
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
        target_txt.color = disabled_cosmetic;
    }
    protected override void select(int index)
    {
        GameObject target = selectables[index].gameObject;

        // update text label
        TextMeshProUGUI target_txt = target.GetComponent<TextMeshProUGUI>();
        target_txt.color = enabled_cosmetic;
    }

    public override void handGivenBack()
    {
        base.handGivenBack();
        rootUI.inputHelper.refreshHelper(this);
    }

    public void pick()
    {
        i_cosmetics = i_selected;
        GameObject target = selectables[i_cosmetics].gameObject;
        TextMeshProUGUI target_txt = target.GetComponent<TextMeshProUGUI>();
        target_txt.color = selected_cosmetic;

        selectables[i_cosmetics].enter(this);

        // update input helper according to selectable specialization
        UIGarageColors uigc = target.GetComponent<UIGarageColors>();
        // UIGarageWheels uigw = target.GetComponent<UIGarageWheels>();
        if (!!uigc) {
            rootUI.inputHelper.refreshHelper(uigc);
        }
        // else if (!!uigw) { ... }
    }

    public override void open(bool iAnimate)
    {   
        init();
        base.open(iAnimate);

        Utils.attachUniqueControllable<UIGarageCosmeticsPanel>(this);
        rootUI.inputHelper.refreshHelper(this);

        initSelector();
        i_cosmetics = 0;
        elapsed_time = 0f;
    }

    public override void close(bool iAnimate)
    {
        base.close(iAnimate);
        Utils.detachUniqueControllable<UIGarageCosmeticsPanel>(this);
        deselect(i_cosmetics);
        rootUI.handGivenBack();
    }
}
