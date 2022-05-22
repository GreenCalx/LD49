using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIGarageCosmeticsPanel : UIGaragePanel, IControllable
{
    private int i_cosmetics;

    private Color enabled_stat;
    private Color disabled_stat;

    public UIGarage parentUI;

    // Start is called before the first frame update
    void Start()
    {
        init();
        initSelector();
     
        Utils.attachControllable<UIGarageCosmeticsPanel>(this);
    }

    private void init()
    {
        if (parentUI==null)
        {
            parentUI = GameObject.Find(Constants.GO_UIGARAGE).GetComponent<UIGarage>();
            if (parentUI==null)
            {
                Debug.LogError("ParentUI is null in UIGarageCosmeticsPanel!");
            }
        }
        elapsed_time = 0f;
        
        enabled_stat  = parentUI.enabled_category;
        disabled_stat = parentUI.disabled_category;
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


        if (Entry.Inputs["Jump"].IsDown)
            pick();
        if (Entry.Inputs[Constants.INPUT_CANCEL].IsDown)
            close();
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
        GameObject target = selectables[i_cosmetics].gameObject;
        //UIGarageSelector cosmetic_selector = target.GetComponent<UIGarageSelector>();
        selectables[i_cosmetics].enter();

        //Utils.GetInputManager().SetUnique(cosmetic_selector as IControllable);
    }

    public override void open()
    {   
        init();
        initSelector();

        Utils.GetInputManager().SetUnique(this as IControllable);

        initSelector();
        i_cosmetics = 0;
        elapsed_time = 0f;
    }

    public override void close()
    {
        base.close();
        Utils.GetInputManager().UnsetUnique(this as IControllable);
        deselect(i_cosmetics);
    }
}
