using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIGarageActionConfirmPanel : UIGaragePanel, IControllable
{
    public enum ACTION_NATURE {
        NONE = 0,
        LOAD = 1,
        SAVE = 2
    };

    public string action_name;
    public ACTION_NATURE action_nature;

    public TextMeshProUGUI actionTextField;
    private Color enabled_stat;
    private Color disabled_stat;
    private Color selected_stat;
    private int i_action;

    // Delegation
    private Action onConfirm = null;
    public void setConfirmAction( Action methodOnConfirm )
    {
        onConfirm = methodOnConfirm;
    }

    // Start is called before the first frame update
    void Awake()
    {
        init();
        Utils.attachControllable<UIGarageActionConfirmPanel>(this);
    }
    void OnDestroy() {
        Utils.detachControllable<UIGarageActionConfirmPanel>(this);
    }
    private void init()
    {
        if (rootUI==null)
        {
            Debug.LogWarning("rootUI not given. calling GameObject.Find. Fix the prefab.");
            rootUI = GameObject.Find(Constants.GO_UIGARAGE).GetComponent<UIGarage>();
            if (rootUI==null)
            {
                Debug.LogError("ParentUI is null in UIGarageCarStatsPanel!");
            }
        }
        elapsed_time = 0f;
        
        enabled_stat  = rootUI.enabled_category;
        disabled_stat = rootUI.disabled_category;
        selected_stat = rootUI.entered_category;
        
        setTextBoxField(action_name);
    }

    public void setTextBoxField(string iStr)
    {
        if (null==actionTextField)
        {
            Debug.LogWarning("No actionTextField given. TextBox won't be updated.");
            return;
        }
        actionTextField.text = iStr;
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
        i_action = i_selected;
        GameObject target = selectables[i_action].gameObject;
        
        // update text label
        TextMeshProUGUI target_txt = target.GetComponent<TextMeshProUGUI>();
        target_txt.color = selected_stat;

        // call action delegate
        if (onConfirm!=null)
            onConfirm.Invoke();
        else
            Debug.LogError("Failed to invoke onConfirm() method cause onConfirm is NULL");
        close(true);
    }

    public override void handGivenBack()
    {
        base.handGivenBack();
    }
    
    public override void open(bool iAnimate)
    {
        base.open(iAnimate);
        Utils.attachUniqueControllable<UIGarageActionConfirmPanel>(this);
    }
    public override void close(bool iAnimate)
    {
        base.close(iAnimate);
        Utils.detachUniqueControllable<UIGarageActionConfirmPanel>(this);
        if (!!parentUI)
        { 
            UIGaragePanel uigp = parentUI.GetComponent<UIGaragePanel>();
            if (!!uigp)
                uigp.handGivenBack();
            else
                Debug.LogWarning("No parentUI extending UIGaragePanel found.");
        } else {
            rootUI.handGivenBack();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
