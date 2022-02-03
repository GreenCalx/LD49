using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class ManualCamera : MonoBehaviour
{
    [SerializeField] private Camera cam;
    [SerializeField] private Transform target;
    [SerializeField] private float distanceToTarget = 10;
    
    private Vector3 previousPosition;

    private bool manual_cam = false;
    private float elapsed_time = 0f;
    public float manual_cam_duration = 2.0f;
    private Transform camTransform;

    private void Start()
    {
        camTransform = cam.transform;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            previousPosition = cam.ScreenToViewportPoint(Input.mousePosition);
            manual_cam =! manual_cam;
        }

        if (manual_cam)
            manual();
        else
            auto();
    }

    private void auto()
    {
        float playerAngle = AngleOnXZPlane(target);
        float cameraAngle = AngleOnXZPlane(camTransform);
        // difference in orientations
        float rotationDiff = Mathf.DeltaAngle(cameraAngle, playerAngle);
        if ( Mathf.Abs(rotationDiff) > 9999f)
            transform.RotateAround( target.transform.position, Vector3.up, rotationDiff * Time.deltaTime);
    }
    private void manual()
    {
            Vector3 newPosition = cam.ScreenToViewportPoint(Input.mousePosition);
            Vector3 direction = previousPosition - newPosition;
             
            float rotationAroundYAxis = -direction.x * 180; // camera moves horizontally
            float rotationAroundXAxis = direction.y * 180; // camera moves vertically
            
            cam.transform.position = target.position;
            
            cam.transform.Rotate(new Vector3(1, 0, 0), rotationAroundXAxis);
            cam.transform.Rotate(new Vector3(0, 1, 0), rotationAroundYAxis, Space.World); // <— This is what makes it work!
            
            cam.transform.Translate(new Vector3(0, 0, -distanceToTarget));
            
            previousPosition = newPosition;
    }
    private float AngleOnXZPlane(Transform item)
     {
        // get rotation as vector (relative to parent)
        Vector3 direction = item.rotation * item.parent.forward;

        // return angle in degrees when projected onto xz plane
        return Mathf.Atan2(direction.x, direction.y) * Mathf.Rad2Deg;
     }

}