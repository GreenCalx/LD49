using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TSTSaveStates : MonoBehaviour
{
    private Vector3 ss_pos = Vector3.zero;
    private Quaternion ss_rot = Quaternion.identity;
    private bool hasSS;
    
    public KeyCode load;
    public KeyCode save;

    // Start is called before the first frame update
    void Start()
    {
        hasSS = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(load)) // load
        {
            if (!hasSS)
            {
                Debug.LogError("No save state to load");
                return;
            }
            Rigidbody rb2d = Access.Player().gameObject.GetComponentInChildren<Rigidbody>();
            if (!!rb2d)
            {
                rb2d.velocity           = Vector3.zero;
                rb2d.angularVelocity    = Vector3.zero;
            }
            Access.Player().gameObject.transform.position = ss_pos;
            Access.Player().gameObject.transform.rotation = ss_rot;

            Debug.Log("Savestate loaded");
        } else if (Input.GetKeyDown(save))
        {
            ss_pos = Access.Player().gameObject.transform.position;
            ss_rot = Access.Player().gameObject.transform.rotation;
            hasSS = true;
            Debug.Log("Savestate updated");
        }
    }
}
