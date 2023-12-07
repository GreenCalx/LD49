using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Debug;

namespace Schnibble {
    #if SCH_SUSPENSION_V2
    public class testTransmission : MonoBehaviour
    {
        public List<WheelColliderBehavior> wheels;
        // Start is called before the first frame update
        void Start()
        {
        
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKey(KeyCode.J)){
                Log("press");
                // add torque to wheels
                foreach(var w in wheels){
                    w.wheelCollider.ApplyTorque(new Vector3(Time.deltaTime, 0, 0));
                }
            }
        }
    }
    #endif
}
