using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Schnibble;

public class TutorialBalloon : MonoBehaviour
{
    [Header("Init")]
    public bool enable_move = true;
    
    [Header("MAND")]
    public UIGenerativeTextBox display;
    private PlayerController player;
    private float elapsedTimeSinceStart = 0f;

    // ORBIT
    [Header("ORBIT")]
    [SerializeField] public Transform focus = default;
    [SerializeField, Range(-89f, 89f)] public float minVerticalAngle = -30f, maxVerticalAngle = 60f, defaultVerticalAngle = 30f;
    [SerializeField, Range(1f, 80f)] public float distance = 5f;
    [SerializeField, Min(0f)] public float focusRadius = 1f;
    [SerializeField, Range(0f, 1f)] public float focusCentering = 0.5f;
    [SerializeField, Range(1f, 360f)] public float rotationSpeed = 90f;
    [SerializeField, Range(0f, 90f)] float alignSmoothRange = 45f;
    private Vector3 focusPoint, previousFocusPoint;
    private Vector2 orbitAngles = new Vector2(45f, 0f);

    public void updateDisplay(List<UIGenerativeTextBox.UIGTBElement> iElems)
    {
        display.elements = iElems;
        display.resetAndGenerate();
    }

    // Start is called before the first frame update
    void Start()
    {
        player = Access.Player();
        
        focus = player.gameObject.transform;
        focusPoint = focus.position;

        orbitAngles = new Vector2(defaultVerticalAngle, -90f);
        transform.localRotation = Quaternion.Euler(orbitAngles);

        elapsedTimeSinceStart = 0f;

        facePlayer();
    }

    void LateUpdate()
    {
        if (!enable_move)
        {
            enable_move = Vector3.Distance(transform.position, focus.position) <= distance;
            if (!enable_move)
                return;
        }


        UpdateFocusPoint();
        Quaternion lookRotation;
        if (autoRotation())
        {
            constrainAngles();
            lookRotation = Quaternion.Euler(orbitAngles);
        } else {
            lookRotation = transform.localRotation;
        }
        Vector3 lookDirection = lookRotation * Vector3.forward;
        Vector3 lookPosition = focusPoint - lookDirection * distance;
        
        transform.SetPositionAndRotation(lookPosition, lookRotation);
    }

    void UpdateFocusPoint()
    {
        previousFocusPoint = focusPoint;
        Vector3 targetPoint = focus.position;

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

    bool autoRotation()
    {
        Vector2 movement = new Vector2(focusPoint.x - previousFocusPoint.x, focusPoint.z - previousFocusPoint.z);

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
        else if (0f - deltaAbs < alignSmoothRange)
        {
            rotationChange *= (0f - deltaAbs) / alignSmoothRange;
        }

        orbitAngles.y = Mathf.MoveTowardsAngle(orbitAngles.y, headingAngle, rotationChange);
        return true;
    }

    public void wiggle()
    {
        Vector3 newPosition = transform.position;

        elapsedTimeSinceStart += Time.deltaTime;
        newPosition.y += Mathf.Sin(elapsedTimeSinceStart) * 0.005f;

        transform.position = newPosition;
    }

    private void facePlayer()
    {
        Vector3 difference = player.transform.position - transform.position;
        float rotationY = Mathf.Atan2(difference.x, difference.z) * Mathf.Rad2Deg;

        transform.rotation = Quaternion.Euler(0.0f, rotationY, 0.0f);
    }
    static float GetAngle(Vector2 direction)
    {
        float angle = Mathf.Acos(direction.y) * Mathf.Rad2Deg;
        // (x < 0) => counterclockwise
        return direction.x < 0f ? 360f - angle : angle;
    }
}
