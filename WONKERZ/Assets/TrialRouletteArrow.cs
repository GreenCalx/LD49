using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrialRouletteArrow : MonoBehaviour
{
    public void GetPointedTrial()
    {

    }

    void Update()
    {
        Debug.DrawRay(transform.position, transform.forward * 20f, Color.red );
    }
}
