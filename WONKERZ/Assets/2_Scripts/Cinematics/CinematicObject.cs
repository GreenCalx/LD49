using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CinematicObject : MonoBehaviour
{
    
    public void makeVisible()
    {
        MeshRenderer mr = GetComponent<MeshRenderer>();
        if (!!mr) 
            mr.enabled = true;

        //Ground gnd = GetComponent<Ground>();
        //if (!!gnd) 
        //    gnd.enabled = true;

        MeshCollider mc = GetComponent<MeshCollider>();
        if (!!mc) 
            mc.enabled = true;
    }

    public void makeInvisible()
    {
        MeshRenderer mr = GetComponent<MeshRenderer>();
        if (!!mr) 
            mr.enabled = false;

        //Ground gnd = GetComponent<Ground>();
        //if (!!gnd) 
        //    gnd.enabled = false;

        MeshCollider mc = GetComponent<MeshCollider>();
        if (!!mc) 
            mc.enabled = false;
    }

}
