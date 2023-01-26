using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugCollision : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void FixedUpdate(){ // check for overlap?
            var overlap = Physics.OverlapBox(transform.position,
                                             new Vector3(0.5f, 1, 1),
                                             transform.rotation,
                                             ~(1 << LayerMask.NameToLayer("No Player Collision")));
            if (overlap.Length != 0)
            {
                Debug.Log(overlap[0].name);
                // IMPORTANT : Physx treat mesh collider as hollow surface : if center of overlap is under, triangle will be culled and
                // no penetration will be found........
                Vector3 PenetrationCorrectionDirection = Vector3.zero;
                float PenetrationCorrectionDistance = 0f;
                if (Physics.ComputePenetration(GetComponent<MeshCollider>(),
                                              transform.position,
                                               transform.rotation,
                                              overlap[0],
                                              overlap[0].transform.position,
                                              overlap[0].transform.rotation,
                                              out PenetrationCorrectionDirection, out PenetrationCorrectionDistance))
                //    BoxCollider bc = gameObject.AddComponent<BoxCollider>();
                //    bc.center = transform.InverseTransformPoint(WheelPosition);
                //    bc.size = new Vector3(S.Wheel.Width * 2, S.Wheel.Radius * 2, S.Wheel.Radius * 2);
                //    Debug.DrawLine(WheelPosition, WheelPosition + transform.up * 2);
                //    if (Physics.ComputePenetration(bc, WheelPosition, transform.rotation,
                //                                   overlap[0], overlap[0].transform.position, overlap[0].transform.rotation,
                //                                   out Result.PenetrationCorrectionDirection, out Result.PenetrationCorrectionDistance))
                {
                    Debug.Log("overlap and penetration   ");
                    Debug.DrawLine(transform.position, transform.position + (PenetrationCorrectionDirection * PenetrationCorrectionDistance));
                }
                else
                {
                    Debug.Log("overlap but no penetration?" + "    " + overlap.Length);
                }
                //GameObject.DestroyImmediate(bc);

            }
    }
}
