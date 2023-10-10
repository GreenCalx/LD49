using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Schnibble;
using static Schnibble.SchPhysics; // axles

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
        Utils.detachControllable<TutorialBalloonTrigger>(this);
    }

    void IControllable.ProcessInputs(InputManager currentMgr, GameInput[] Entry)
    {
        if (!triggered)
            return;

        weight_pressed = ((Entry[(int)PlayerInputs.InputCode.WeightControl] as GameInputButton).GetState().heldDown);
        if (weight_pressed)
        { weight_pressed_elapsed += Time.deltaTime; }
        if ((Entry[(int)PlayerInputs.InputCode.WeightControl] as GameInputButton).GetState().up)
        { weight_pressed_elapsed = 0f; }

        jump_pressed = ((Entry[(int)PlayerInputs.InputCode.Jump] as GameInputButton).GetState().down);
        if (jump_pressed)
        { jump_pressed_elapsed += Time.deltaTime; }
        if ((Entry[(int)PlayerInputs.InputCode.Jump] as GameInputButton).GetState().up)
        { jump_pressed_elapsed = 0f; }

        LJoyDown_pressed = ((Entry[(int)PlayerInputs.InputCode.WeightY] as GameInputAxis).GetState().valueSmooth < 0f);
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
                HappyValidation();
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
        PlayerController pc = Access.Player();
        float spring_ratio = pc.springElapsedCompression / pc.springCompressionTime;
        if (spring_ratio > 0.9f)
        {
            HappyValidation();
        }
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

        var rear = pc.car.axles[(int)AxleType.rear];
        var front = pc.car.axles[(int)AxleType.front];

        if  (rear.left.isHandbraked  &&
                rear.right.isHandbraked &&
                front.left.isHandbraked &&
                front.right.isHandbraked)
        {
            HappyValidation();
        }
    }

    public void HappyValidation()
    {
        tutorialBalloon.updateDisplay(validationElements);
        Utils.detachControllable<TutorialBalloonTrigger>(this);
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
            tutorialBalloon.enable_move = false;
            tutorialBalloon.disable_balloon_follow = true;
        }

        if (Utils.colliderIsPlayer(iCollider))
        {
            tutorialBalloon.updateDisplay(elements);
            
            tutorialBalloon.currTrigger = this;
            triggered = true;
        }
    }
}
