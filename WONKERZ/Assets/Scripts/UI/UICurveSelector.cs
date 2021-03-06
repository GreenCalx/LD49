using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UICurveSelector : UIGarageSelectable, IControllable, IUIGarageElement
{
    [Header("Tweaks")]
    public float moveStep = 0.005f; // percentage between 0f+ to 1f
    
    [Header("Internals")]
    public float XLeftBound;
    public float XRightBound;
    public float XKeyLeftBound;
    public float XKeyRightBound;
    public UIGarageCarStatsPanel observer;
    public int movable_key;

    
    private ResolutionManager resolutionManager;

    // Start is called before the first frame update
    void Start()
    {
        resolutionManager = Utils.getResolutionManager();
        Utils.attachControllable<UICurveSelector>(this);
    }

    void OnDestroy() {
        Utils.detachControllable<UICurveSelector>(this);
    }

    private float getMoveStep()
    {
        if (!resolutionManager)
            return moveStep;
        Debug.Log(resolutionManager.getHorMoveStep(moveStep));
        return resolutionManager.getStdMoveStep().x;
    }

    void IControllable.ProcessInputs(InputManager.InputData Entry) {
        float X = Entry.Inputs["Turn"].AxisValue;
        if( X > 0.2f)
        {
            if ((transform.position.x < XRightBound) && (transform.position.x < XKeyRightBound) )
            {
                transform.position += new Vector3( getMoveStep(), 0f, 0f);
                observer.notifySliderMove(movable_key);
            }
        } else if ( X < -0.2f)
        {
            if ((transform.position.x > XLeftBound) && (transform.position.x > XKeyLeftBound))
            {
                observer.notifySliderMove(movable_key);
                transform.position -= new Vector3( getMoveStep(), 0f, 0f);
            }
        }

        if (Entry.Inputs["Cancel"].IsDown) {
            Utils.GetInputManager().UnsetUnique(this as IControllable);
            quit();
            // TODO : set curve back to previous positon
        }
        else if (Entry.Inputs[Constants.INPUT_JUMP].IsDown) {
            Utils.GetInputManager().UnsetUnique(this as IControllable);
            observer.updatePlayerCurve();
            quit();
        }
    }

    Dictionary<string,string> IUIGarageElement.getHelperInputs()
    {
        Dictionary<string,string> retval = new Dictionary<string, string>();

        retval.Add(Constants.RES_ICON_A, "CONFIRM");
        retval.Add(Constants.RES_ICON_B, "CANCEL");

        return retval;
    }

    // Update is called once per frame
    void Update()
    {
    }

    public override void enter(UIGarageSelector uigs)
    {
        base.enter(uigs);
    }
    public override void quit()
    {
        base.quit();
    }

}
