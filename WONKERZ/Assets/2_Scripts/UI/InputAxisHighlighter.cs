using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Schnibble;
using System;
public class InputAxisHighlighter : MonoBehaviour, IControllable
{
    [Header("MAND")]
    public Image self_imgRef;
    [Header("Tweaks")]
    public PlayerInputs.InputCode axis0;
    public PlayerInputs.InputCode axis1;
    public Color baseColor;
    public Color highlightColor;
    public float deadzone = 0.1f;

    [Header("Internals")]
    public bool highlight = false;
    public InputManager IM_Player1 = null;

    void Start()
    {
        Access.Player().inputMgr.Attach(this as IControllable);
    }

    void OnDestroy()
    {
        Access.Player().inputMgr.Detach(this as IControllable);
    }

    void LateUpdate()
    {
        if (highlight)
            self_imgRef.color = highlightColor;
        else
            self_imgRef.color = baseColor;
    }


    void IControllable.ProcessInputs(InputManager currentMgr, GameInput[] Entry)
    {
        // if (Access.PlayerInputsManager().player1 == currentMgr)
        // {
        //     Debug.Log("IM_Player1");
        // }      

        GameInputAxis   inputAxis0   = Entry[(int)axis0] as GameInputAxis;
        GameInputAxis   inputAxis1   = Entry[(int)axis1] as GameInputAxis;

        if (null!=inputAxis0)
        {
            var axis_value = inputAxis0.GetState().valueSmooth;
            if ((axis_value>deadzone)||(axis_value<(-1*deadzone)))
            {
                highlight = true;
                return;
            }
        } 
        if ((null!=inputAxis1) && !highlight)
        {
            var axis_value = inputAxis1.GetState().valueSmooth;
            if ((axis_value>deadzone)||(axis_value<(-1*deadzone)))
            {
                highlight = true;
                return;
            }
        } 
        highlight = false;
    }
}
