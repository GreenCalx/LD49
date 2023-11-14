using UnityEngine;
using System;
using Schnibble;

public class ManualCamera : PlayerCamera, IControllable
{
    [Header("ManualCamera")]
    /// TWEAKS
    [SerializeField] public bool needButtonPressBeforeMove = true;
    [SerializeField] public bool autoAlign = false;
    [SerializeField] public Transform focus = default;
    [SerializeField, Range(1f, 80f)] public float distance = 5f;
    [SerializeField, Min(0f)] public float focusRadius = 1f;
    [SerializeField, Range(0f, 1f)] public float focusCentering = 0.5f;
    [SerializeField, Range(1f, 360f)] public float rotationSpeed = 90f;

    [SerializeField, Range(-89f, 89f)] public float minVerticalAngle = -30f, maxVerticalAngle = 60f, defaultVerticalAngle = 30f;
    [SerializeField, Min(0f)] public float alignDelay = 5f;
    [SerializeField, Min(0f)] public float alignDelayWithSecondaryFocus = 0f;
    [SerializeField, Range(0f, 90f)] float alignSmoothRange = 45f;
    [SerializeField] LayerMask obstructionMask = -1;
    [SerializeField, Range(0f, 180f)] public float camReverseDetectionThreshold = 140f;
    [Header("Jump")]
    [SerializeField, Min(0f)] float jumpDelay = 5f;
    [SerializeField, Min(0f)] float jumpMaxFocusRadius = 15f;
    [SerializeField, Min(0f)] float jumpFocusRadiusStep = 1f;
    /// Internals
    private Vector3 focusPoint, previousFocusPoint;
    private float previousHeadingAngle;
    private Vector2 orbitAngles = new Vector2(45f, 0f);
    [HideInInspector]
    public float lastManualRotationTime;
    private float jumpStartTime;
    private float baseFocusRadius;

    private Vector3 CameraHalfExtends
    {
        get
        {
            Vector3 halfExtends;
            halfExtends.y =
                cam.nearClipPlane * Mathf.Tan(0.5f * Mathf.Deg2Rad * cam.fieldOfView);
            halfExtends.x = halfExtends.y * cam.aspect;
            halfExtends.z = 0f;
            return halfExtends;
        }
    }

    /// Methods
    void Awake()
    {
        cam = GetComponent<Camera>();
        //Utils.attachControllable<ManualCamera>(this);
        
        initial_FOV = cam.fieldOfView;
        jumpStartTime = 0f;
        baseFocusRadius = focusRadius;

    }
    private void Start()
    {
        init();
    }

    void Update()
    {
        updateSecondaryFocus();
        Debug.DrawRay(focus.position, focus.forward*10, Color.blue);
    }

    void OnDestroy()
    {
        try{
            Access.PlayerInputsManager().player1.Detach(this as IControllable);
        } catch (NullReferenceException e) {
            this.Log(gameObject.name + " OnDestroy : NULL ref on detachable");
        }
    }

    void IControllable.ProcessInputs(InputManager currentMgr, GameInput[] Entry)
    {
        // Camera manual movements if no focus
        if (null==secondaryFocus)
        {
            Vector2 multiplier = new Vector2(InputSettings.InverseCameraMappingX ? -1f : 1f,
                InputSettings.InverseCameraMappingY ? -1f : 1f);
            if (!needButtonPressBeforeMove || Input.GetMouseButton(0))
            {
                    // input is cameraY, cameraX, because it represents the axis of rotation.
                // therefor, trying to move the camera left (cameraX) means rotating around Y orbitaly.
                var current = new Vector2(
                    (Entry[(int)PlayerInputs.InputCode.CameraY] as GameInputAxis).GetState().valueRaw,
                    -(Entry[(int)PlayerInputs.InputCode.CameraX] as GameInputAxis).GetState().valueRaw);
                current = Vector2.Scale(current, multiplier);
                input.Add(current);
            }
        }

        // View reset
        if ((Entry[(int)PlayerInputs.InputCode.CameraReset] as GameInputButton).GetState().down)
        {
            resetView();
        }

        // Camera targeting
        // set secondary focus
        if ((Entry[(int)PlayerInputs.InputCode.CameraFocus] as GameInputButton).GetState().heldDown)
        {
            if ((null==secondaryFocus)&& !focusInputLock) {
                findFocus();
                elapsedPressTimeToCancelSecondaryFocus = 0f;
                focusInputLock = true;
            } else {
                elapsedPressTimeToCancelSecondaryFocus += Time.unscaledDeltaTime;
                if (elapsedPressTimeToCancelSecondaryFocus > pressTimeSecondaryFocus)
                {
                    resetFocus();
                    focusInputLock = true;
                }

                if (!!uISecondaryFocus)
                { 
                    if (!focusInputLock)
                    {
                        float ratio = elapsedPressTimeToCancelSecondaryFocus/pressTimeSecondaryFocus;
                        if (ratio>0.2f)
                            uISecondaryFocus.updateFillAmount(ratio);
                    }
                }
            }
        }

        // poll for focus change
        elapsedTimeFocusChange += Time.deltaTime;
        if ((null!=secondaryFocus)&&(elapsedTimeFocusChange >= focusChangeInputLatch))
        {
            var focus_change_val = ((Entry[(int)PlayerInputs.InputCode.CameraFocusChange] as GameInputAxis)).GetState().valueRaw;
            if (focus_change_val > 0)
            {
                changeFocus();
                elapsedTimeFocusChange = 0f;
            }
            else if (focus_change_val < 0)
            {
                changeFocus();
                elapsedTimeFocusChange = 0f;
            }
        }

        // poll for stop focus
        if ((Entry[(int)PlayerInputs.InputCode.CameraFocus] as GameInputButton).GetState().up)
        {
            if (!focusInputLock)
            {
                if (elapsedPressTimeToCancelSecondaryFocus > pressTimeSecondaryFocus) 
                {
                    resetFocus();
                }
            }

            focusInputLock = false;
            elapsedPressTimeToCancelSecondaryFocus = 0f;
            if (!!uISecondaryFocus)
            { 
                uISecondaryFocus.updateFillAmount(0f);            
            }
        }

    }

    // Game camera overrides
    public override void init()
    {
        Access.Player().inputMgr.Attach(this as IControllable);
        playerRef = Access.Player().gameObject;
        focus = playerRef.transform;
        focusPoint = focus.position;

        resetView();

    }

    public override void resetView() 
    { 
        Vector2 fwd_angle = (focus.position + focus.forward).normalized * -1;
        float headingAngle = GetAngle(fwd_angle);
        previousHeadingAngle = headingAngle;
        //orbitAngles.y = headingAngle;
        orbitAngles.y = focus.eulerAngles.y;
        orbitAngles.x = defaultVerticalAngle;
        //constrainAngles();

        Quaternion lookRotation = Quaternion.Euler(orbitAngles);
        Vector3 lookDirection = lookRotation * focus.forward;
        Vector3 lookPosition = focusPoint - lookDirection * distance;

        Vector3 rectOffset = lookDirection * cam.nearClipPlane;
        Vector3 rectPosition = lookPosition + rectOffset;
        Vector3 castFrom = focus.position;
        Vector3 castLine = rectPosition - castFrom;
        float castDistance = castLine.magnitude;

        Vector3 castDirection = castLine / castDistance;

        // check if we hit something between camera and focuspoint
        if (Physics.BoxCast(castFrom, CameraHalfExtends, castDirection, out RaycastHit hit, lookRotation, castDistance, obstructionMask))
        {
            rectPosition = castFrom + castDirection * hit.distance;
            lookPosition = rectPosition - rectOffset;
        }
        transform.SetPositionAndRotation(lookPosition, lookRotation);
    }

    // behavior

    void LateUpdate()
    {
        var player = Access.Player();
        if (!!player)
        {
            if (player.flags[PlayerController.FJump])
            {
                if (jumpStartTime <= 0f)
                jumpStartTime = Time.unscaledTime;
                UpdateFocusPointInJump();
            }
            else
            {
                jumpStartTime = 0f;
                UpdateFocusPoint();
            }
        }

        //UpdateFocusPoint();
        Quaternion lookRotation;

        if (ManualRotation() || (autoAlign && autoRotation()))
        {
            constrainAngles();
            lookRotation = Quaternion.Euler(orbitAngles);
        }
        else
        {
            lookRotation = transform.localRotation;
        }

        Vector3 lookDirection = lookRotation * Vector3.forward;
        Vector3 lookPosition = focusPoint - lookDirection * distance;

        Vector3 rectOffset = lookDirection * cam.nearClipPlane;
        Vector3 rectPosition = lookPosition + rectOffset;
        Vector3 castFrom = focus.position;
        Vector3 castLine = rectPosition - castFrom;
        float castDistance = castLine.magnitude;

        Vector3 castDirection = castLine / castDistance;

        // check if we hit something between camera and focuspoint
        if (Physics.BoxCast(castFrom, CameraHalfExtends, castDirection, out RaycastHit hit, lookRotation, castDistance, obstructionMask))
        {
            rectPosition = castFrom + castDirection * hit.distance;
            lookPosition = rectPosition - rectOffset;
        }

        // align with secondary focus
        if (!!secondaryFocus)
        {

        }

        transform.SetPositionAndRotation(lookPosition, lookRotation);
    }

    void UpdateFocusPoint()
    {
        previousFocusPoint = focusPoint;

        Vector3 targetPoint = focus.position;

        if (focusRadius > baseFocusRadius)
        focusRadius = ((focusRadius - jumpFocusRadiusStep) > baseFocusRadius) ? focusRadius - jumpFocusRadiusStep : baseFocusRadius;

        if (focusRadius > 0f)
        {
            float distance = Vector3.Distance(targetPoint, focusPoint);
            float t = 1f;
            if (distance > 0.01f && focusCentering > 0f)
            { t = Mathf.Pow(1f - focusCentering, Time.unscaledDeltaTime); }
            if (distance > focusRadius)
            {
                t = Mathf.Min(t, focusRadius / distance);
            }
            focusPoint = Vector3.Lerp(targetPoint, focusPoint, t);
        }
        else
        {
            focusPoint = targetPoint;
        }
    }

    void UpdateFocusPointInJump()
    {
        if (Time.unscaledTime - jumpStartTime < jumpDelay)
        return;

        previousFocusPoint = focusPoint;
        Vector3 targetPoint = focus.position;

        if (focusRadius < jumpMaxFocusRadius)
        focusRadius += jumpFocusRadiusStep;

        if (focusRadius > 0f)
        {
            float distance = Vector3.Distance(targetPoint, focusPoint);
            float t = 1f;
            if (distance > 0.01f && focusCentering > 0f)
            { t = Mathf.Pow(1f - focusCentering, Time.unscaledDeltaTime); }
            if (distance > focusRadius)
            {
                t = Mathf.Min(t, focusRadius / distance);
            }
            focusPoint = Vector3.Lerp(targetPoint, focusPoint, t);
        }
        else
        {
            focusPoint = targetPoint;
        }
    }

    void OnValidate()
    {
        if (maxVerticalAngle < minVerticalAngle)
        {
            maxVerticalAngle = minVerticalAngle;
        }
    }

    void constrainAngles()
    {
        orbitAngles.x = Mathf.Clamp(orbitAngles.x, minVerticalAngle, maxVerticalAngle);
        if (orbitAngles.y < 0f)
        {
            orbitAngles.y += 360f;
        }
        else if (orbitAngles.y >= 360f)
        {
            orbitAngles.y -= 360f;
        }
    }


    private SchMathf.AccumulatorV2 input;
    bool ManualRotation()
    {
        const float e = 0.001f;

        var inputAvg = input.average;
        if (inputAvg.x < -e || inputAvg.x > e || inputAvg.y < -e || inputAvg.y > e)
        {
            orbitAngles += rotationSpeed * Time.unscaledDeltaTime * inputAvg;
            lastManualRotationTime = Time.unscaledTime;

            input.Reset();
            return true;
        }
        return false;
    }

    bool autoRotation()
    {
        if (null==secondaryFocus)
            if (Time.unscaledTime - lastManualRotationTime < alignDelay)
                return false;
        else
            if (Time.unscaledTime - lastManualRotationTime < alignDelayWithSecondaryFocus)
                return false;

        Vector2 movement = Vector2.zero;
        if (null==secondaryFocus)
        {
            movement = new Vector2(focusPoint.x - previousFocusPoint.x, focusPoint.z - previousFocusPoint.z);
        }
        else {
            movement = new Vector2( secondaryFocus.transform.position.x - focusPoint.x , secondaryFocus.transform.position.z - focusPoint.z);
        }
        


        float movementDeltaSqr = movement.sqrMagnitude;
        if (movementDeltaSqr < 0.000001f)
        {
            return false;
        }

        float headingAngle = GetAngle(movement / Mathf.Sqrt(movementDeltaSqr));

        if (!ValidateNewHeadingAngle(headingAngle))
        { 
            return false; 
        }

        previousHeadingAngle = headingAngle;
        float deltaAbs = Mathf.Abs(Mathf.DeltaAngle(orbitAngles.y, headingAngle));
        float rotationChange = rotationSpeed * Mathf.Min(Time.unscaledDeltaTime, movementDeltaSqr);
        if (deltaAbs < alignSmoothRange)
        {
            rotationChange *= deltaAbs / alignSmoothRange;
        }
        else if (180f - deltaAbs < alignSmoothRange)
        {
            rotationChange *= (180f - deltaAbs) / alignSmoothRange;
        }

        orbitAngles.y = Mathf.MoveTowardsAngle(orbitAngles.y, headingAngle, rotationChange);
        return true;
    }

    static float GetAngle(Vector2 direction)
    {
        float angle = Mathf.Acos(direction.y) * Mathf.Rad2Deg;
        // (x < 0) => counterclockwise
        return direction.x < 0f ? 360f - angle : angle;
    }

    public void forceAlignToHorizontal(float iHorAngle)
    {
        orbitAngles = new Vector2(defaultVerticalAngle, iHorAngle);
        transform.localRotation = Quaternion.Euler(orbitAngles);

    }

    public void forceAngles(bool iForce, Vector2 forceAngle)
    {
        autoAlign = iForce;
        if (!iForce)
            return;
        orbitAngles = forceAngle;
        //transform.localRotation = Quaternion.Euler(orbitAngles);
    }

    public bool ValidateNewHeadingAngle(float iNewHeadingAngle)
    {
        // We don't want camera to go in front of player when he is going backward
        // Thus we don't accept any new angle with a delta around 180~
        // For now its not working well
        // TODO : Use Vector3.SignedAngle for a better result
        float deltaAngle = Mathf.DeltaAngle(iNewHeadingAngle, previousHeadingAngle);
        if (( deltaAngle >= camReverseDetectionThreshold) || (deltaAngle <= -camReverseDetectionThreshold))
        {
            return false;
        }

        return true;
    }
}
