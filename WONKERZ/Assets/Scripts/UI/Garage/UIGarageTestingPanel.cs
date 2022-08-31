using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIGarageTestingPanel : UIGarageCancelablePanel
{
    [Header("MANDATORY")]
    public TextMeshProUGUI txt_load_status;
    
    private int i_test;

    private Color enabled_test;
    private Color disabled_test;
    private Color selected_test;

    private bool test_is_running = false;
    // Start is called before the first frame update
    void Start()
    {
        init();
        txt_load_status.gameObject.SetActive(false);
    }

    private void init()
    {
        elapsed_time = 0f;
        
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

    public void activate(){
        Utils.attachUniqueControllable(this);
        isActivated = true;
        i_test = 0;
        elapsed_time = 0f;
        Tabs[0].onSelect?.Invoke();
    }
    public void open()
    {   
        gameObject.SetActive(true);
        animateIn();
        foreach(UITab t in Tabs) {
            t.onDeselect?.Invoke();
        }

    }

    public void close(){
        gameObject.SetActive(false);
        animateOut();
    }

    public void deactivate()
    {    
        if (test_is_running) {
            test_is_running=false;
            return;
        }
        Utils.detachUniqueControllable();
        isActivated = false;
        txt_load_status.gameObject.SetActive(false);
        activator.onSelect?.Invoke();
    }
}
