using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class UIGaragePanel : UIGarageSelector, IControllable
{    
    protected float elapsed_time;
    public UIGarageDisplayPanel displayPanel;
    public UIGarage parentUI;

    private List<Animator> animators;


    // Start is called before the first frame update
    void Start()
    {
        elapsed_time = 0f;
        initSelector();

        animators = new List<Animator>(GetComponentsInChildren<Animator>());

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
        animateIn();
    }

    public virtual void close()
    {
        animateOut();
    }

    public void animateIn()
    {
        // Both panel come from under
        animators = new List<Animator>(GetComponentsInChildren<Animator>());
        if (animators!=null)
        {
            foreach(Animator a in animators)
            { 
                a.enabled = true;
                a.updateMode = AnimatorUpdateMode.UnscaledTime; // as we pause game by putting deltaTime to 0
                a.SetTrigger("animatePanel");
            }
        }
    }

    public void animateOut()
    {
        // Exit Left for LPanel, right for Rpanel
                // Both panel come from under
        animators = new List<Animator>(GetComponentsInChildren<Animator>());
        if (animators!=null)
        {
            foreach(Animator a in animators)
            { 
                a.enabled = true;
                a.updateMode = AnimatorUpdateMode.UnscaledTime; // as we pause game by putting deltaTime to 0
                a.SetTrigger("animatePanel");
            }
        }
    }
}
