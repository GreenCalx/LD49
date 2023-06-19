using UnityEngine;
using System.Collections.Generic;

public class Dummy : MonoBehaviour
{

    public bool rec_rot = false;


    Quaternion          q0 = Quaternion.identity;
    List<Quaternion>    rec = new List<Quaternion>();



    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (rec_rot)
        {
            record();
        }
    }

    private void record()
    {
        float angle =0f;
        Vector3 axis = Vector3.zero;
        transform.rotation.ToAngleAxis(out angle, out axis);

        Debug.Log( "angle : " + angle + " axis : " + axis);
        Debug.DrawRay(transform.position, Vector3.forward*10, Color.green);
    }

}
