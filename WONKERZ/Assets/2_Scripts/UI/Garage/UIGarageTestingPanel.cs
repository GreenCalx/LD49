using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class UIGarageTestingPanel : UIGaragePanel
{
    #if false
    [Header("MANDATORY")]
    public TextMeshProUGUI txt_load_status;

    private bool test_is_running = false;
    // Start is called before the first frame update
    override protected void Awake()
    {
        base.Awake();

        txt_load_status.gameObject.SetActive(false);
        test_is_running = false;
    }

    override protected List<IUIGarageElement.UIGarageHelperValue> getHelperInputs()
    {
        return new List<IUIGarageElement.UIGarageHelperValue>{
            new IUIGarageElement.UIGarageHelperValue(Constants.RES_ICON_A, "REPLAY"),
            new IUIGarageElement.UIGarageHelperValue(Constants.RES_ICON_Y, "RECORD"),
            new IUIGarageElement.UIGarageHelperValue(Constants.RES_ICON_B, "CANCEL")
        };
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
            if (Access.TestManager().testIsRunning())
            {
                //selectables[i_test].quit();
                test_is_running = false;
                Access.TestManager().quitTest();
            }
        }
    }

    public void pick()
    {
        test_is_running = true;
    }

    override public void deactivate()
    {
        if (test_is_running)
        {
            test_is_running = false;
            return;
        }

        base.deactivate();
        txt_load_status.gameObject.SetActive(false);
    }
    #endif
}
