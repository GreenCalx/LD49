using System.Collections;

using UnityEngine;

using Schnibble;
using Schnibble.UI;
using Schnibble.Managers;

public class UIWaitInputsPanel : UIPanel
{
    override protected void ProcessInputs(InputManager currentMgr, GameController Entry)
    {
        if (isActivated)
        {
            if (currentMgr.GetAnyKeyDown(out Controller.InputCode code))
            {
                (parent as UIBindingElement).SetBinding(code);
                Deactivate();
            }
        }
    }

    override public void Show() {
        base.Show();
        Activate();
    }

    override public void Hide() {
        base.Hide();
        Deactivate();
    }

    override public void Activate() {
        base.Activate();
        // :InputNextFrame:
        // start inputs only on next frame =>
        // this is important because we dont want the input that activated us to
        // fire on the same frame our own activation.
        // NOTE: object needs to be active for coroutines to work.
        StartCoroutine(StartInputsNextFrame());
    }

    override public void Deactivate() {
        base.Deactivate();
        // same logic as :InitNextFrame: but in reverse : we dont want to fire
        // an events coming from us waiting the input.
        // except here we could have the player maintaining an axis for some reason
        // so we will wait for more than one frame and swallow all incoming inputs
        // NOTE: object should be active until the end of the coroutine!
        StartCoroutine(StopInputsNextFrame());
    }

    IEnumerator StartInputsNextFrame() {
        yield return new WaitForEndOfFrame();
        StartInputs();
        yield break;
    }

    IEnumerator StopInputsNextFrame() {
        yield return new WaitForSeconds(0.1f);
        StopInputs();
        Hide();
        yield break;
    }
}
