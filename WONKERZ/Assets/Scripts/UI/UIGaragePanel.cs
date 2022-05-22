using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIGaragePanel : UIGarageSelector, IControllable
{    
    protected float elapsed_time;
    public UIGarageDisplayPanel displayPanel;

    // Start is called before the first frame update
    void Start()
    {
        elapsed_time = 0f;
        initSelector();

        Utils.attachControllable<UIGaragePanel>(this);
    }
    void OnDestroy()
    {
        Utils.detachControllable<UIGaragePanel>(this);
    }

    void IControllable.ProcessInputs(InputManager.InputData Entry) 
    {
        if ( elapsed_time > selector_latch )
        {
            float Y = Entry.Inputs[Constants.INPUT_UIUPDOWN].AxisValue;
            if ( Y < -0.2f )
            {
                selectPrevious();
                elapsed_time = 0f;
            }
            else if ( Y > 0.2f )
            {
                selectNext();
                elapsed_time = 0f;
            };
        }
        elapsed_time += Time.unscaledDeltaTime;
    }
    
    // Update is called once per frame
    void Update()
    {

    }

    public virtual void open()
    {
        initSelector();
    }

    public virtual void close()
    {

    }
}
