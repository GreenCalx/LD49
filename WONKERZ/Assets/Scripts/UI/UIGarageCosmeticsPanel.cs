using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIGarageCosmeticsPanel : UIGaragePanel, IControllable
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
        if (parentUI==null)
        {
            parentUI = GameObject.Find(Constants.GO_UIGARAGE).GetComponent<UIGarage>();
            if (parentUI==null)
            {
                Debug.LogError("ParentUI is null in UIGarageCosmeticsPanel!");
            }
        }
        elapsed_time = 0f;
        
        enabled_cosmetic  = parentUI.enabled_category;
        disabled_cosmetic = parentUI.disabled_category;
        selected_cosmetic = parentUI.entered_category;
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
    }

    public void pick()
    {
        GameObject target = selectables[i_cosmetics].gameObject;
        TextMeshProUGUI target_txt = target.GetComponent<TextMeshProUGUI>();
        target_txt.color = selected_cosmetic;

        selectables[i_cosmetics].enter(this);
    }

    public override void open(bool iAnimate)
    {   
        init();
        base.open(iAnimate);
        Utils.GetInputManager().SetUnique(this as IControllable);

        initSelector();
        i_cosmetics = 0;
        elapsed_time = 0f;
    }

    public override void close(bool iAnimate)
    {
        base.close(iAnimate);
        Utils.GetInputManager().UnsetUnique(this as IControllable);
        deselect(i_cosmetics);
        parentUI.handGivenBack();
    }
}
