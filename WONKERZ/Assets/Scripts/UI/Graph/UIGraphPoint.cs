using UnityEngine;
using UnityEngine.UI;

public class UIGraphPoint : UIGarageCancelableImageTab
{

    public int index;

    override protected void ProcessInputs(InputManager.InputData Entry)
    {
        base.ProcessInputs(Entry);

        if (isActivated)
        {
            if (elapsed_time > selector_latch)
            {
                float X = 0;
                float Y = 0;
                X = Entry.Inputs[Constants.INPUT_TURN].AxisValue;
                Y = Entry.Inputs[Constants.INPUT_UIUPDOWN].AxisValue;
                (Parent as UIGraphPanel).moveKey(index, X, Y);
                elapsed_time = 0;
            }
        }
        elapsed_time += Time.unscaledDeltaTime;
    }
}
