using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class UIGarageTestingPanel : UIGaragePanel, IControllable, IUIGarageElement
{
    private int i_test;

    private Color enabled_test;
    private Color disabled_test;
    private Color selected_test;

    private bool test_is_running = false;
    // Start is called before the first frame update
    void Start()
    {
        init();
        initSelector();
     
        Utils.attachControllable<UIGarageTestingPanel>(this);
    }

    private void init()
    {
        if (rootUI==null)
        {
            rootUI = GameObject.Find(Constants.GO_UIGARAGE).GetComponent<UIGarage>();
            if (rootUI==null)
            {
                Debug.LogError("rootUI is null in UIGarageCosmeticsPanel!");
            }
        }
        elapsed_time = 0f;
        
        enabled_test  = rootUI.enabled_category;
        disabled_test = rootUI.disabled_category;
        selected_test = rootUI.entered_category;
        
        test_is_running = false;
    }

    void OnDestroy() {
        Utils.detachControllable<UIGarageTestingPanel>(this);
    }

    void Update()
    {
        // Hack to quit test simulation
        // when InputManager is in autopilot
        // and test car is the unique controllable
        // It causes NPE on quit because its called multiple times
        // here, which is not the case with input manager
        // > will be fixed by moving onto InputManager
        if (Input.GetKey(KeyCode.Escape))
        {
            selectables[i_test].quit();
            test_is_running = false;
        }
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
        {
            if (!test_is_running)
                close(true);
            else
                selectables[i_test].quit();
            test_is_running = false;
        }
    }

    Dictionary<string,string> IUIGarageElement.getHelperInputs()
    {
        Dictionary<string,string> retval = new Dictionary<string, string>();

        retval.Add(Constants.RES_ICON_A, "PLAY");
        retval.Add(Constants.RES_ICON_B, "BACK");

        return retval;
    }

    protected override void deselect(int index)
    {
        GameObject target = selectables[index].gameObject;

        // update text label
        TextMeshProUGUI target_txt = target.GetComponent<TextMeshProUGUI>();
        target_txt.color = disabled_test;
    }
    protected override void select(int index)
    {
        GameObject target = selectables[index].gameObject;

        // update text label
        TextMeshProUGUI target_txt = target.GetComponent<TextMeshProUGUI>();
        target_txt.color = enabled_test;
    }

    public override void handGivenBack()
    {
        base.handGivenBack();
        rootUI.inputHelper.refreshHelper(this);
    }

    public void pick()
    {
        i_test = i_selected;

        GameObject target = selectables[i_test].gameObject;
        TextMeshProUGUI target_txt = target.GetComponent<TextMeshProUGUI>();
        target_txt.color = selected_test;

        selectables[i_test].enter(this);
        test_is_running = true;
    }

    public override void open(bool iAnimate)
    {   
        init();
        base.open(iAnimate);

        Utils.GetInputManager().SetUnique(this as IControllable);
        rootUI.inputHelper.refreshHelper(this);

        initSelector();
        i_test = 0;
        elapsed_time = 0f;
    }

    public override void close(bool iAnimate)
    {
        base.close(iAnimate);
        
        Utils.GetInputManager().UnsetUnique(this as IControllable);
        deselect(i_test);

        rootUI.handGivenBack();
    }
}
