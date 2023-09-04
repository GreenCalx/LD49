using UnityEngine;
using System.Collections.Generic;

public class Dummy : MonoBehaviour
{

    public enum FLIP { NONE, FRONT, BACK };

    public bool rec_rot = false;
    public bool reset_rec_rot = true;

    public float n_flips_front = 0f;
    public float n_flips_back = 0f;

    public Vector3 crossP_result = Vector3.zero;
    public Vector3 refFwd = Vector3.zero;
    public FLIP currFlip = FLIP.NONE;
    public FLIP prevFlip = FLIP.NONE;
    private float epsilon = 0.001f; // pb : ca peut creer un decalage over timu
    public float xEpsilon = 0f;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (reset_rec_rot)
        {
            n_flips_front = 0f;
            n_flips_back = 0f;
            reset();
        }

        if (rec_rot)
        {
            record();
        }

        Debug.DrawRay(transform.position, transform.up * 10, Color.green, 0.1f);
        Debug.DrawRay(transform.position, transform.forward * 10, Color.blue, 0.1f);
        Debug.DrawRay(transform.position, transform.right * 10, Color.red, 0.1f);
    }

    void reset()
    {
        reset_rec_rot = false;
        refFwd = transform.forward;
        currFlip = FLIP.NONE;
        prevFlip = FLIP.NONE;
    }

    private void record()
    {

        crossP_result = Vector3.Cross(refFwd, transform.forward);
        
        if (crossP_result.x > 0 ) { currFlip = FLIP.FRONT; }
        if (crossP_result.x < 0 ) { currFlip = FLIP.BACK; }

        if (prevFlip!=FLIP.NONE && (currFlip!=prevFlip))
        {
            reset();
            return;
        }

        float yEpsilon = crossP_result.y / 2; // tilting on Y siminish the crossP result range
        float zEpsilon = 0f; // axes of forward, doesnt change dot prod
        
        xEpsilon = epsilon + Mathf.Abs(yEpsilon);
        if (currFlip == FLIP.FRONT)
        {
            if (crossP_result.x > (1 - xEpsilon))
            {
                n_flips_front+=0.25f;
                refFwd = transform.forward;
            }
        }
        if (currFlip == FLIP.BACK)
        {
            if (crossP_result.x < (-1 + xEpsilon) )
            {
                n_flips_back+=0.25f;
                refFwd = transform.forward;
            }            
        }
        prevFlip = currFlip;
    }



    float GetXDegrees(Transform t) 
    {
        // Get the angle about the world x axis in range -pi to +pi,
        // with 0 corresponding to a 180 degree rotation.
        var radians = Mathf.Atan2(t.forward.y, -t.forward.z);

        // Map to range from 0 to 360 degrees,
        // with 0 corresponding to no rotation.
        return 180 + radians * Mathf.Rad2Deg;
    }

}
