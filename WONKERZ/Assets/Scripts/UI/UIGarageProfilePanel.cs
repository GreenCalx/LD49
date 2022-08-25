using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIGarageProfilePanel :  UIGaragePanel, IControllable, IUIGarageElement
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
    void Awake()
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
            rootUI = Access.UIGarage();
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
        if (Entry.Inputs[Constants.INPUT_RESPAWN].IsDown)
            load();
        if (Entry.Inputs[Constants.INPUT_CANCEL].IsDown)
            close(true);
    }

    Dictionary<string,string> IUIGarageElement.getHelperInputs()
    {
        Dictionary<string,string> retval = new Dictionary<string, string>();

        retval.Add(Constants.RES_ICON_A, "SAVE");
        retval.Add(Constants.RES_ICON_B, "CANCEL");
        retval.Add(Constants.RES_ICON_Y, "LOAD");

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
        SaveAndLoad.datas.Add(profile);

        i_profile = i_selected;
        GameObject target = selectables[i_profile].gameObject;
        
        // update text label
        TextMeshProUGUI target_txt = target.GetComponent<TextMeshProUGUI>();
        target_txt.color = selected_stat;

        UIGaragePickableProfile pickable_profile = target.GetComponent<UIGaragePickableProfile>();
        if (null==pickable_profile)
            return;
        
        fillProfileFromPlayerCC();

        if (!!confirmPanel)
        {
            confirmPanel.gameObject.SetActive(true);
            confirmPanel.parentUI = this.gameObject;
            confirmPanel.open(true);
            confirmPanel.setTextBoxField("SAVE ?");
            confirmPanel.action_nature = UIGarageActionConfirmPanel.ACTION_NATURE.SAVE;
            confirmPanel.setConfirmAction(() => SaveAndLoad.save(pickable_profile.profile_name));
        }

    }

    public void fillProfileFromPlayerCC()
    {
        CarController cc = rootUI.getPlayerCC();
        if (!!cc)
        {
            // TORQUE
            profile.TORQUE_CURVE = new List<Keyframe>(cc.TORQUE.keys);
            // WEIGHT
            profile.WEIGHT_CURVE = new List<Keyframe>(cc.WEIGHT.keys);
            // COLOR
            profile.color = PlayerColorManager.Instance.getCurrentColor();
            
        } else {
            Debug.LogError("Failed to retrieve player CC from rootUI");
        }
    }

    public void load()
    {
        SaveAndLoad.datas.Add(profile);

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
            confirmPanel.setTextBoxField("LOAD ?");
            confirmPanel.action_nature = UIGarageActionConfirmPanel.ACTION_NATURE.LOAD;
            confirmPanel.setConfirmAction(() => SaveAndLoad.loadGarageProfile(pickable_profile.profile_name, profile));
        }
    }

    public void updatePlayerFromProfile()
    {
        CarController cc = rootUI.getPlayerCC();
        if (!!cc)
        {
            // TORQUE
            cc.TORQUE = new AnimationCurve(profile.TORQUE_CURVE.ToArray());
            // WEIGHT
            cc.WEIGHT = new AnimationCurve(profile.WEIGHT_CURVE.ToArray());
            //color
            PlayerColorManager.Instance.colorize(profile.color);
        } else {
            Debug.LogError("Failed to retrieve player CC from rootUI");
        }
    }

    public override void handGivenBack()
    {
        if (confirmPanel.action_nature == UIGarageActionConfirmPanel.ACTION_NATURE.LOAD )
        {
            updatePlayerFromProfile();
        }
        confirmPanel.gameObject.SetActive(false);
        base.handGivenBack();
    }
    public override void open(bool iAnimate)
    {
        base.open(iAnimate);
        Utils.attachUniqueControllable<UIGarageProfilePanel>(this);
        rootUI.inputHelper.refreshHelper(this);
    }
    public override void close(bool iAnimate)
    {
        base.close(iAnimate);
        Utils.detachUniqueControllable<UIGarageProfilePanel>(this);
        SaveAndLoad.datas.Remove(this);
        rootUI.handGivenBack();
    }
}
