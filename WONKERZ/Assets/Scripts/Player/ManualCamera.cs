using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class ManualCamera : MonoBehaviour
{
    [SerializeField] private Camera cam;
    
    private Vector3 previousPosition;

    private bool manual_cam = false;

    // AUTO CAM
    [SerializeField] public Transform focus = default;
    [SerializeField, Range(1f,50f)] public float distance = 5f;
    [SerializeField, Min(0f)] public float focusRadius = 1f;
    [SerializeField, Range(0f, 1f)] public float focusCentering = 0.5f;
    [SerializeField, Range(1f, 360f)]public float rotationSpeed = 90f;
    [SerializeField, Range(-89f, 89f)] public float minVerticalAngle = -30f, maxVerticalAngle = 60f;
    [SerializeField, Min(0f)] float alignDelay = 5f;
    [SerializeField, Range(0f, 90f)] float alignSmoothRange = 45f;
    private Vector3 focusPoint, previousFocusPoint;
    private Vector2 orbitAngles = new Vector2(45f, 0f);
    private float lastManualRotationTime;

    
    void Awake()
    {
        focusPoint = focus.position;
        transform.localRotation = Quaternion.Euler(orbitAngles);
    }
    private void Start()
    {

    }

    void Update()
    {
        // if (Input.GetMouseButtonDown(0))
        // {
        //     previousPosition = cam.ScreenToViewportPoint(Input.mousePosition);
        //     manual_cam =! manual_cam;
        // }
    }

    void LateUpdate()
    {
        UpdateFocusPoint();
        Quaternion lookRotation;
        
        if (ManualRotation() || autoRotation() )
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
        if ( focusRadius > 0f)
        {
            float distance = Vector3.Distance(targetPoint, focusPoint);
            float t = 1f;
            if (distance > 0.01f && focusCentering > 0f)
            { t = Mathf.Pow(1f-focusCentering, Time.unscaledDeltaTime); }
            if (distance > focusRadius)
            {
                t = Mathf.Min(t, focusRadius / distance );
            }
            focusPoint = Vector3.Lerp(targetPoint, focusPoint, t);
        } else {
            focusPoint = targetPoint;
        }
    }

    void OnValidate()
    {
        if (maxVerticalAngle < minVerticalAngle) {
            maxVerticalAngle = minVerticalAngle;
        }
    }

    void constrainAngles()
    {
        orbitAngles.x = Mathf.Clamp( orbitAngles.x, minVerticalAngle, maxVerticalAngle);
        if (orbitAngles.y < 0f)
        {
            orbitAngles.y += 360f;
        } else if ( orbitAngles.y >= 360f)
        {
            orbitAngles.y -= 360f;
        }
    }


    bool ManualRotation () {
		Vector2 input = new Vector2(
			Input.GetAxis("Mouse Y") * (-1),
			Input.GetAxis("Mouse X")
		);
		const float e = 0.001f;
		if (input.x < -e || input.x > e || input.y < -e || input.y > e) {
			orbitAngles += rotationSpeed * Time.unscaledDeltaTime * input;
            lastManualRotationTime = Time.unscaledTime;
            return true;
		}
        return false;
	}

    bool autoRotation()
    {
        if (Time.unscaledTime - lastManualRotationTime < alignDelay )
            return false;
        
        Vector2 movement = new Vector2( focusPoint.x - previousFocusPoint.x, focusPoint.z - previousFocusPoint.z);
        float movementDeltaSqr = movement.sqrMagnitude;
        if (movementDeltaSqr < 0.000001f )
        {
            return false;
        }
        float headingAngle = GetAngle(movement / Mathf.Sqrt(movementDeltaSqr));
        float deltaAbs = Mathf.Abs(Mathf.DeltaAngle(orbitAngles.y, headingAngle));
        float rotationChange = rotationSpeed * Mathf.Min(Time.unscaledDeltaTime, movementDeltaSqr);
        if (deltaAbs < alignSmoothRange) {
			rotationChange *= deltaAbs / alignSmoothRange;
		}
        else if (180f - deltaAbs < alignSmoothRange) {
			rotationChange *= (180f - deltaAbs) / alignSmoothRange;
        }
		orbitAngles.y = Mathf.MoveTowardsAngle(orbitAngles.y, headingAngle, rotationChange);
        return true;
    }

    static float GetAngle (Vector2 direction) {
		float angle = Mathf.Acos(direction.y) * Mathf.Rad2Deg;
        // (x < 0) => counterclockwise
		return direction.x < 0f ? 360f - angle : angle;
	}
    // OLD

    // private void auto()
    // {

    // }
    // private void manual()
    // {
    //         Vector3 newPosition = cam.ScreenToViewportPoint(Input.mousePosition);
    //         Vector3 direction = previousPosition - newPosition;
             
    //         float rotationAroundYAxis = -direction.x * 180;
    //         float rotationAroundXAxis = direction.y * 180;
            
    //         cam.transform.position = target.position;
            
    //         cam.transform.Rotate(new Vector3(1, 0, 0), rotationAroundXAxis);
    //         cam.transform.Rotate(new Vector3(0, 1, 0), rotationAroundYAxis, Space.World);
            
    //         cam.transform.Translate(new Vector3(0, 0, -distance));
            
    //         previousPosition = newPosition;
    // }
    // private float AngleOnXZPlane(Transform itemA, Transform itemB)
    //  {
    //     Vector3 direction = itemA.rotation * itemB.forward;
    //     return Mathf.Atan2(direction.x, direction.y) * Mathf.Rad2Deg;
    //  }

}