// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;

// public class FollowPlayerHUB : MonoBehaviour
// {
//     // camera will follow this object
//     public Transform Target;
//     //camera transform
//     public GameObject cam;
//     private Transform camTransform;
//     // offset between camera and target
//     public Vector3 Offset;
//     // change this value to get desired smoothness
//     public float SmoothTime = 0.3f;

//     // This value will change at the runtime depending on target movement. Initialize with zero vector.
//     private Vector3 velocity = Vector3.zero;

//     private bool manual_cam = false;
//     private float elapsed_time = 0f;
//     public float manual_cam_duration = 2.0f;

//     private Vector3 previousPosition;

//     private void Start()
//     {
//         camTransform = cam.transform;

//         Offset = camTransform.position - Target.position;
//         Vector3 targetPosition = Target.position + Offset;
//         camTransform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, SmoothTime);
//         transform.LookAt(Target);

//         previousPosition = transform.position;
//     }

//     private void Update()
//     {

//         if (Input.GetMouseButtonDown(0))
//         {
//             previousPosition = cam.ScreenToViewportPoint(Input.mousePosition);
//         }
//         else if (Input.GetMouseButton(0))
//         {
//             Vector3 currentPosition = cam.ScreenToViewportPoint(Input.mousePosition);
//             Vector3 direction = previousPosition - currentPosition;
            
//             float rotationAroundYAxis = -direction.x * 180; // camera moves horizontally
//             float rotationAroundXAxis = direction.y * 180; // camera moves vertically
            
//             previousPosition = currentPosition;
//         }
//         /*
//         if( manual_cam ) // manual cam
//         {
//             Debug.Log("MANUAL CAM");
//             camTransform.RotateAround(Target.transform.position, 
//                                      camTransform.up,
//                                      -Input.GetAxis("Mouse X")*5);

//             camTransform.RotateAround(Target.transform.position, 
//                                      camTransform.right,
//                                      -Input.GetAxis("Mouse Y")*5);

//         } else {
            
//             Debug.Log("AUTO CAM");

//             float playerAngle = AngleOnXZPlane(Target);
//             float cameraAngle = AngleOnXZPlane(camTransform);

//             // difference in orientations
//             float rotationDiff = Mathf.DeltaAngle(cameraAngle, playerAngle);

//             if ( Mathf.Abs(rotationDiff) > 9999f)
//                 transform.RotateAround( Target.transform.position, Vector3.up, rotationDiff * Time.deltaTime);
        
//         }*/
//     }

//     private float AngleOnXZPlane(Transform item)
//     {

//     // get rotation as vector (relative to parent)
//     Vector3 direction = item.rotation * item.parent.forward;

//     // return angle in degrees when projected onto xz plane
//     return Mathf.Atan2(direction.x, direction.y) * Mathf.Rad2Deg;
//     }
// }
