using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Schnibble;
using System;
using static Schnibble.Physics; // axles
using Schnibble.UI;
using Schnibble.Managers;

public class TutorialBalloonTrigger : MonoBehaviour, IControllable
{
    [Header("UI")]
    public List<UIGenerativeTextBox.UIGTBElement> elements;
    public List<UIGenerativeTextBox.UIGTBElement> validationElements;

    [Header("Behaviors")]
    private bool triggered = false;
    public bool disableBalloon = false;
    public bool triggerOnce = false;
    private bool tutorialCompleted = false;
    /// ---
    public UnityEvent ValidationCond;

    public TutorialBalloon.BALLOON_XANGLE BalloonXAngle = TutorialBalloon.BALLOON_XANGLE.MIDDLE;
    public TutorialBalloon.BALLOON_YANGLE BalloonYAngle = TutorialBalloon.BALLOON_YANGLE.MIDDLE;

    [Header("Internals")]
    public TutorialBalloon tutorialBalloon;
    public bool jump_pressed;
    public bool weight_pressed;
    public bool LJoyDown_pressed;
    public float weight_pressed_elapsed = 0f;
    public float jump_pressed_elapsed = 0f;
    public float LJoyDown_pressed_elapsed = 0f;

    void Start()
    {
        jump_pressed    = false;
        weight_pressed  = false;
        LJoyDown_pressed = false;
        weight_pressed_elapsed  = 0f;
        jump_pressed_elapsed    = 0f;
        LJoyDown_pressed_elapsed= 0f;
    }

    void OnDestroy()
    {
        try{
            Access.PlayerInputsManager().player1.Detach(this as IControllable);
        } catch (NullReferenceException e) {
            this.Log(gameObject.name + " OnDestroy : NULL ref on detachable");
        }
    }

    void IControllable.ProcessInputs(InputManager currentMgr, GameController Entry)
    {
        if (!triggered)
        return;

        // weight_pressed = ((Entry[(int)PlayerInputs.InputCode.CameraControl] as GameInputButton).GetState().up);
        // if (weight_pressed)
        // { weight_pressed_elapsed += Time.deltaTime; }
        // if ((Entry[(int)PlayerInputs.InputCode.CameraControl] as GameInputButton).GetState().heldDown)
        // { weight_pressed_elapsed = 0f; }

        jump_pressed = ((Entry.Get((int)PlayerInputs.InputCode.Jump) as GameInputButton).GetState().down);
        if (jump_pressed)
        { jump_pressed_elapsed += Time.deltaTime; }
        if ((Entry.Get((int)PlayerInputs.InputCode.Jump) as GameInputButton).GetState().up)
        { jump_pressed_elapsed = 0f; }

        LJoyDown_pressed = ((Entry.Get((int)PlayerInputs.InputCode.WeightY) as GameInputAxis).GetState().valueSmooth < 0f);
        if (LJoyDown_pressed)
        { LJoyDown_pressed_elapsed += Time.deltaTime; }
    }

    void Update()
    {
        if (triggered && (null!=ValidationCond) && !tutorialCompleted)
        {
            ValidationCond.Invoke();
        }
    }

    /// # PANEL PLANT
    public void PanelPlantedCond()
    {
        CheckPointManager cpm = Access.CheckPointManager();
        if (!!cpm)
        {
            if (cpm.hasSS)
            {
                // Ensure SS is within trigger's box
                Vector3 ss_pos = cpm.saveStateMarkerInst.transform.position;
                
                BoxCollider trigg_box = GetComponent<BoxCollider>();
                Vector3 box_pos = trigg_box.transform.position;
                Vector3 box_size = trigg_box.size;

                if ((ss_pos.x < box_pos.x + box_size.x) && (ss_pos.x > box_pos.x - box_size.x))
                {
                    if ((ss_pos.z < box_pos.z + box_size.z) && (ss_pos.z > box_pos.z - box_size.z))
                    {
                        HappyValidation();
                    }
                }
            }
        }
    }

    /// # MOVING WHEIGHT
    public void IsMovingWeightCond()
    {
        PlayerInputsManager pim = Access.PlayerInputsManager();
        if (weight_pressed_elapsed >= 0.2f)
        {
            HappyValidation();
        }
    }

    public void IsMovingWeightBackCond()
    {
        PlayerInputsManager pim = Access.PlayerInputsManager();
        if ((weight_pressed_elapsed >= 0.5f)&&(LJoyDown_pressed_elapsed>=0.5f))
        {
            HappyValidation();
        }
    }

    // # SPRINGS
    public void IsInRaceCarCond()
    {
        #if false
        PlayerController pc = Access.Player();
        float spring_ratio = pc.springElapsedCompression / pc.springCompressionTime;
        if (spring_ratio > 0.9f)
        {
            HappyValidation();
        }
        #endif
    }

    /// # HAS JUMPED
    public void HasJumpedCond()
    {
        PlayerController pc = Access.Player();
        if ((jump_pressed_elapsed>0.5f) && !pc.TouchGround())
        {
            HappyValidation();
        }
    }

    /// # FOCUS
    public void FocusValidationCond()
    {
        CameraManager cm = Access.CameraManager();
        if (cm.active_camera==null)
        return;

        PlayerCamera pCam = cm.active_camera.GetComponent<PlayerCamera>();
        if (!!pCam)
        {
            if (pCam.secondaryFocus != null)
            {
                HappyValidation();
            }
        }
    }
    /// # DRIFT   
    public void DriftValidationCond()
    {
        PlayerController pc = Access.Player();

        #if false
        var rear = pc.car.axles[(int)AxleType.rear];
        var front = pc.car.axles[(int)AxleType.front];

        if  (rear.left.isHandbraked  &&
             rear.right.isHandbraked &&
             front.left.isHandbraked &&
             front.right.isHandbraked)
        {
            HappyValidation();
        }
        #endif
    }

    public void HappyValidation()
    {
        tutorialBalloon.updateDisplay(validationElements);
        Access.PlayerInputsManager().player1.Detach(this as IControllable);
        tutorialCompleted = true;
    }
    // -------------

    void OnTriggerEnter(Collider iCollider)
    {
        if (tutorialCompleted)
        return;

        if (triggerOnce)
        {
            if (triggered)
            return;
        }

        if (disableBalloon)
        {
            tutorialBalloon.disable();
        }

        if (Utils.colliderIsPlayer(iCollider))
        {
            tutorialBalloon.updateDisplay(elements);
            
            tutorialBalloon.currTrigger = this;
            tutorialBalloon.updateAnchor(BalloonXAngle, BalloonYAngle);

            triggered = true;
        }
    }
}
