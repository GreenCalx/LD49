using UnityEngine;

public class UIWaitInputsPanel : UIPanelControlable
{
    private bool wait = false;
    private float waitBeforeInputs = 0.5f;
    private float currentTime = 0f;
    override protected void ProcessInputs(InputManager.InputData Entry)
    {
        if (currentTime > waitBeforeInputs)
        {
            if (wait)
            {
                if (Entry.GetAnyKeyDown(out int code, out Device device))
                {
                    wait = false;
                    (Parent as UIBindingElement).SetBinding(code, device);
                }
                //var e = Event.current;
                //if (e.isKey && e.type == EventType.KeyDown) {
                //    if (e.keyCode != KeyCode.None)
                //    {
                //        wait = false;
                //        (Parent as UIBindingElement).SetBinding(e.keyCode);
                //    }
                //}
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
