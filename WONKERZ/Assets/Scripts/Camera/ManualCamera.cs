using UnityEngine;
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
    [SerializeField, Range(0f, 90f)] float alignSmoothRange = 45f;
    [SerializeField] LayerMask obstructionMask = -1;
    [Header("Jump")]
    [SerializeField, Min(0f)] float jumpDelay = 5f;
    [SerializeField, Min(0f)] float jumpMaxFocusRadius = 15f;
    [SerializeField, Min(0f)] float jumpFocusRadiusStep = 1f;
    /// Internals
    private Vector3 focusPoint, previousFocusPoint;
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
        Utils.attachControllable<ManualCamera>(this);
        initial_FOV = cam.fieldOfView;
        jumpStartTime = 0f;
        baseFocusRadius = focusRadius;

    }
    private void Start()
    {
        init();
    }

    void OnDestroy()
    {
        Utils.detachControllable<ManualCamera>(this);
    }

    void IControllable.ProcessInputs(InputManager.InputData Entry)
    {
        input = Vector3.zero;
        float multiplier = (InputSettings.InverseRSMapping) ? -1f : 1f;
        if (!needButtonPressBeforeMove || Input.GetMouseButton(0))
        {
            if (!(Entry.Inputs[(int)GameInputsButtons.WeightControl].Down
                  && (GameInputsUtils.IsSame(GameInputsAxis.CameraX, GameInputsAxis.WeightX)
                      || GameInputsUtils.IsSame(GameInputsAxis.CameraY, GameInputsAxis.WeightY))))
            {
                // input is cameraY, cameraX, because it represents the axis of rotation.
                // therefor, trying to move the camera left (cameraX) means rotating around Y orbitaly.
                input = new Vector2(Entry.Inputs[(int)GameInputsAxis.CameraY].AxisValue, -Entry.Inputs[(int)GameInputsAxis.CameraX].AxisValue) * multiplier;
            }
        }
    }

    public override void init()
    {
        playerRef = Utils.getPlayerRef();
        focus = playerRef.transform;
        focusPoint = focus.position;

        orbitAngles = new Vector2(defaultVerticalAngle, 180f);
        transform.localRotation = Quaternion.Euler(orbitAngles);

    }

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


    private Vector2 input;
    bool ManualRotation()
    {
        const float e = 0.001f;
        if (input.x < -e || input.x > e || input.y < -e || input.y > e)
        {
            orbitAngles += rotationSpeed * Time.unscaledDeltaTime * input;
            lastManualRotationTime = Time.unscaledTime;
            return true;
        }
        return false;
    }

    bool autoRotation()
    {
        if (Time.unscaledTime - lastManualRotationTime < alignDelay)
        return false;

        Vector2 movement = new Vector2(focusPoint.x - previousFocusPoint.x, focusPoint.z - previousFocusPoint.z);
        if  (movement.y > 0) // backward movement -- we dont want to rotate the camera around the player
        {
            return false;
        }

        float movementDeltaSqr = movement.sqrMagnitude;
        if (movementDeltaSqr < 0.000001f)
        {
            return false;
        }
        float headingAngle = GetAngle(movement / Mathf.Sqrt(movementDeltaSqr));
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
}
