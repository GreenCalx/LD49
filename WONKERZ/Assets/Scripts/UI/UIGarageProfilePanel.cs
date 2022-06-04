using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIGarageProfilePanel :  UIGaragePanel, IControllable
{
    public int n_slots = 5;

    private int i_profile;


    private Color enabled_stat;
    private Color disabled_stat;
    private Color selected_stat;
    
    // Start is called before the first frame update
    void Start()
    {
        init();
        Utils.attachControllable<UIGarageProfilePanel>(this);
    }

    void OnDestroy() {
        Utils.detachControllable<UIGarageProfilePanel>(this);
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
        elapsed_time = 0f;
        
        enabled_stat  = parentUI.enabled_category;
        disabled_stat = parentUI.disabled_category;
        selected_stat = parentUI.entered_category;
    }

    void IControllable.ProcessInputs(InputManager.InputData Entry) 
    {
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
    }
    protected override void select(int index)
    {
        GameObject target = selectables[index].gameObject;

        // update text label
        TextMeshProUGUI target_txt = target.GetComponent<TextMeshProUGUI>();
        target_txt.color = enabled_stat;
    }
    public void pick()
    {
        i_profile = i_selected;
        GameObject target = selectables[i_profile].gameObject;
        
        // update text label
        TextMeshProUGUI target_txt = target.GetComponent<TextMeshProUGUI>();
        target_txt.color = selected_stat;

        // ::: TODO ::: 
        // SAVE : 
        // 1. Create UIGarageProfile
        // 2. Fill it up with player stats
        // 3. Call SaveAndLoad.save()

        // LOAD :
        // 1. Create UIGarageProfile
        // 2. Call SaveAndLoad.load()
        // 3. transfer loaded datas of UIGarageProfile to player
    }

    public override void handGivenBack()
    {
        base.handGivenBack();
    }
    public override void open(bool iAnimate)
    {
        base.open(iAnimate);
        Utils.GetInputManager().SetUnique(this as IControllable);
    }
    public override void close(bool iAnimate)
    {
        base.close(iAnimate);
        Utils.GetInputManager().UnsetUnique(this as IControllable);
        parentUI.handGivenBack();
    }
}
