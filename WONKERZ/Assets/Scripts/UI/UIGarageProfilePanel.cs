using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIGarageProfilePanel :  UIGaragePanel, IControllable
{
    //public int n_slots = 5;    
    [Header("MAND")]
    private UIGarageProfile profile;
    public UIGarageActionConfirmPanel confirmPanel;
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
        if (rootUI==null)
        {
            rootUI = GameObject.Find(Constants.GO_UIGARAGE).GetComponent<UIGarage>();
            if (rootUI==null)
            {
                Debug.LogError("rootUI is null in UIGarageCarStatsPanel!");
            }
        }
        elapsed_time = 0f;
        
        enabled_stat  = rootUI.enabled_category;
        disabled_stat = rootUI.disabled_category;
        selected_stat = rootUI.entered_category;

        profile = GetComponent<UIGarageProfile>();
        if (null==profile)
        {
            Debug.LogWarning("UIGarageProfile is missing on UIGarageProfilePanel.");
        }
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
            save();
        /*if (Entry.Inputs[Constants.<...>].IsDown)
            load();*/
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
    public void save()
    {
        i_profile = i_selected;
        GameObject target = selectables[i_profile].gameObject;
        
        // update text label
        TextMeshProUGUI target_txt = target.GetComponent<TextMeshProUGUI>();
        target_txt.color = selected_stat;

        UIGaragePickableProfile pickable_profile = target.GetComponent<UIGaragePickableProfile>();
        if (null==pickable_profile)
            return;

        if (!!confirmPanel)
        {
            confirmPanel.gameObject.SetActive(true);
            confirmPanel.parentUI = this.gameObject;
            confirmPanel.open(true);
            confirmPanel.action_name = "SAVE ?";
            confirmPanel.setConfirmAction(() => SaveAndLoad.save(pickable_profile.profile_name));
        }

    }

    public void load()
    {
        i_profile = i_selected;
        GameObject target = selectables[i_profile].gameObject;
        
        // update text label
        TextMeshProUGUI target_txt = target.GetComponent<TextMeshProUGUI>();
        target_txt.color = selected_stat;

        UIGaragePickableProfile pickable_profile = target.GetComponent<UIGaragePickableProfile>();
        if (null==pickable_profile)
            return;

        if (!!confirmPanel)
        {
            confirmPanel.gameObject.SetActive(true);
            confirmPanel.open(true);
            confirmPanel.parentUI = this.gameObject;
            confirmPanel.action_name = "LOAD ?";
            confirmPanel.setConfirmAction(() => SaveAndLoad.load(pickable_profile.profile_name));
        }        
    }

    public override void handGivenBack()
    {
        confirmPanel.gameObject.SetActive(false);
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
        rootUI.handGivenBack();
    }
}
