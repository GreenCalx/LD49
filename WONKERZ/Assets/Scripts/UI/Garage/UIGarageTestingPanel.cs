using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIGarageTestingPanel : UIGaragePanel
{
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
            if (test_is_running)
            {
                //selectables[i_test].quit();
                test_is_running = false;
            }
        }
    }

    public void pick()
    {
        test_is_running = true;
    }

    override public void deactivate()
    {    
        if (test_is_running) {
            test_is_running=false;
            return;
        }

        base.deactivate();
        txt_load_status.gameObject.SetActive(false);
    }
}
