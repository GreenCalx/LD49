using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Schnibble;
using System;

public class InputButtonHighlighter : MonoBehaviour, IControllable
{
    [Header("MAND")]
    public Image self_imgRef;
    [Header("Tweaks")]
    public PlayerInputs.InputCode key;
    public Color baseColor;
    public Color highlightColor;

    [Header("Internals")]
    public bool highlight = false;

    void Start()
    {
        Access.Player().inputMgr.Attach(this as IControllable);
    }

    void OnDestroy()
    {
        Access.Player().inputMgr.Detach(this as IControllable);
    }

    void Update()
    {
        if (highlight)
            self_imgRef.color = highlightColor;
        else
            self_imgRef.color = baseColor;
    }


    void IControllable.ProcessInputs(InputManager currentMgr, GameInput[] Entry)
    {
        GameInputButton inputButton = Entry[(int)key] as GameInputButton;
        if (null!=inputButton)
        {
            if (inputButton.GetState().down)
            {
                highlight = true;
            }
            else if (inputButton.GetState().heldDown)
            {
                highlight = true;
            }
            else if (inputButton.GetState().up)
            {
                highlight = false;
            }
        }
    }
}
