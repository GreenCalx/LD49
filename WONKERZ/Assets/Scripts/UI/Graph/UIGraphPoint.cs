using UnityEngine;
using UnityEngine.UI;

public class UIGraphPoint : UIImageTab
{    // avoid too many inputs : we use axis they can be true for a long time
    protected float elapsed_time = 0f;
    public float selector_latch = 0f;

    public int index;

    override protected void ProcessInputs(InputManager.InputData Entry)
    {
        if (isActivated)
        {
            if (Entry.Inputs["Cancel"].IsDown)
                onDeactivate?.Invoke();

            if (elapsed_time > selector_latch)
            {
                float X = 0;
                float Y = 0;
                X = Entry.Inputs[Constants.INPUT_TURN].AxisValue;
                Y = Entry.Inputs[Constants.INPUT_UIUPDOWN].AxisValue;
                (Parent as UIGraphPanel).moveKey(index, X, Y);
                elapsed_time = 0;
            }
        } else {
            base.ProcessInputs(Entry);
        }
        elapsed_time += Time.unscaledDeltaTime;
    }

    override public void select()
    {
        base.select();
    }

    override public void deselect()
    {
        base.deselect();
    }

    override public void activate()
    {
        base.activate();
        Utils.attachUniqueControllable(this);
        isActivated = true;
    }

    override public void deactivate()
    {
        base.deactivate();
        Utils.detachUniqueControllable();
        isActivated = false;
        activator?.onSelect?.Invoke();
    }

}
