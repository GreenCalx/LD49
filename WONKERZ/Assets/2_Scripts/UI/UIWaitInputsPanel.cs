using UnityEngine;
using Schnibble;

public class UIWaitInputsPanel : UIPanelControlable
{
    private bool wait = false;
    private float waitBeforeInputs = 0.5f;
    private float currentTime = 0f;
    override protected void ProcessInputs(InputManager currentMgr, GameInput[] Entry)
    {
        if (currentTime > waitBeforeInputs)
        {
            if (wait)
            {
                if (currentMgr.GetAnyKeyDown(out Controller.InputCode code))
                {
                    wait = false;
                    (Parent as UIBindingElement).SetBinding(code);
                }
            }
        }
        currentTime += Time.unscaledDeltaTime;
    }

    // IMPORTANT toffa: needs to be done in update to be sure that we dont send
    // inputs back next frame.
    void Update()
    {
        if (!wait)
        onDeactivate?.Invoke();
    }

    public void WaitForInput()
    {
        wait = true;
        currentTime = 0f;
    }

    public void SetParent(UIElement e)
    {
        Parent = e;
    }
}
