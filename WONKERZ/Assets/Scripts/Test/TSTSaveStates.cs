using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class TSTSaveStates : MonoBehaviour
{
    private Vector3 ss_pos = Vector3.zero;
    private Quaternion ss_rot = Quaternion.identity;
    private bool hasSS;
    
    public Transform startPortal;
    public KeyCode load;
    public KeyCode save;

    [Serializable]
    public struct ESS
    {
        public KeyCode k;
        public Transform t;
    }
    public List<ESS> ess;

    // Start is called before the first frame update
    void Start()
    {
        hasSS = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(load) || pollExtraSaveStates()) // load
        {
            loadState();
        } else if (Input.GetKeyDown(save))
        {
            ss_pos = Access.Player().gameObject.transform.position;
            ss_rot = Access.Player().gameObject.transform.rotation;
            hasSS = true;
            Debug.Log("Savestate updated");
        }
    }

    public bool pollExtraSaveStates()
    {
        if (ess == null)
            return false;

        foreach (ESS e in ess)
        {
            if (Input.GetKeyDown(e.k))
            {
                Transform t = e.t;
                if (t==null)
                {
                    Debug.LogError("No transform could be found for given KeyCode : " + e.k);
                    return false;
                }
                ss_pos = t.position;
                ss_rot = t.rotation;
                return true;
            }
        }
        return false;
    }

    public void loadState()
    {
        if (!hasSS)
        {
            Debug.LogError("No save state to load. Loading start portal.");
            ss_pos = startPortal.position;
            ss_rot = Quaternion.identity;
            hasSS = true;
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
    }
}
