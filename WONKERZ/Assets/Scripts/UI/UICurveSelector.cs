using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UICurveSelector : MonoBehaviour, IControllable
{
    [Header("Tweaks")]
    public float moveStep = 1f;
    
    [Header("Internals")]
    public float XLeftBound;
    public float XRightBound;
    public float XKeyLeftBound;
    public float XKeyRightBound;
    public UIGarageCarStatsPanel observer;
    public int movable_key;


    // Start is called before the first frame update
    void Start()
    {
        Utils.attachControllable<UICurveSelector>(this);
    }

    void OnDestroy() {
        Utils.detachControllable<UICurveSelector>(this);
    }

    void IControllable.ProcessInputs(InputManager.InputData Entry) {
        float X = Entry.Inputs["Turn"].AxisValue;
        if( X > 0.2f)
        {
            if ((transform.position.x < XRightBound) && (transform.position.x < XKeyRightBound) )
            {
                transform.position += new Vector3( moveStep, 0f, 0f);
                observer.notifySliderMove(movable_key);
            }
        } else if ( X < -0.2f)
        {
            if ((transform.position.x > XLeftBound) && (transform.position.x > XKeyLeftBound))
            {
                observer.notifySliderMove(movable_key);
                transform.position -= new Vector3( moveStep, 0f, 0f);
            }
        }

        if (Entry.Inputs["Cancel"].IsDown) {
            Utils.GetInputManager().UnsetUnique(this as IControllable);
            // TODO : set curve back to previous positon
        }
        else if (Entry.Inputs[Constants.INPUT_JUMP].IsDown) {
            Utils.GetInputManager().UnsetUnique(this as IControllable);
        }
    }

    // Update is called once per frame
    void Update()
    {
    }
}
